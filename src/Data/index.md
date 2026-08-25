# Data

MSSQL 데이터베이스 접근을 위한 추상 DbContext 기본 클래스.

- **네임스페이스**: `HYSoft.Data.Mssql`
- **의존성**: EntityFramework 6.5.1

## DbContextBase (abstract)

`System.Data.Entity.DbContext` 상속. Template Method 패턴.

```csharp
// 앱 시작 시 한 번 호출
public static void Initialize(string server, string port,
    string uid, string pwd, string database);

// 파생 클래스에서 구현
protected abstract void ConfigureModel(DbModelBuilder modelBuilder);
```

- 연결: Encrypt=True, TrustServerCertificate=True, MARS=True
- 스레드 안전: volatile + lock 초기화, 한 번 쓰기 패턴
- Initialize 미호출 시 InvalidOperationException

## 사용

```csharp
public class AppDbContext : DbContextBase
{
    public DbSet<User> Users { get; set; }
    protected override void ConfigureModel(DbModelBuilder mb)
    {
        mb.Entity<User>().ToTable("Users").HasKey(u => u.Id);
    }
}
```

## 참고

현재 솔루션 내 다른 프로젝트에서 직접 참조하지 않음. 인프라 코드.

## 상세 문서

[Data docs](../Docs/Documents/05-Data/README.md)
