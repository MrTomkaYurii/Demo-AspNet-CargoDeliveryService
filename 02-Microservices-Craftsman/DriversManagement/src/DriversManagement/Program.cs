using System.Text;
using Destructurama;
using Serilog;
using Serilog.Extensions.Logging;
using Hangfire;
using HeimGuard;
using Hellang.Middleware.ProblemDetails;
using DriversManagement.Extensions.Application;
using DriversManagement.Extensions.Services;
using DriversManagement.Databases;
using DriversManagement.Resources;
using DriversManagement.Resources.HangfireUtilities;
using DriversManagement.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
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
    // 🔑 03.1 АВТЕНТИФІКАЦІЯ ТА АВТОРИЗАЦІЯ: ПРОСТИЙ CUSTOM JWT (AuthorizationModuleAPI)
    // ------------------------------------------------------------------------------------
    // Усі налаштування безпеки винесені прямо у Program.cs для повної прозорості.
    // DriversManagement демонструє підхід 03.1:
    //   • Симетричний підпис HMAC-SHA256 (HS256) за спільним секретом Auth:SimpleJwt:Secret.
    //   • Без OIDC Discovery / JWKS — токен перевіряється повністю локально.
    //   • У функціональних тестах (FunctionalTesting) реальний JwtBearer пропускається.
    // ====================================================================================
    if (!builder.Environment.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
    {
        var jwtSection = builder.Configuration.GetSection("Auth:SimpleJwt");
        var jwtSecret = jwtSection["Secret"] ?? "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM";
        var jwtIssuer = jwtSection["Issuer"] ?? "AuthorizationModuleAPI";
        var jwtAudience = jwtSection["Audience"] ?? "cargodelivery.microservices";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
    }

    // Реєстрація політик авторизації та HeimGuard (перевірка дозволів на рівні MediatR/контролерів)
    builder.Services.AddAuthorization();
    builder.Services.AddHeimGuard<UserPolicyHandler>()
        .MapAuthorizationPolicies()
        .AutomaticallyCheckPermissions();

    // Налаштування Swagger UI для введення Bearer-токена, отриманого від AuthorizationModuleAPI
    builder.Services.ConfigureSwaggerGen(config =>
    {
        config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "【03.1 Simple JWT】 Вставте jwtToken, отриманий через POST /accounts/authenticate у AuthorizationModuleAPI."
        });

        config.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

var app = builder.Build();

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

    app.UseCors("DriversManagementCorsPolicy");

    app.MapHealthChecks("api/health");
    app.MapDefaultEndpoints();
    app.UseSerilogRequestLogging();
    app.UseRouting();

    // Конвеєр безпеки: спочатку перевірка JWT-токена (03.1 Simple JWT), потім політики та HeimGuard
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