// ====================================================================================
// 📋 КОНФІГУРАЦІЯ DUENDE IDENTITYSERVER (IN-MEMORY)
// ------------------------------------------------------------------------------------
// Цей файл описує 4 базові сутності OAuth 2.0 / OpenID Connect:
//   1. IdentityResources — дані ПРО КОРИСТУВАЧА (потрапляють в ID Token і /connect/userinfo).
//   2. ApiScopes         — ДОЗВОЛИ на виклик конкретних API (передаються в параметрі scope=...).
//   3. ApiResources      — логічні API-сервіси (формують клейм "aud" і список UserClaims,
//                          які Duende має покласти безпосередньо в Access Token).
//   4. Clients           — зареєстровані клієнтські застосунки (Swagger UI, M2M-сервіси),
//                          яким дозволено просити токени в цього STS.
// ====================================================================================

using Duende.IdentityServer.Models;

namespace DuendeIdentityInMemory
{
    public static class Config
    {
        // ============================================================================
        // 1. IDENTITY RESOURCES (OpenID Connect — «ХТО ЦЕЙ КОРИСТУВАЧ?»)
        // ----------------------------------------------------------------------------
        // Коли клієнт запитує ці скоупи, відповідні клейми потрапляють в ID Token
        // та на ендпоінт /connect/userinfo.
        // ============================================================================
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),   // Обов'язковий для OIDC: додає клейм "sub" (унікальний ID користувача)
                new IdentityResources.Profile(),  // Додає ім'я/прізвище (name, given_name, family_name, website)
                new IdentityResources.Email(),    // Додає email та email_verified
                // Кастомний ресурс: дозволяє запитувати scope=roles і отримувати клейм "role"
                new IdentityResource("roles", "User Roles", new[] { "role" }),
            };

        // ============================================================================
        // 2. API SCOPES (OAuth 2.0 — «ДО ЯКИХ API ПРОСИМО ДОСТУП?»)
        // ----------------------------------------------------------------------------
        // Скоупи — це імена дозволів, які клієнт вказує у запиті на токен (scope=...)
        // і які потім видно у клеймі "scope" всередині Access Token (at+jwt).
        // ============================================================================
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                // Скоупи для N-Tier проєкту DriversApi (Блок 01)
                new ApiScope("DriverApi.read"),
                new ApiScope("DriverApi.write"),

                // Скоупи для мікросервісів Craftsman (Блок 02; зокрема blog_management для BlogManagement)
                new ApiScope("drivers_management", "DriversManagement Microservice API"),
                new ApiScope("cargo_ordering", "OrderingManagement Microservice API"),
                new ApiScope("blog_management", "BlogManagement Microservice API"),
            };

        // ============================================================================
        // 3. API RESOURCES (ЛОГІЧНІ ЗАХИЩЕНІ СЕРВІСИ)
        // ----------------------------------------------------------------------------
        // Зв'язують ApiScopes із конкретним API-ресурсом:
        //   • Додають ім'я ресурсу в клейм "aud" (Audience) Access Token.
        //   • ВАЖЛИВО: UserClaims = { "role" } змушує Duende скопіювати клейм "role"
        //     користувача прямо в Access Token (at+jwt), щоб у BlogManagement працювали
        //     політики RequireRole("SuperAdmin") та User.IsInRole(...).
        // ============================================================================
        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("DriverApi")
            {
                Scopes = new List<string> { "DriverApi.read", "DriverApi.write" },
            },
            new ApiResource("drivers_management")
            {
                Scopes = new List<string> { "drivers_management", "cargo_ordering", "blog_management" },
                UserClaims = { "role" },
            },
            new ApiResource("cargo_ordering")
            {
                Scopes = new List<string> { "cargo_ordering", "drivers_management" },
                UserClaims = { "role" },
            },
            new ApiResource("blog_management")
            {
                Scopes = new List<string> { "blog_management" },
                UserClaims = { "role" },
            }
        };

        // ============================================================================
        // 4. CLIENTS (ЗАСТОСУНКИ, ЯКІ ОТРИМУЮТЬ ТОКЕНИ)
        // ----------------------------------------------------------------------------
        // Кожен клієнт визначає:
        //   • AllowedGrantTypes   — яким OAuth2-потоком йому дозволено користуватися
        //   • RequirePkce         — чи обов'язковий PKCE (code_challenge / code_verifier)
        //   • RequireClientSecret — false для браузерних клієнтів (Swagger/SPA), які не
        //                           можуть безпечно зберігати секрет
        //   • AllowedScopes       — які саме скоупи цей клієнт має право запитувати
        // ============================================================================
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // --------------------------------------------------------------------
                // КЛІЄНТ 1: Machine-to-Machine (Client Credentials Flow)
                // Використовується для запитів сервер-сервер БЕЗ участі користувача.
                // Клієнт шле client_id + client_secret на POST /connect/token і одразу
                // отримує Access Token (без ID Token і без клейму sub).
                // --------------------------------------------------------------------
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    // Секрет зберігається у вигляді SHA-256 хешу; клієнт передає відкрите значення
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "DriverApi.read", "DriverApi.write", "drivers_management", "cargo_ordering", "blog_management" }
                },

                // --------------------------------------------------------------------
                // КЛІЄНТ 2: Універсальний інтерактивний Swagger (Authorization Code + PKCE)
                // Підходить для N-Tier DriversApi (:4002) та мікросервісів (:5000–:5003).
                // --------------------------------------------------------------------
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger UI for CargoDelivery Platform",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code, // Authorization Code Flow (одноразовий code -> POST /connect/token)

                    RequirePkce = true,                  // Обов'язковий захист PKCE (S256)
                    RequireClientSecret = false,         // Публічний клієнт (у браузері секрет не передається)

                    // Дозволені адреси повернення після логіну (у Development додатково працює DevLocalhostRedirectUriValidator)
                    RedirectUris =
                    {
                        "https://localhost:4002/swagger/oauth2-redirect.html",
                        "https://localhost:5000/swagger/oauth2-redirect.html",
                        "https://localhost:5001/swagger/oauth2-redirect.html",
                        "https://localhost:5002/swagger/oauth2-redirect.html",
                        "https://localhost:5003/swagger/oauth2-redirect.html"
                    },
                    // Дозволені origin-и для CORS-запитів із браузера до /connect/token
                    AllowedCorsOrigins =
                    {
                        "https://localhost:4002",
                        "https://localhost:5000",
                        "https://localhost:5001",
                        "https://localhost:5002",
                        "https://localhost:5003"
                    },

                    AllowOfflineAccess = true, // Дозволяє запитувати scope=offline_access для отримання Refresh Token
                    AllowedScopes = { "openid", "profile", "email", "DriverApi.read", "DriverApi.write", "drivers_management", "cargo_ordering", "blog_management" }
                },

                // --------------------------------------------------------------------
                // КЛІЄНТ 3: Основний Swagger-клієнт для BlogManagement (03.2)
                // Саме цей ClientId прописаний у BlogManagement/.../SwaggerAppExtension.cs.
                // Включає скоуп "roles", щоб ролі користувача (SuperAdmin/User) передавалися в токен.
                // --------------------------------------------------------------------
                new Client
                {
                    ClientId = "craftsman.swagger",
                    ClientName = "Craftsman Microservices Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code, // Authorization Code Flow
                    RequirePkce = true,                  // PKCE обов'язковий (Swagger UI вмикає config.OAuthUsePkce())
                    RequireClientSecret = false,         // Публічний клієнт — поле client_secret у Swagger лишається порожнім
                    RedirectUris = { "https://localhost:5000/swagger/oauth2-redirect.html" }, // Для :5002 у Dev дозволяє DevLocalhostRedirectUriValidator
                    AllowOfflineAccess = true,           // Підтримка Refresh Token (scope=offline_access)
                    AllowedScopes = { "openid", "profile", "email", "roles", "drivers_management", "cargo_ordering", "blog_management" }
                }
            };
    }
}
