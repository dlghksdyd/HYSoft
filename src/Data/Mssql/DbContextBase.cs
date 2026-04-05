using System;
using System.Data.Entity;

namespace HYSoft.Data.Mssql
{
    /// <summary>
    ///     MSSQL 데이터베이스 연결 및 모델 구성을 위한 추상 DbContext 기본 클래스입니다.
    ///     파생 클래스는 <see cref="ConfigureModel(DbModelBuilder)"/>를 구현하여 모델 매핑을 정의해야 합니다.
    /// </summary>
    public abstract class DbContextBase : DbContext
    {
        private static volatile string? _connectionString;
        private static readonly object _initLock = new object();

        /// <summary>
        ///     기본 생성자입니다.
        ///     <see cref="Initialize"/>가 먼저 호출되지 않으면 예외가 발생합니다.
        /// </summary>
        protected DbContextBase()
            : base(_connectionString ?? throw new InvalidOperationException("먼저 Initialize를 호출해야 합니다."))
        {
        }

        /// <summary>
        ///     MSSQL 연결 문자열을 구성하고 내부 옵션을 초기화합니다.
        ///     애플리케이션 시작 시 한 번만 호출하는 것을 권장합니다.
        /// </summary>
        public static void Initialize(string server, string port, string uid, string pwd, string database)
        {
            lock (_initLock)
            {
                _connectionString = $"server={server},{port};uid={uid};pwd={pwd};database={database};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            }
        }

        /// <summary>
        ///     파생 클래스에서 엔터티 모델을 구성할 때 구현해야 하는 메서드입니다.
        /// </summary>
        protected abstract void ConfigureModel(DbModelBuilder modelBuilder);

        /// <inheritdoc />
        protected sealed override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureModel(modelBuilder);
        }
    }
}
