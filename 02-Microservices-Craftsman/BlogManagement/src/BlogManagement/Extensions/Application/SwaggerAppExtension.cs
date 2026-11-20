namespace BlogManagement.Extensions.Application;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Resources;
using Swashbuckle.AspNetCore.SwaggerUI;

public static class SwaggerAppExtension
{
    public static void UseSwaggerExtension(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "BlogManagement v1");
                config.DocExpansion(DocExpansion.None);
                // Налаштування OAuth 2.0 + PKCE для Swagger UI (клієнт із DuendeIdentityInMemory/Config.cs):
                //   • OAuthClientId("craftsman.swagger") — підставляє client_id у форму Authorize (client_secret порожній).
                //   • OAuthUsePkce()                     — вмикає генерацію code_verifier / code_challenge (S256) у браузері.
                config.OAuthClientId("craftsman.swagger");
                config.OAuthAppName("BlogManagement - Duende PKCE");
                config.OAuthUsePkce();
            });
        }
    }
}