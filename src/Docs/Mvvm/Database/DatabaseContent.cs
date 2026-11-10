using Docs.Mvvm.Shared;

namespace Docs.Mvvm.Database
{
    /// <summary>
    /// DB Support 페이지가 보여 주는 표·코드 예시. 내용은 HYSoft.Data 의 실제 코드
    /// (Data/Mssql/DbContextBase.cs, EF Core 9.0.8 · net8.0-windows)가 기준이다.
    /// </summary>
    public static class DatabaseContent
    {
        public static IReadOnlyList<MemberRow> Members { get; } = new[]
        {
            new MemberRow("abstract class DbContextBase : DbContext", "Microsoft.EntityFrameworkCore.DbContext base for SQL Server. Derive one context per database model."),
            new MemberRow("static void Initialize(string server, string port, string uid, string pwd, string database)", "Builds the SQL Server options shared by every DbContextBase-derived context. Call once at startup."),
            new MemberRow("protected DbContextBase()", "Uses the options from Initialize. Throws InvalidOperationException when Initialize has not been called."),
            new MemberRow("protected abstract void ConfigureModel(ModelBuilder modelBuilder)", "Map entities, keys, relations and constraints here."),
            new MemberRow("protected sealed override void OnModelCreating(ModelBuilder modelBuilder)", "Calls base.OnModelCreating, then ConfigureModel. Sealed - override ConfigureModel instead."),
        };

        public const string ConnectionString =
"""
server={server},{port};uid={uid};pwd={pwd};database={database};
Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
""";

        public const string DefineContext =
"""
using HYSoft.Data.Mssql;
using Microsoft.EntityFrameworkCore;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

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
""";

        public const string InitializeAtStartup =
"""
// App.OnStartup - once, before the first context is created.
// Read the password from protected configuration, never from source code.
DbContextBase.Initialize(
    server: settings.Server,
    port: settings.Port,          // string, e.g. "1433"
    uid: settings.User,
    pwd: settings.Password,
    database: settings.Database);
""";

        public const string QueryAndSave =
"""
// One short-lived context per unit of work. DbContext is not thread-safe.
using (var db = new AppDbContext())
{
    List<User> active = await db.Users
        .Where(u => u.IsActive)
        .AsNoTracking()
        .ToListAsync();

    db.Users.Add(new User { Name = "Hong", IsActive = true });
    await db.SaveChangesAsync();
}
""";
    }
}
