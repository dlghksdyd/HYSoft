# HYSoft.Data Library

MSSQL DbContext 기본 클래스. 네임스페이스: `HYSoft.Data.Mssql`. 의존성: EF 6.5.1.

## DbContextBase (abstract)

`System.Data.Entity.DbContext` 상속. Template Method 패턴.

### Initialize (static)

```csharp
public static void Initialize(
    string server, string port, string uid, string pwd, string database)
```

- 앱 시작 시 한 번 호출
- `lock(_initLock)` 스레드 안전, `volatile` 연결 문자열 저장
- 연결 속성: Encrypt=True, TrustServerCertificate=True, MARS=True

### 생성자 (protected)

```csharp
protected DbContextBase()
    : base(_connectionString
        ?? throw new InvalidOperationException("먼저 Initialize를 호출해야 합니다."))
```

### OnModelCreating (sealed)

```csharp
protected sealed override void OnModelCreating(DbModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    ConfigureModel(modelBuilder);  // 파생 클래스에 위임
}
```

### ConfigureModel (abstract)

```csharp
protected abstract void ConfigureModel(DbModelBuilder modelBuilder);
```

파생 클래스에서 엔티티 매핑, 관계, 제약 정의.

## 설계 패턴

| 패턴 | 적용 |
|------|------|
| Template Method | OnModelCreating → ConfigureModel |
| Singleton (연결 문자열) | static volatile + lock |
| 한 번 쓰기 | 초기화 후 불변 |

## 스레드 안전

volatile + lock 초기화. 한번 설정된 문자열은 수정 불가.

하위: [Usage](01-Usage.md)
