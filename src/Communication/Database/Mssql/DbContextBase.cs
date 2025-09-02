using System;
using Microsoft.EntityFrameworkCore;

namespace HYSoft.Communication.Database.Mssql
{
    /// <summary>
    ///     MSSQL 데이터베이스 연결 및 모델 구성을 위한 추상 DbContext 기본 클래스입니다.
    ///     파생 클래스는 <see cref="ConfigureModel(ModelBuilder)"/>를 구현하여 모델 매핑을 정의해야 합니다.
    /// </summary>
    public abstract class DbContextBase : DbContext
    {
        private static DbContextOptions<DbContextBase>? _options;

        /// <summary>
        ///     기본 생성자입니다. 
        ///     <see cref="Initialize"/>가 먼저 호출되지 않으면 예외가 발생합니다.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     아직 <see cref="Initialize"/>가 호출되지 않은 경우 발생합니다.
        /// </exception>
        protected DbContextBase()
            : base(_options ?? throw new InvalidOperationException("먼저 Initialize를 호출해야 합니다."))
        {
        }

        /// <summary>
        ///     MSSQL 연결 문자열을 구성하고 내부 <see cref="DbContextOptions{TContext}"/>를 초기화합니다.
        ///     애플리케이션 시작 시 한 번만 호출하는 것을 권장합니다.
        /// </summary>
        /// <param name="server">MSSQL 서버 주소</param>
        /// <param name="port">MSSQL 서버 포트</param>
        /// <param name="uid">데이터베이스 사용자 ID</param>
        /// <param name="pwd">데이터베이스 비밀번호</param>
        /// <param name="database">연결할 데이터베이스 이름</param>
        public static void Initialize(string server, string port, string uid, string pwd, string database)
        {
            var cs = $"server={server},{port};uid={uid};pwd={pwd};database={database};Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
            var builder = new DbContextOptionsBuilder<DbContextBase>();
            builder.UseSqlServer(cs);
            _options = builder.Options;
        }

        /// <summary>
        ///     파생 클래스에서 엔터티 모델을 구성할 때 구현해야 하는 메서드입니다.
        ///     <see cref="OnModelCreating(ModelBuilder)"/>에서 호출됩니다.
        /// </summary>
        /// <param name="modelBuilder">모델 구성을 위한 <see cref="ModelBuilder"/> 인스턴스</param>
        protected abstract void ConfigureModel(ModelBuilder modelBuilder);

        /// <inheritdoc />
        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureModel(modelBuilder);
        }
    }
}
