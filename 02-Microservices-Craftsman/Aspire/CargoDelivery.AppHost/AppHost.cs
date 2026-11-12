// ====================================================================================
// 🚀 .NET 10 ASPIRE APPHOST - ГОЛОВНИЙ ДИРИГЕНТ ОРКЕСТРАЦІЇ РОЗПОДІЛЕНОЇ СИСТЕМИ
// ------------------------------------------------------------------------------------
// Замість складного docker-compose.yaml та розрізнених скриптів, AppHost описує всю
// топологію проєкту чистою, типізованою мовою C#:
// 1. Контейнери інфраструктури (RabbitMQ Broker, MS SQL Server).
// 2. Бази даних для кожного ізольованого Bounded Context.
// 3. Мікросервісні Web API (.NET 10).
// 4. Декларативні залежності та усунення Race Conditions через .WaitFor().
// 5. Автоматичне впровадження Service Discovery та Connection Strings.
// ====================================================================================

var builder = DistributedApplication.CreateBuilder(args);

// ====================================================================================
// 🐰 1. ІНФРАСТРУКТУРА: BROKER RABBITMQ (AMQP 0-9-1)
// ------------------------------------------------------------------------------------
// Aspire автоматично завантажує Docker-образ RabbitMQ (rabbitmq:4.x-management) і публікує:
// - AMQP-ендпоінт (всередині контейнера 5672) — для бінарних фреймів повідомлень MassTransit;
// - HTTP-ендпоінт (всередині контейнера 15672) — веб-панель адміністрування (WithManagementPlugin).
// Зовнішні порти на хості призначаються ДИНАМІЧНО — дивіться колонку URLs у дашборді.
// Логін за замовчуванням — guest, а пароль Aspire ГЕНЕРУЄ і зберігає як параметр
// "rabbitmq-password" (вкладка Parameters у дашборді, user secrets AppHost).
// Залежні сервіси отримують готовий рядок ConnectionStrings__rabbitmq
// (amqp://guest:{пароль}@localhost:{порт}) через .WithReference(rabbitmq).
// ====================================================================================
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ====================================================================================
// 💾 2. ІНФРАСТРУКТУРА: РЕЛЯЦІЙНА БАЗА ДАНИХ MS SQL SERVER 2022
// ------------------------------------------------------------------------------------
// Патерн "Database-per-Microservice": кожен Bounded Context має власну фізичну або
// логічну БД, унеможливлюючи несанкціонований доступ до чужих таблиць напряму через SQL.
//
// ⚠️ ВАЖЛИВО: імена ресурсів в Aspire РЕГІСТРОНЕЗАЛЕЖНІ та мають бути унікальними.
// Якщо назвати базу "CargoOrderingManager", а проєкт — "cargoorderingmanager", AppHost
// впаде на старті з DistributedApplicationException ("resource ... already exists").
// Тому ресурси БД мають суфікс "-db", а очікуване сервісом ім'я рядка підключення
// (ConnectionStrings:CargoOrderingManager тощо) задається явно через connectionName
// у .WithReference(...) нижче.
//
// Другий аргумент AddDatabase — фізична назва БД всередині SQL Server. Якщо БД ще
// не існує, Aspire створює її сам; таблиці потім створюють EF Core міграції
// (MigrationHostedService у кожному мікросервісі).
// ====================================================================================
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("driversmanagement-db", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("driversblogmanagement-db", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("cargoorderingmanager-db", "dev_cargoorderingmanager");
var authJwtDb = sqlserver.AddDatabase("auth-jwt-db", "DriversManagementJWT");

// ====================================================================================
// 🛡️ 2A. ПІДСИСТЕМА БЕЗПЕКИ (03-Identity-Security) — ОДНОЧАСНИЙ ЗАПУСК 3 ПРОВАЙДЕРІВ
// ------------------------------------------------------------------------------------
// Всі три модулі автентифікації оркеструються Aspire одночасно:
//   • 03.1 [auth-simple-api]  — Custom JWT Bearer (AuthorizationModuleAPI + SQL Server)
//   • 03.2 [auth-duende]      — Duende IdentityServer (OIDC / OAuth 2.0 + PKCE)
//   • 03.3 [keycloak]         — Enterprise IAM Keycloak у контейнері + PostgreSQL
//                               (автоімпорт реалму DevRealmDrivers + федерація в Duende)
// ====================================================================================

// --- 03.3 Інфраструктура Keycloak (PostgreSQL + Контейнер Keycloak) ---
var keycloakPg = builder.AddPostgres("keycloak-pg")
    .WithDataVolume("keycloak-pg-data");
var keycloakDb = keycloakPg.AddDatabase("keycloakdb", "keycloak");

var keycloakImportDir = Path.GetFullPath(
    Path.Combine(builder.AppHostDirectory, "..", "..", "Keycloak"));

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.0")
    .WithHttpEndpoint(port: 5080, targetPort: 8080, name: "http")
    .WithBindMount(keycloakImportDir, "/opt/keycloak/data/import", isReadOnly: true)
    .WithArgs("start-dev", "--import-realm")
    .WithEnvironment("KC_HTTP_RELATIVE_PATH", "/auth")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_DB", "postgres")
    .WithEnvironment("KC_DB_URL", "jdbc:postgresql://keycloak-pg:5432/keycloak")
    .WithEnvironment("KC_DB_USERNAME", keycloakPg.Resource.UserNameReference)
    .WithEnvironment("KC_DB_PASSWORD", keycloakPg.Resource.PasswordParameter)
    .WaitFor(keycloakDb);

var keycloakRealmAuthority = ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/auth/realms/DevRealmDrivers");

// --- 03.1 Простий Custom JWT API (AuthorizationModuleAPI) ---
var simpleAuthApi = builder.AddProject<Projects.AuthorizationModuleAPI>("auth-simple-api")
    .WithReference(authJwtDb, connectionName: "WebApiJWTDatabase")
    .WithHttpHealthCheck("/health")
    .WaitFor(authJwtDb);

// --- 03.2 Duende IdentityServer (+ федерація з Keycloak 03.3) ---
var duendeAuth = builder.AddProject<Projects.DuendeIdentityInMemory>("auth-duende")
    .WithEnvironment("Keycloak__Authority", keycloakRealmAuthority)
    .WithHttpHealthCheck("/health")
    .WaitFor(keycloak);

// ====================================================================================
// 📦 3. МІКРОСЕРВІС ЗАМОВЛЕНЬ ТА ДОСТАВОК: CargoOrderingManager
// ------------------------------------------------------------------------------------
// - Володіє логістичним доменом (Order, Delivery, Carrier).
// - MassTransit Consumer для IDriverCreated / IDriverUpdated (актуалізація водіїв).
// - MassTransit Publisher для IOrderCreated.
// - Внутрішній REST-ендпоінт api/internal/deliveries для агрегаторів.
// ====================================================================================
var cargoApi = builder.AddProject<Projects.CargoOrderingManager>("cargoorderingmanager")
    // connectionName → змінна ConnectionStrings__CargoOrderingManager, яку читає ConnectionStringOptions сервісу
    .WithReference(cargoDb, connectionName: "CargoOrderingManager")
    .WithReference(rabbitmq)
    .WithEnvironment("Auth__Mode", "Multi")
    .WithEnvironment("Auth__SimpleJwt__Secret", "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM")
    .WithEnvironment("Auth__SimpleJwt__Issuer", "AuthorizationModuleAPI")
    .WithEnvironment("Auth__SimpleJwt__Audience", "cargodelivery.microservices")
    .WithEnvironment("Auth__Duende__Authority", duendeAuth.GetEndpoint("https"))
    .WithEnvironment("Auth__Keycloak__Authority", keycloakRealmAuthority)
    // Health probe для Aspire: ресурс вважається "Healthy" лише коли GET /health повертає 200.
    // Саме на цей стан чекають залежні ресурси у .WaitFor(cargoApi).
    .WithHttpHealthCheck("/health")
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 📝 4. МІКРОСЕРВІС БЛОГУ ВОДІЇВ: DriversBlogManagement
// ------------------------------------------------------------------------------------
// - Соціальний домен (Post, Comment, Like, Shadow Driver).
// - gRPC Server: реалізує DriverRpcService (HTTP/2 бінарний контракт).
// - MassTransit Consumer: асинхронно синхронізує Shadow Drivers.
// - HTTP REST API: публічні ендпоінти блогу + внутрішній api/internal/postaboutdrivers для агрегаторів.
// ====================================================================================
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb, connectionName: "DriversBlogManagement")
    .WithReference(rabbitmq)
    .WithEnvironment("Auth__Mode", "Multi")
    .WithEnvironment("Auth__SimpleJwt__Secret", "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM")
    .WithEnvironment("Auth__SimpleJwt__Issuer", "AuthorizationModuleAPI")
    .WithEnvironment("Auth__SimpleJwt__Audience", "cargodelivery.microservices")
    .WithEnvironment("Auth__Duende__Authority", duendeAuth.GetEndpoint("https"))
    .WithEnvironment("Auth__Keycloak__Authority", keycloakRealmAuthority)
    .WithHttpHealthCheck("/health")
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 🚚 5. ГОЛОВНИЙ СЕРВІС ВОДІЇВ ТА BFF АГРЕГАТОР: DriversManagement
// ------------------------------------------------------------------------------------
// - Володіє повним досьє водія (Driver Aggregate Root).
// - MassTransit Publisher: ініціює події створення та оновлення водія.
// - gRPC Client: викликає DriversBlogManagement напряму по HTTP/2 Protobuf.
// - HTTP Gateway Aggregator / BFF: паралельно опитує CargoOrderingManager та
//   DriversBlogManagement через Task.WhenAll для формування зведеного дашборду.
// - .WaitFor(...) гарантує, що сервіс стартує лише після повної готовності БД, брокера
//   та суміжних API, усуваючи неприємні стартові винятки Connection Refused!
// ====================================================================================
var driversApi = builder.AddProject<Projects.DriversManagement>("driversmanagement")
    .WithReference(driversDb, connectionName: "DriversManagement")
    .WithReference(rabbitmq)
    .WithReference(blogApi)
    .WithReference(cargoApi)
    .WithEnvironment("Auth__Mode", "Multi")
    .WithEnvironment("Auth__SimpleJwt__Secret", "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM")
    .WithEnvironment("Auth__SimpleJwt__Issuer", "AuthorizationModuleAPI")
    .WithEnvironment("Auth__SimpleJwt__Audience", "cargodelivery.microservices")
    .WithEnvironment("Auth__Duende__Authority", duendeAuth.GetEndpoint("https"))
    .WithEnvironment("Auth__Keycloak__Authority", keycloakRealmAuthority)
    .WithHttpHealthCheck("/health")
    .WaitFor(driversDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 🛡️ 6. GATEWAY AGGREGATOR ДИСПЕТЧЕРА: CargoDelivery.HttpAggregator (:5005)
// ------------------------------------------------------------------------------------
// - Патерн Scatter-Gather: паралельний Fan-Out / Fan-In через IHttpClientFactory.
// - Демонстрація OpenTelemetry Distributed Tracing (батьківські та дочірні спани).
// - Демонстрація Polly Resilience (Retry, Timeout, Circuit Breaker).
// - Не має власної БД чи брокера, тому стартує паралельно без блокування.
// ====================================================================================
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WithHttpHealthCheck("/health");

// ====================================================================================
// 🚪 7. ВХІДНИЙ API GATEWAY НА БАЗІ OCELOT: CargoDelivery.ApiGateway (:5000)
// ------------------------------------------------------------------------------------
// - Єдина вхідна точка (Reverse Proxy / Edge Gateway) для зовнішніх клієнтів.
// - Декларативна маршрутизація (/gateway/*), Rate Limiting та трансформація заголовків.
// - Вбудована агрегація Ocelot (/gateway/ocelot-aggregate/{id}) та кастомний
//   C#-агрегатор IDefinedAggregator (/gateway/custom-aggregate/{id}).
// ====================================================================================
var apiGateway = builder.AddProject<Projects.CargoDelivery_ApiGateway>("apigateway")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WithReference(httpAggregator)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
