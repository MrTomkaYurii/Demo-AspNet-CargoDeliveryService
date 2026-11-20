// ====================================================================================
// ⚙️ НАЛАШТУВАННЯ DI-КОНТЕЙНЕРА ТА HTTP-КОНВЕЄРА DUENDE IDENTITYSERVER
// ------------------------------------------------------------------------------------
// У цьому файлі зібрано два методи-розширення, які викликаються з Program.cs:
//   1. ConfigureServices() — підключає Razor Pages (UI логіну/логауту), сам движок
//      Duende IdentityServer, in-memory конфігурацію з Config.cs та dev-валідатори.
//   2. ConfigurePipeline() — задає точний порядок ASP.NET Core middleware.
// ====================================================================================

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using DuendeIdentityInMemory;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;

namespace DuendeIdentityInMemory
{
    internal static class HostingExtensions
    {
        /// <summary>
        /// Реєструє сервіси Duende IdentityServer, Razor Pages та допоміжні dev-компоненти в DI.
        /// </summary>
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            // Razor Pages потрібні для вбудованого UI: сторінок логіну (/Account/Login),
            // виходу (/Account/Logout), згоди (/Consent) та діагностики (/Diagnostics).
            builder.Services.AddRazorPages();

            // Підключення ядра Duende IdentityServer.
            // Автоматично підіймає всі OIDC/OAuth2 ендпоінти:
            //   /.well-known/openid-configuration, /connect/authorize, /connect/token, /connect/userinfo
            var isBuilder = builder.Services.AddIdentityServer(options =>
                {
                    // Вмикаємо генерацію внутрішніх подій (успішні/невдалі входи, видача токенів тощо)
                    // — зручно для аудиту й дебагу в логах Serilog / Aspire Dashboard.
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // Додає стандартний статичний клейм аудиторії ("aud": "{issuer}/resources") у токени.
                    // Докладніше: https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                })
                // Підключає in-memory сховище тестових користувачів (TestUserStore) замість БД.
                // Саме цей store перевіряє логін/пароль (alice/alice, bob/bob) на сторінці /Account/Login.
                .AddTestUsers(TestUsers.Users);

            // Реєстрація in-memory конфігурації OIDC/OAuth2 із Config.cs:
            //   • IdentityResources — що потрапляє в ID Token (openid, profile, email, roles)
            //   • ApiScopes         — гранульовані права доступу до API (blog_management тощо)
            //   • Clients           — зареєстровані застосунки (craftsman.swagger, m2m.client, swagger)
            //   • ApiResources      — логічні API-ресурси та список UserClaims, які включати в Access Token
            isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
            isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
            isBuilder.AddInMemoryClients(Config.Clients);
            isBuilder.AddInMemoryApiResources(Config.ApiResources);

            if (builder.Environment.IsDevelopment())
            {
                // Під час запуску через .NET Aspire порти localhost можуть змінюватися динамічно.
                // Замінюємо стандартний валідатор Redirect URI, щоб дозволити будь-який loopback (localhost),
                // і відкриваємо CORS, щоб Swagger UI з інших портів міг звертатися до /connect/token.
                builder.Services.AddSingleton<IRedirectUriValidator, DevLocalhostRedirectUriValidator>();
                builder.Services.AddSingleton<ICorsPolicyService>(sp =>
                    new DefaultCorsPolicyService(sp.GetRequiredService<ILogger<DefaultCorsPolicyService>>())
                    {
                        AllowAll = true
                    });
            }


            // Якщо потрібні серверні сесії (Server-Side Sessions):
            // https://blog.duendesoftware.com/posts/20220406_session_management/
            //isBuilder.AddServerSideSessions();
            //
            // та обмеження доступу до адмін-сторінки сесій:
            //builder.Services.AddAuthorization(options =>
            //       options.AddPolicy("admin",
            //           policy => policy.RequireClaim("sub", "1"))
            //   );
            //builder.Services.Configure<RazorPagesOptions>(options =>
            //    options.Conventions.AuthorizeFolder("/ServerSideSessions", "admin"));


            // Модуль 03.2 (DuendeIdentityInMemory) працює повністю автономно — без зовнішньої федерації.
            return builder.Build();
        }

        /// <summary>
        /// Налаштовує конвеєр обробки HTTP-запитів (порядок middleware має критичне значення).
        /// </summary>
        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            // Структуроване логування кожного вхідного HTTP-запиту через Serilog
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Статичні файли UI (Bootstrap, CSS, JS, іконки для сторінок логіну з wwwroot/)
            app.UseStaticFiles();
            app.UseRouting();

            // Головний middleware Duende IdentityServer:
            // перехоплює запити до /.well-known/*, /connect/* та вмикає cookie-автентифікацію STS
            app.UseIdentityServer();

            // Перевірка прав доступу для захищених Razor Pages (наприклад /Grants, /Diagnostics)
            app.UseAuthorization();

            // Ендпоінти здоров'я Aspire (/health, /alive)
            app.MapDefaultEndpoints();

            // Усі Razor Pages за замовчуванням вимагають входу, крім тих, де явно вказано [AllowAnonymous]
            // (наприклад, /Account/Login, /Home/Error)
            app.MapRazorPages()
                .RequireAuthorization();

            return app;
        }
    }

    /// <summary>
    /// Валідатор Redirect URI для середовища розробки Aspire:
    /// дозволяє будь-які локальні адреси (localhost / 127.0.0.1) з динамічними портами
    /// для Swagger OAuth2 PKCE, а для зовнішніх доменів перевіряє точний список із Config.cs.
    /// </summary>
    internal sealed class DevLocalhostRedirectUriValidator : IRedirectUriValidator
    {
        /// <summary>
        /// Перевіряє redirect_uri під час входу (/connect/authorize та /connect/token).
        /// </summary>
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri) && uri.IsLoopback)
                return Task.FromResult(true);

            return Task.FromResult(client.RedirectUris.Contains(requestedUri));
        }

        /// <summary>
        /// Перевіряє post_logout_redirect_uri після виходу (/connect/endsession).
        /// </summary>
        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri) && uri.IsLoopback)
                return Task.FromResult(true);

            return Task.FromResult(client.PostLogoutRedirectUris.Contains(requestedUri));
        }
    }
}
