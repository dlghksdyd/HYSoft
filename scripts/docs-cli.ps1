<#
.SYNOPSIS
    HYSoft Docs 앱 조종 CLI (AI 전용). 앱이 --cli 로 연 Named Pipe 에 명령 한 줄을 보내고 JSON 응답 한 줄을 출력한다.

.DESCRIPTION
    launch                 Docs.exe 를 --cli --background 로 띄우고 준비될 때까지 기다린다(창은 화면 밖, 포커스 없음).
    <명령> [인자...]        앱에 명령을 보낸다. 목록은 'help'. 예) area comm / open "Live Echo Demo" / echo.send 안녕
    quit                   앱을 닫고 프로세스가 끝났는지 확인한다.

    종료 코드: 0 = ok, 1 = 앱이 ok=false 로 답함, 2 = 대상 앱 없음·여러 개, 3 = 통신 실패·시간 초과.
    대상 앱은 -TargetPid 로 고르고, 생략하면 채널이 열린 Docs 프로세스가 정확히 하나일 때 그것을 쓴다.

.EXAMPLE
    pwsh -File scripts/docs-cli.ps1 launch
    pwsh -File scripts/docs-cli.ps1 area comm
    pwsh -File scripts/docs-cli.ps1 shot C:\temp\docs.png
