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
    // BlogManagement демонструє підхід 03.2:
    //   • Виділений STS (DuendeIdentityInMemory), що видає токени формату "at+jwt" (RS256).
    //   • Публічні ключі (JWKS) завантажуються автоматично через OIDC Discovery.
    //   • Підтримка ролей (клейм "role" → RoleClaimType) для політик авторизації та HeimGuard.
    // ====================================================================================
    var duendeAuthority = (builder.Configuration["Auth:Duende:Authority"]
                           ?? "https://localhost:5012").TrimEnd('/');

    if (!builder.Environment.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = duendeAuthority;
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                // Duende випускає токени з типом "at+jwt" (RFC 9068).
                // За замовчуванням MapInboundClaims = true автоматично мапить клейм "role" у ClaimTypes.Role.
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidTypes = new[] { "at+jwt", "JWT" };
                options.TokenValidationParameters.NameClaimType = "name";

                if (builder.Environment.IsDevelopment())
                {
                    // У локальному Aspire-стенді дозволяємо dev-сертифікат для backchannel-запиту JWKS
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                }
            });
    }

    // Налаштування рольової авторизації (для клеймів із Duende: SuperAdmin / User) та HeimGuard
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin"));
        options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "SuperAdmin"));
    });
    builder.Services.AddHeimGuard<UserPolicyHandler>()
        .MapAuthorizationPolicies()
        .AutomaticallyCheckPermissions();

    // Налаштування Swagger UI для входу через Duende IdentityServer (Authorization Code + PKCE)
    builder.Services.ConfigureSwaggerGen(config =>
    {
        config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "【03.2 Duende OIDC】 Authorization Code + PKCE (alice/alice — SuperAdmin, bob/bob — User).",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{duendeAuthority}/connect/authorize"),
                    TokenUrl = new Uri($"{duendeAuthority}/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "profile", "Профіль користувача" },
                        { "roles", "Ролі користувача (role)" },
                        { "blog_management", "Доступ до BlogManagement API" }
                    }
                }
            }
        });

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

    // Конвеєр безпеки: перевірка OIDC-токена Duende (03.2) та рольових політик / HeimGuard
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