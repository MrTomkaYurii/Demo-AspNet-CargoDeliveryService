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
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddRazorPages();

            var isBuilder = builder.Services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                })
                .AddTestUsers(TestUsers.Users);

            // in-memory, code config
            isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
            isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
            isBuilder.AddInMemoryClients(Config.Clients);
            isBuilder.AddInMemoryApiResources(Config.ApiResources);

            if (builder.Environment.IsDevelopment())
            {
                // Дозволяємо динамічні localhost-порти, які .NET Aspire призначає Swagger UI сервісів
                builder.Services.AddSingleton<IRedirectUriValidator, DevLocalhostRedirectUriValidator>();
                builder.Services.AddSingleton<ICorsPolicyService>(sp =>
                    new DefaultCorsPolicyService(sp.GetRequiredService<ILogger<DefaultCorsPolicyService>>())
                    {
                        AllowAll = true
                    });
            }


            // if you want to use server-side sessions: https://blog.duendesoftware.com/posts/20220406_session_management/
            // then enable it
            //isBuilder.AddServerSideSessions();
            //
            // and put some authorization on the admin/management pages
            //builder.Services.AddAuthorization(options =>
            //       options.AddPolicy("admin",
            //           policy => policy.RequireClaim("sub", "1"))
            //   );
            //builder.Services.Configure<RazorPagesOptions>(options =>
            //    options.Conventions.AuthorizeFolder("/ServerSideSessions", "admin"));


            var keycloakAuthority = builder.Configuration["Keycloak:Authority"];
            var authBuilder = builder.Services.AddAuthentication();

            if (!string.IsNullOrWhiteSpace(keycloakAuthority))
            {
                // ================================================================================
                // 🤝 03.3 ФЕДЕРАЦІЯ KEYCLOAK + DUENDE (External OIDC Identity Provider, опціонально)
                // --------------------------------------------------------------------------------
                // Реєструється лише коли задано Keycloak:Authority (наприклад, з Aspire AppHost).
                // При автономному запуску Duende (03.2) цей блок не активується.
                // ================================================================================
                authBuilder.AddOpenIdConnect("keycloak", "Keycloak (Corporate SSO)", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.SaveTokens = true;

                    options.Authority = keycloakAuthority;
                    options.ClientId = "duende.federation";
                    options.ClientSecret = "duende-keycloak-secret";
                    options.ResponseType = "code";
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.GetClaimsFromUserInfoEndpoint = true;
                });
            }

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapDefaultEndpoints();
            app.MapRazorPages()
                .RequireAuthorization();

            return app;
        }
    }

    /// <summary>
    /// Валідатор Redirect URI для середовища розробки Aspire: дозволяє динамічні порти localhost для Swagger OAuth2 PKCE.
    /// </summary>
    internal sealed class DevLocalhostRedirectUriValidator : IRedirectUriValidator
    {
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri) && uri.IsLoopback)
                return Task.FromResult(true);

            return Task.FromResult(client.RedirectUris.Contains(requestedUri));
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (Uri.TryCreate(requestedUri, UriKind.Absolute, out var uri) && uri.IsLoopback)
                return Task.FromResult(true);

            return Task.FromResult(client.PostLogoutRedirectUris.Contains(requestedUri));
        }
    }
}