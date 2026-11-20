using Destructurama;
using Serilog;
using Serilog.Extensions.Logging;
using Hangfire;
using HeimGuard;
using Hellang.Middleware.ProblemDetails;
using BlogManagement.Extensions.Application;
using BlogManagement.Extensions.Services;
using BlogManagement.Databases;
using BlogManagement.Domain.Drivers.Services;
using BlogManagement.Resources;
using BlogManagement.Resources.HangfireUtilities;
using BlogManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

    // ====================================================================================
    // 📝 SERILOG + OPENTELEMETRY: ЯК ЛОГИ ПОТРАПЛЯЮТЬ В ASPIRE DASHBOARD
    // ------------------------------------------------------------------------------------
    // builder.Host.UseSerilog() ЗАМІНЮЄ всі стандартні провайдери логування Microsoft.
    // Серед них і провайдер OpenTelemetry, який підключає AddServiceDefaults() — тому без
    // додаткових кроків логи сервісу видно лише в "Console logs", а вкладка
    // "Structured logs" в Aspire Dashboard порожня.
    // Рішення: LoggerProviderCollection + WriteTo.Providers(...) — Serilog пише як у консоль
    // (за appsettings), так і в усі зареєстровані ILoggerProvider (у т.ч. OTLP-експортер).
    // ====================================================================================
    var serilogProviders = new LoggerProviderCollection();
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
        .Destructure.UsingAttributes()
        .WriteTo.Providers(serilogProviders)
        .CreateLogger();

    builder.Host.UseSerilog(providers: serilogProviders);
    builder.ConfigureServices();

    // ====================================================================================
    // 🏛️ 03.2 АВТЕНТИФІКАЦІЯ ТА АВТОРИЗАЦІЯ: DUENDE IDENTITYSERVER (OIDC / OAuth 2.0 + PKCE)
    // ------------------------------------------------------------------------------------
    // Усі налаштування безпеки винесені прямо у Program.cs для повної прозорості.
    // BlogManagement демонструє підхід 03.2 (виділений Security Token Service):
    //   • Мікросервіс НЕ зберігає паролів і НЕ знає спільного секрету (на відміну від 03.1).
    //   • Приватний ключ підпису RS256 лежить лише в DuendeIdentityInMemory (:5012).
    //   • Публічні ключі (JWKS) підтягуються автоматично через OIDC Discovery при першому запиті.
    //   • Підтримуються як рольові політики ASP.NET Core (SuperAdmin / User), так і HeimGuard.
    // ====================================================================================

    // 1. Адреса STS (DuendeIdentityInMemory):
    //    • Під Aspire інжектується автоматично через змінну Auth__Duende__Authority в AppHost.cs.
    //    • При автономному запуску використовується локальний fallback https://localhost:5012.
    var duendeAuthority = (builder.Configuration["Auth:Duende:Authority"]
                           ?? "https://localhost:5012").TrimEnd('/');

    // У середовищі FunctionalTesting реальний JwtBearer не реєструється, бо тести підставляють фейкову автентифікацію.
    if (!builder.Environment.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
    {
        // 2. Реєстрація схеми автентифікації JwtBearer (перевірка заголовка Authorization: Bearer <access_token>)
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // КЛЮЧОВИЙ РЯДОК OIDC: змушує JwtBearerHandler при першому запиті з токеном
                // сходити на {Authority}/.well-known/openid-configuration, забрати jwks_uri,
                // завантажити публічні RSA-ключі (JWKS) і закешувати їх для локальної перевірки RS256.
                options.Authority = duendeAuthority;

                // У Production вимагаємо виключно HTTPS для Discovery/JWKS; у Development дозволяємо локальний стенд.
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                // Спрощення для навчального стенду: не блокуємо токен за полем "aud" (Audience),
                // але підпис (RS256), видавця (iss) та термін дії (exp) все одно перевіряємо суворо.
                options.TokenValidationParameters.ValidateAudience = false;

                // Duende IdentityServer за стандартом RFC 9068 випускає Access Token із заголовком "typ": "at+jwt".
                // Явно дозволяємо "at+jwt" і класичний "JWT", щоб валідатор не відхиляв токени Duende з 401.
                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt", "JWT" };

                // Вказуємо, з якого клейму брати User.Identity.Name.
                // (За замовчуванням MapInboundClaims = true автоматично мапить клейм "role" у ClaimTypes.Role,
                // тому User.IsInRole("SuperAdmin") та policy.RequireRole("SuperAdmin") працюють одразу.)
                options.TokenValidationParameters.NameClaimType = "name";

                if (builder.Environment.IsDevelopment())
                {
                    // Backchannel — це прямий сервер-сервер запит BlogManagement -> Duende за документом Discovery/JWKS.
                    // У локальному Aspire-стенді дозволяємо self-signed dev-сертифікат HTTPS, щоб цей запит не падав.
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
            });
    }

    // 3. Реєстрація політик авторизації на основі клейму "role" (який Duende кладе в Access Token
    //    завдяки ApiResource.UserClaims = { "role" } у DuendeIdentityInMemory/Config.cs):
    //      • alice / alice -> SuperAdmin, User (проходить обидві політики)
    //      • bob   / bob   -> User             (проходить RequireUserRole, на RequireAdminRole отримає 403 Forbidden)
    //    Застосовуються на контролерах через [Authorize(Policy = "RequireAdminRole")] тощо.
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin"));
        options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "SuperAdmin"));
    });

    // 4. HeimGuard — гранульована перевірка прав (permissions) на рівні MediatR-хендлерів через UserPolicyHandler.
    builder.Services.AddHeimGuard<UserPolicyHandler>()
        .MapAuthorizationPolicies()
        .AutomaticallyCheckPermissions();

    // 5. Налаштування Swagger UI як OAuth 2.0 клієнта для Duende (Authorization Code + PKCE):
    //    Тут описуємо OpenAPI-схему безпеки (ендпоінти Duende та скоупи), а сам client_id ("craftsman.swagger")
    //    і прапорець PKCE (OAuthUsePkce()) підключаються у SwaggerAppExtension.cs.
    builder.Services.ConfigureSwaggerGen(config =>
    {
        config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "【03.2 Duende OIDC】 Authorization Code + PKCE (alice/alice — SuperAdmin, bob/bob — User).",
            Flows = new OpenApiOAuthFlows
            {
                // AuthorizationCode Flow: Swagger UI редиректить браузер на /connect/authorize (Крок 2),
                // отримує одноразовий code (Крок 3) і обмінює його + code_verifier на /connect/token (Крок 4).
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{duendeAuthority}/connect/authorize"),
                    TokenUrl = new Uri($"{duendeAuthority}/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect (обов'язковий: видає ID Token і клейм sub)" },
                        { "profile", "Профіль користувача (name, given_name, family_name)" },
                        { "roles", "Ролі користувача (клейм role: SuperAdmin / User)" },
                        { "blog_management", "Доступ до BlogManagement API (включає role в Access Token)" }
                    }
                }
            }
        });

        // Прив'язує схему "oauth2" глобально до всіх ендпоінтів у Swagger UI,
        // щоб після входу Swagger автоматично додавав заголовок Authorization: Bearer <access_token>.
        config.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                },
                new[] { "openid", "profile", "roles", "blog_management" }
            }
        });
    });

