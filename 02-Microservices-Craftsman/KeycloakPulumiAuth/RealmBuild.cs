namespace KeycloakPulumiAuth;

using KeycloakPulumiAuth.Extensions;
using KeycloakPulumiAuth.Factories;
using Pulumi;
using Pulumi.Keycloak;
using Pulumi.Keycloak.Inputs;

// ====================================================================================
// 🏢 МОДУЛЬ 03.3 (КЕЙС B): ДЕКЛАРАТИВНИЙ IAC ДЛЯ KEYCLOAK (PULUMI STACK)
// ------------------------------------------------------------------------------------
// На відміну від Кейсу A (автоімпорт realm-export.json в Aspire при старті з чистої БД),
// Pulumi керує живим Keycloak через Admin REST API як кодом (IaC):
//   • Керує окремим реалмом `DevRealmDrivers-Pulumi`, щоб не конфліктувати з `DevRealmDrivers`.
//   • Перший `pulumi up` створює базову конфігурацію (Realm, Scopes, Clients, Users).
//   • Повторний `pulumi up` (після розкоментування блоку UPDATE нижче) точково оновлює
//     живий реалм БЕЗ перезапуску контейнера та БЕЗ втрати існуючих користувачів.
// ====================================================================================
class RealmBuild : Stack
{
    public RealmBuild()
    {
        // 1. Створення окремого IaC-реалму `DevRealmDrivers-Pulumi`
        var realm = new Realm("DevRealmDrivers-Pulumi-realm", new RealmArgs
        {
            RealmName = "DevRealmDrivers-Pulumi",
            RegistrationAllowed = true,
            ResetPasswordAllowed = true,
            RememberMe = true,
            EditUsernameAllowed = true
        });

        // 2. Реєстрація кастомних OIDC-скоупів для кожного мікросервісу
        var driversmanagementScope = ScopeFactory.CreateScope(realm.Id, "drivers_management");
        var blogmanagementScope = ScopeFactory.CreateScope(realm.Id, "blog_management");
        var cargoOrderingScope = ScopeFactory.CreateScope(realm.Id, "cargo_ordering");

        // 3. Клієнти для DriversManagement (M2M, Postman, Swagger UI, BFF)
        // Client Credentials Flow — використовується для запитів сервер-сервер без логіну користувача
        var driversManagementPostmanMachineClient = ClientFactory.CreateClientCredentialsFlowClient(realm.Id,
            "drivers_management.postman.machine",
            "974d6f71-d41b-4601-9a7a-a33081f84682",
            "DriversManagement Postman Machine",
            "https://oauth.pstmn.io");
        driversManagementPostmanMachineClient.ExtendDefaultScopes(driversmanagementScope.Name);
        driversManagementPostmanMachineClient.AddAudienceMapper("drivers_management");

        // Authorization Code Flow — для отримання токена від імені користувача у Postman
        var driversManagementPostmanCodeClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "drivers_management.postman.code",
            "974d6f71-d41b-4601-9a7a-a33081f84680",
            "DriversManagement Postman Code",
            "https://oauth.pstmn.io",
            redirectUris: null,
            webOrigins: null);
        driversManagementPostmanCodeClient.ExtendDefaultScopes(driversmanagementScope.Name);
        driversManagementPostmanCodeClient.AddAudienceMapper("drivers_management");

