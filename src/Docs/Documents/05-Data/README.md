# HYSoft.Data Library

MSSQL DbContext 기본 클래스. 네임스페이스: `HYSoft.Data.Mssql`.
의존성: Microsoft.EntityFrameworkCore / .SqlServer **9.0.8** (EF Core, net8.0-windows). EF6 아님.

## DbContextBase (abstract)

`Microsoft.EntityFrameworkCore.DbContext` 상속. Template Method 패턴.

### Initialize (static)

```csharp
public static void Initialize(
    string server, string port, string uid, string pwd, string database)
```

- 앱 시작 시 한 번 호출. `DbContextOptions` 를 만들어 **static 필드 하나**에 저장
- 연결 속성: Encrypt=True, TrustServerCertificate=True, MultipleActiveResultSets=True
- 잠금·한 번 쓰기 보장 없음 — 다시 부르면 그 뒤에 만드는 컨텍스트부터 새 설정을 쓴다

### 생성자 (protected)

```csharp
protected DbContextBase()
    : base(_options ?? throw new InvalidOperationException("먼저 Initialize를 호출해야 합니다."))
```

### OnModelCreating (sealed) / ConfigureModel (abstract)

```csharp
protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    ConfigureModel(modelBuilder);  // 파생 클래스에 위임
}

protected abstract void ConfigureModel(ModelBuilder modelBuilder);
```

EF Core 는 컨텍스트 형식마다 모델을 한 번 만들어 캐시한다 — ConfigureModel 은 형식당 1회 실행.

## 주의

| 항목 | 내용 |
|------|------|
| DB 개수 | 프로세스당 1개(모든 파생 컨텍스트가 같은 옵션 공유) |
| 인증서 | TrustServerCertificate=True — 서버 인증서를 검증하지 않음. 신뢰된 망에서만 |
| 비밀번호 | 연결 문자열에 들어간다 — 보호된 설정에서 읽고 로그에 남기지 말 것 |

하위: [Usage](01-Usage.md)
