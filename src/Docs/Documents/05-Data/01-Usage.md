# Data Library - Usage (EF Core 9)

## 1. 파생 DbContext 정의

```csharp
using HYSoft.Data.Mssql;
using Microsoft.EntityFrameworkCore;

public sealed class AppDbContext : DbContextBase
{
    public DbSet<User> Users => Set<User>();

    protected override void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).HasMaxLength(50).IsRequired();
        });
    }
}
```

## 2. 앱 시작 시 초기화

```csharp
DbContextBase.Initialize(
    server: settings.Server,
    port: settings.Port,        // 문자열, 예: "1433"
    uid: settings.User,
    pwd: settings.Password,     // 소스에 적지 말 것
    database: settings.Database);
```

## 3. 데이터 접근

```csharp
// 작업 단위마다 짧게 쓴다. DbContext 는 스레드 안전하지 않다.
using (var db = new AppDbContext())
{
    var active = await db.Users.Where(u => u.IsActive).AsNoTracking().ToListAsync();
    db.Users.Add(new User { Name = "홍길동" });
    await db.SaveChangesAsync();
}
```

## 참고

솔루션 내 다른 프로젝트는 직접 참조하지 않는다. 비즈니스 로직 프로젝트에서 파생 DbContext 를 구현해 쓴다.
Docs 앱 › DB Support › DbContextBase 페이지에 같은 내용이 있다.