#>
param(
    [Parameter(Position = 0)] [string] $Command = 'help',
    [Parameter(Position = 1, ValueFromRemainingArguments = $true)] [string[]] $CommandArgs = @(),
    [int] $TargetPid = 0,
    [int] $TimeoutSec = 30,
    [string] $ExePath = ''
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PIPE_PREFIX = 'HYSoft.Docs.Cli.'

function Write-Result([bool] $isOk, [string] $errorText, $payload) {
    $result = [ordered]@{ ok = $isOk }
    if ($errorText) { $result.error = $errorText }
    if ($null -ne $payload) { $result.data = $payload }
    $result | ConvertTo-Json -Depth 10 -Compress
}

function Get-CliPipeIds {
    # 채널이 열린 Docs 프로세스 PID 목록(파이프 이름의 PID 가 살아 있는 Docs 인 것만).
    $pipeIds = [System.IO.Directory]::GetFiles('\\.\pipe\') |
        ForEach-Object { [System.IO.Path]::GetFileName($_) } |
        Where-Object { $_.StartsWith($PIPE_PREFIX) } |
        ForEach-Object { [int]($_.Substring($PIPE_PREFIX.Length)) } |
        Sort-Object -Unique
    @($pipeIds | Where-Object { (Get-Process -Id $_ -ErrorAction SilentlyContinue).ProcessName -eq 'Docs' })
}

function Resolve-TargetPid {
    if ($TargetPid -gt 0) { return $TargetPid }
    $candidates = Get-CliPipeIds
    if ($candidates.Count -eq 1) { return $candidates[0] }
    if ($candidates.Count -eq 0) {
        Write-Result $false 'No Docs app with an open CLI channel. Run: docs-cli.ps1 launch' $null
        exit 2
    }
    Write-Result $false ("Several Docs apps have a CLI channel - pass -TargetPid. Candidates: " + ($candidates -join ', ')) $null
    exit 2
}

function Send-CliRequest([int] $processId, [string] $cmdName, [string[]] $cmdArgs, [int] $timeoutMs) {
    $requestJson = [ordered]@{ cmd = $cmdName; args = [string[]]@($cmdArgs) } | ConvertTo-Json -Compress -Depth 3
    $pipeClient = [System.IO.Pipes.NamedPipeClientStream]::new('.', "$PIPE_PREFIX$processId",
        [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::Asynchronous)
    try {
        $pipeClient.Connect([Math]::Min($timeoutMs, 5000))
        $requestBytes = [System.Text.Encoding]::UTF8.GetBytes($requestJson + "`n")
        $pipeClient.Write($requestBytes, 0, $requestBytes.Length)
        $pipeClient.Flush()

        $responseBytes = [System.Collections.Generic.List[byte]]::new()
        $oneByte = [byte[]]::new(1)
        $deadline = [DateTime]::UtcNow.AddMilliseconds($timeoutMs)
        while ($true) {
            $remainingMs = [int]($deadline - [DateTime]::UtcNow).TotalMilliseconds
            if ($remainingMs -le 0) { throw [TimeoutException]::new("No response within $([int]($timeoutMs / 1000)) s.") }
            $readTask = $pipeClient.ReadAsync($oneByte, 0, 1)
            if (-not $readTask.Wait($remainingMs)) { throw [TimeoutException]::new("No response within $([int]($timeoutMs / 1000)) s.") }
            if ($readTask.Result -eq 0 -or $oneByte[0] -eq 10) { break }
            $responseBytes.Add($oneByte[0])
        }
        return [System.Text.Encoding]::UTF8.GetString($responseBytes.ToArray())
    }
    finally {
        $pipeClient.Dispose()
    }
}

function Invoke-AndPrint([int] $processId, [string] $cmdName, [string[]] $cmdArgs) {
    try {
        $responseLine = Send-CliRequest $processId $cmdName $cmdArgs ($TimeoutSec * 1000)
    }
    catch {
        Write-Result $false "Channel error (pid $processId): $($_.Exception.Message)" $null
        exit 3
    }
    if (-not $responseLine) {
        Write-Result $false "Empty response (pid $processId)." $null
        exit 3
    }
    $responseLine
    $parsed = $responseLine | ConvertFrom-Json
    if ($parsed.ok) { return } else { exit 1 }
}

switch ($Command.ToLowerInvariant()) {
    'launch' {
        if (-not $ExePath) {
            $repoRoot = Split-Path -Parent $PSScriptRoot
            $ExePath = Join-Path $repoRoot 'src\Docs\bin\Debug\net8.0-windows\Docs.exe'
        }
        if (-not (Test-Path $ExePath)) {
            Write-Result $false "Docs.exe not found: $ExePath (build src/Docs/Docs.csproj first, or pass -ExePath)." $null
            exit 2
        }
        $launched = Start-Process -FilePath $ExePath -ArgumentList '--cli', '--background' -PassThru
        $readyDeadline = [DateTime]::UtcNow.AddSeconds($TimeoutSec)
        while ([DateTime]::UtcNow -lt $readyDeadline) {
            if ($launched.HasExited) {
                Write-Result $false "Docs exited during startup (exit code $($launched.ExitCode))." $null
                exit 3
            }
            try {
                $pingLine = Send-CliRequest $launched.Id 'ping' @() 2000
                $pingResult = $pingLine | ConvertFrom-Json
                if ($pingResult.ok -and $pingResult.data.ready) {
                    Write-Result $true $null ([ordered]@{ pid = $launched.Id; exe = $ExePath; started = $launched.StartTime.ToString('o') })
                    exit 0
                }
            }
            catch { }
            Start-Sleep -Milliseconds 300
        }
        Write-Result $false "Docs (pid $($launched.Id)) did not become ready within $TimeoutSec s. It is still running - inspect or stop it." ([ordered]@{ pid = $launched.Id })
        exit 3
    }
    'list' {
        Write-Result $true $null ([ordered]@{ pids = @(Get-CliPipeIds) })
        exit 0
    }
    'quit' {
        $processId = Resolve-TargetPid
        try { $null = Send-CliRequest $processId 'quit' @() 5000 }
        catch {
            Write-Result $false "Channel error (pid $processId): $($_.Exception.Message)" $null
            exit 3
        }
        # 「닫았다」는 응답이 아니라 프로세스 부재로 판정한다.
        $exitDeadline = [DateTime]::UtcNow.AddSeconds(10)
        while ([DateTime]::UtcNow -lt $exitDeadline -and (Get-Process -Id $processId -ErrorAction SilentlyContinue)) {
            Start-Sleep -Milliseconds 200
        }
        if (Get-Process -Id $processId -ErrorAction SilentlyContinue) {
            Write-Result $false "Docs (pid $processId) is still running 10 s after quit." $null
            exit 3
        }
        Write-Result $true $null ([ordered]@{ pid = $processId; exited = $true })
        exit 0
    }
    default {
        $processId = Resolve-TargetPid
        Invoke-AndPrint $processId $Command $CommandArgs
        exit 0
    }
}
