// ====================================================================================
// 🏛️ 03.2 DUENDE IDENTITYSERVER (IN-MEMORY) — ТОЧКА ВХОДУ STS-СЕРВЕРА (:5012)
// ------------------------------------------------------------------------------------
// Цей проєкт є виділеним сервером автентифікації та авторизації (Security Token Service,
// скорочено STS) за стандартами OAuth 2.0 та OpenID Connect (OIDC).
//
// Що саме робить цей сервіс:
//   1. Показує сторінку входу користувача (/Account/Login) і видає сесійну cookie.
//   2. Публікує стандартні OIDC/OAuth2 ендпоінти:
//      • /.well-known/openid-configuration — Discovery-документ із публічними ключами (JWKS)
//      • /connect/authorize                — початок Authorization Code Flow + PKCE
//      • /connect/token                    — обмін коду або client_credentials на JWT-токени
//      • /connect/userinfo                 — отримання клеймів профілю користувача
//   3. Підписує токени асиметричним ключем RS256 (ключі зберігаються у папці /keys).
//
// Конфігурація клієнтів, скоупів і користувачів зберігається в пам'яті (In-Memory):
//   • Config.cs             — IdentityResources, ApiScopes, ApiResources, Clients
//   • HostingExtensions.cs  — реєстрація IdentityServer у DI та HTTP-конвеєр (pipeline)
//   • TestUsers.cs          — тестові користувачі (alice / alice, bob / bob)
// ====================================================================================

using DuendeIdentityInMemory;
using Serilog;

// 1. Початковий (bootstrap) логер Serilog — логує етап старту до побудови DI-контейнера
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // 2. Підключення .NET Aspire ServiceDefaults:
    //    OpenTelemetry (логи, метрики, трейси), HealthChecks (/health, /alive) та Service Discovery
    builder.AddServiceDefaults();

    // 3. Повноцінне налаштування Serilog із appsettings.json та форматованим виводом у консоль
    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    // 4. Реєстрація сервісів IdentityServer та побудова HTTP-конвеєра (див. HostingExtensions.cs)
    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    // 5. Запуск веб-сервера (порт за замовчуванням: https://localhost:5012)
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