var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.MapGrpcService<DriverRpcService>();

    using var scope = app.Services.CreateScope();
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseProblemDetails();

    // For elevated security, it is recommended to remove this middleware and set your server to only listen on https.
    // A slightly less secure option would be to redirect http to 400, 505, etc.
    app.UseHttpsRedirection();

    app.UseCors("BlogManagementCorsPolicy");

    app.MapHealthChecks("api/health");
app.MapDefaultEndpoints();
    app.UseSerilogRequestLogging();
    app.UseRouting();

    // 6. Конвеєр безпеки (порядок обов'язковий!):
    //    • UseAuthentication() — читає Bearer-токен, перевіряє RS256-підпис через JWKS Duende і заповнює HttpContext.User.
    //    • UseAuthorization()  — перевіряє атрибути [Authorize], рольові політики (RequireAdminRole) та HeimGuard.
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        AsyncAuthorization = new[] { new HangfireAuthorizationFilter(scope.ServiceProvider) },
        IgnoreAntiforgeryToken = true
    });

    app.UseSwaggerExtension(builder.Configuration, builder.Environment);

    try
    {
        Log.Information("Starting application");
        await app.RunAsync();
    }
    catch (Exception e)
    {
        Log.Error(e, "The application failed to start correctly");
        throw;
    }
    finally
    {
        Log.Information("Shutting down application");
        Log.CloseAndFlush();
    }

    // Make the implicit Program class public so the functional test project can access it
    public partial class Program { }