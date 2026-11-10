# Data

MSSQL 데이터베이스 접근을 위한 추상 DbContext 기본 클래스.

- **네임스페이스**: `HYSoft.Data.Mssql`
- **의존성**: Microsoft.EntityFrameworkCore / .SqlServer 9.0.8 (EF Core — EF6 아님)

## DbContextBase (abstract)

`Microsoft.EntityFrameworkCore.DbContext` 상속. Template Method 패턴.

```csharp
// 앱 시작 시 한 번 호출 — DbContextOptions 를 static 필드 하나에 저장
public static void Initialize(string server, string port,
    string uid, string pwd, string database);

// 파생 클래스에서 구현 (OnModelCreating 은 sealed)
protected abstract void ConfigureModel(ModelBuilder modelBuilder);
```

- 연결: Encrypt=True, TrustServerCertificate=True(인증서 미검증), MARS=True
- 프로세스당 DB 1개. 잠금·한 번 쓰기 보장 없음 — 다시 부르면 이후 컨텍스트부터 새 설정
- Initialize 미호출 시 생성자에서 InvalidOperationException

## 사용

```csharp
public sealed class AppDbContext : DbContextBase
{
    public DbSet<User> Users => Set<User>();
    protected override void ConfigureModel(ModelBuilder mb)
    {
        mb.Entity<User>(e => { e.ToTable("Users"); e.HasKey(u => u.Id); });
    }
}
```

## 참고

현재 솔루션 내 다른 프로젝트에서 직접 참조하지 않음. 인프라 코드.

## 상세 문서

[Data docs](../Docs/Documents/05-Data/README.md)