        // Swagger UI клієнт (Authorization Code + PKCE) — використовується для інтерактивного входу з браузера
        var driversManagementSwaggerClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "drivers_management.swagger",
            "974d6f71-d41b-4601-9a7a-a33081f80687",
            "DriversManagement Swagger",
            "https://localhost:5001",
            redirectUris: null,
            webOrigins: null);
        driversManagementSwaggerClient.ExtendDefaultScopes(driversmanagementScope.Name, cargoOrderingScope.Name);
        driversManagementSwaggerClient.AddAudienceMapper("drivers_management");

        // BFF-клієнт (Backend-for-Frontend) із явним переліком redirect_uri та CORS webOrigins
        var driversManagementBFFClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "drivers_management.bff",
            "974d6f71-d41b-4601-9a7a-a33081f80688",
            "DriversManagement BFF",
            "https://localhost:4000",
            redirectUris: new InputList<string>
            {
                "https://localhost:4000/signin-oidc",
            },
            webOrigins: new InputList<string>
            {
                "https://localhost:5301",
                "https://localhost:4000",
            });
        driversManagementBFFClient.ExtendDefaultScopes(driversmanagementScope.Name);
        driversManagementBFFClient.AddAudienceMapper("drivers_management");

        // 4. Клієнти для BlogManagement (M2M, Postman, Swagger UI, BFF)
        var blogManagementPostmanMachineClient = ClientFactory.CreateClientCredentialsFlowClient(realm.Id,
            "blog_management.postman.machine",
            "56a32679-2c51-409b-9c4c-618274a06993",
            "BlogManagement Postman Machine",
            "https://oauth.pstmn.io");
        blogManagementPostmanMachineClient.ExtendDefaultScopes(blogmanagementScope.Name);
        blogManagementPostmanMachineClient.AddAudienceMapper("blog_management");

        var blogManagementPostmanCodeClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "blog_management.postman.code",
            "13b4ad5c-d172-4aab-8822-baa0cf96a9fb",
            "BlogManagement Postman Code",
            "https://oauth.pstmn.io",
            redirectUris: null,
            webOrigins: null);
        blogManagementPostmanCodeClient.ExtendDefaultScopes(blogmanagementScope.Name);
        blogManagementPostmanCodeClient.AddAudienceMapper("blog_management");

        var blogManagementSwaggerClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "blog_management.swagger",
            "81053a06-52b5-4f88-a885-43fbb487c1a4",
            "BlogManagement Swagger",
            "https://localhost:5002",
            redirectUris: null,
            webOrigins: null);
        blogManagementSwaggerClient.ExtendDefaultScopes(blogmanagementScope.Name);
        blogManagementSwaggerClient.AddAudienceMapper("blog_management");

        var blogManagementBFFClient = ClientFactory.CreateCodeFlowClient(realm.Id,
            "blog_management.bff",
            "48179ca7-36c7-4e9a-b1c4-4fb4ab092eaf",
            "BlogManagement BFF",
            "https://localhost:4000",
            redirectUris: new InputList<string>
            {
                "https://localhost:4000/signin-oidc",
            },
            webOrigins: new InputList<string>
            {
                "https://localhost:5301",
                "https://localhost:5302",
                "https://localhost:4000",
            });
        blogManagementBFFClient.ExtendDefaultScopes(blogmanagementScope.Name);
        blogManagementBFFClient.AddAudienceMapper("blog_management");

        // 5. Тестові користувачі реалму DevRealmDrivers (аналогічні тестовим обліковкам у Duende)
        var bob = new User("bob", new UserArgs
        {
            RealmId = realm.Id,
            Username = "bob",
            Enabled = true,
            Email = "bob@domain.com",
            FirstName = "Smith",
            LastName = "Bobson",
            InitialPassword = new UserInitialPasswordArgs
            {
                Value = "bob",
                Temporary = true,
            },
        });

        var alice = new User("alice", new UserArgs
        {
            RealmId = realm.Id,
            Username = "alice",
            Enabled = true,
            Email = "alice@domain.com",
            FirstName = "Alice",
            LastName = "Smith",
            InitialPassword = new UserInitialPasswordArgs
            {
                Value = "alice",
                Temporary = true,
            },
        });

        // ================================================================================
        // 🔄 6. КЕЙС АПДЕЙТА (UPDATE): ДЕМОНСТРАЦІЯ ЕВОЛЮЦІЇ ЖИВОГО KEYCLOAK ЧЕРЕЗ PULUMI
        // --------------------------------------------------------------------------------
        // Як продемонструвати Update:
        //   1. Спочатку виконайте `pulumi up` БЕЗ цього блоку — створиться `DevRealmDrivers-Pulumi`.
        //   2. Розкоментуйте код нижче і виконайте `pulumi preview` → побачите точковий diff
        //      (+ create лише для нового скоупа, клієнта та ролі, без перестворення існуючих).
        //   3. Виконайте `pulumi up` — зміни миттєво з'являться в адмінці Keycloak (:5080/auth).
        // ================================================================================
        /*
        var cargoAnalyticsScope = ScopeFactory.CreateScope(realm.Id, "cargo_analytics");

        var analyticsServiceClient = ClientFactory.CreateClientCredentialsFlowClient(
            realm.Id,
            "ordering_management.analytics",
            "analytics-m2m-secret-2026",
            "OrderingManagement Analytics Service (M2M)",
            "https://localhost:5003");
        analyticsServiceClient.ExtendDefaultScopes(cargoOrderingScope.Name, cargoAnalyticsScope.Name);
        analyticsServiceClient.AddAudienceMapper("cargo_ordering");

        var dispatcherRole = new Role("dispatcher-role", new RoleArgs
        {
            RealmId = realm.Id,
            Name = "Dispatcher",
            Description = "Роль диспетчера замовлень, додана через Pulumi Update"
        });
        */
    }
}
