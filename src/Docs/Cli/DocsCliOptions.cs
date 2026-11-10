namespace Docs.Cli
{
    /// <summary>
    /// 실행 인자에서 읽은 CLI 채널 설정. 앱 시작 때 한 번 정해지고 바뀌지 않는다.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>--cli</c>: AI 용 Named Pipe 채널을 연다. 없으면 채널은 열리지 않는다(사람이 그냥 실행한 경우).</item>
    /// <item><c>--background</c>: 창을 화면 밖에, 작업 표시줄 없이, 포커스를 가져가지 않고 띄운다 —
    /// AI 가 시험하는 동안 사람의 다른 작업을 가리거나 방해하지 않게 한다.</item>
    /// </list>
    /// </remarks>
    public sealed class DocsCliOptions
    {
        public static DocsCliOptions Current { get; private set; } = new DocsCliOptions(false, false);

        public DocsCliOptions(bool enableCli, bool background)
        {
            EnableCli = enableCli;
            Background = background;
        }

        public bool EnableCli { get; }

        public bool Background { get; }

        /// <summary>인자를 읽어 <see cref="Current"/> 로 고정한다. App.OnStartup 에서 창이 만들어지기 전에 부른다.</summary>
        public static DocsCliOptions Initialize(IEnumerable<string> args)
        {
            bool enableCli = false;
            bool background = false;
            foreach (var arg in args)
            {
                if (string.Equals(arg, "--cli", StringComparison.OrdinalIgnoreCase)) enableCli = true;
                else if (string.Equals(arg, "--background", StringComparison.OrdinalIgnoreCase)) background = true;
            }

            Current = new DocsCliOptions(enableCli, background);
            return Current;
        }
    }
}
