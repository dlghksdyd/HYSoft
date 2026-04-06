# Data Library - Usage

## 1. 파생 DbContext 정의

```csharp
public class AppDbContext : DbContextBase
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void ConfigureModel(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .ToTable("Users")
            .HasKey(u => u.Id);

        modelBuilder.Entity<Product>()
            .ToTable("Products")
            .HasMany(p => p.Categories);
    }
}
```

## 2. 앱 시작 시 초기화

```csharp
DbContextBase.Initialize(
    server: "sqlserver.example.com",
    port: "1433",
    uid: "app_user",
    pwd: "password",
    database: "MyAppDb"
);
```

## 3. 데이터 접근

```csharp
using (var db = new AppDbContext())
{
    var users = db.Users.Where(u => u.IsActive).ToList();
    db.Users.Add(new User { Name = "홍길동" });
    db.SaveChanges();
}
```

## 참고

현재 솔루션 내 다른 프로젝트에서 직접 참조하지 않음.
비즈니스 로직 프로젝트에서 파생 DbContext를 구현하여 사용.
