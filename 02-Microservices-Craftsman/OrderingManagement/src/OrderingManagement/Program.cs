using System.Security.Claims;
using System.Text.Json;
using Destructurama;
using Serilog;
using Serilog.Extensions.Logging;
using Hangfire;
using HeimGuard;
using Hellang.Middleware.ProblemDetails;
using OrderingManagement.Extensions.Application;
using OrderingManagement.Extensions.Services;
using OrderingManagement.Databases;
using OrderingManagement.Resources;
using OrderingManagement.Resources.HangfireUtilities;
using OrderingManagement.Services;
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
// 🏢 03.3 АВТЕНТИФІКАЦІЯ ТА АВТОРИЗАЦІЯ: KEYCLOAK ENTERPRISE IAM (Realm DevRealmDrivers)
// ------------------------------------------------------------------------------------
// Усі налаштування безпеки винесені прямо у Program.cs для повної прозорості.
// OrderingManagement демонструє підхід 03.3:
//   • Корпоративний IAM Keycloak у контейнері Aspire (http://localhost:5080/auth/realms/DevRealmDrivers).
//   • Валідація RS256-токенів через OIDC Discovery / JWKS.
//   • Мапінг вкладеного JSON-клейму Keycloak `realm_access.roles` у стандартні ClaimTypes.Role.
// ====================================================================================
var keycloakAuthority = (builder.Configuration["Auth:Keycloak:Authority"]
                         ?? "http://localhost:5080/auth/realms/DevRealmDrivers").TrimEnd('/');

if (!builder.Environment.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = keycloakAuthority;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "preferred_username";
            options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

            // Keycloak передає ролі реалму всередині JSON-об'єкта realm_access: { "roles": [...] }.
            // Перетворюємо їх на стандартні ClaimTypes.Role, щоб працювали політики та User.IsInRole().
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                    {
                        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                        if (!string.IsNullOrEmpty(realmAccess))
                        {
                            using var doc = JsonDocument.Parse(realmAccess);
                            if (doc.RootElement.TryGetProperty("roles", out var roles))
                            {
                                foreach (var role in roles.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrWhiteSpace(roleName))
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });
}

// Налаштування рольової авторизації (SuperAdmin / User із Keycloak) та HeimGuard
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User", "SuperAdmin"));
});
builder.Services.AddHeimGuard<UserPolicyHandler>()
    .MapAuthorizationPolicies()
    .AutomaticallyCheckPermissions();

// Налаштування Swagger UI для входу через Keycloak (Authorization Code + PKCE)
builder.Services.ConfigureSwaggerGen(config =>
{
    config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Description = "【03.3 Keycloak IAM】 Вхід через Realm DevRealmDrivers (alice/alice — SuperAdmin, bob/bob — User).",
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{keycloakAuthority}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "Профіль користувача" },
                    { "roles", "Ролі реалму" },
                    { "cargo_ordering", "Доступ до OrderingManagement API" }
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
            new[] { "openid", "profile", "roles", "cargo_ordering" }
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

app.UseCors("OrderingManagementCorsPolicy");

app.MapHealthChecks("api/health");
app.MapDefaultEndpoints();
app.UseSerilogRequestLogging();
app.UseRouting();

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