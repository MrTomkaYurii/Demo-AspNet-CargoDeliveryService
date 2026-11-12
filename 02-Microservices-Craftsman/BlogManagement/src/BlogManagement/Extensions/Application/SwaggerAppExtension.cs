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
                // Налаштування PKCE для Swagger-клієнта Duende IdentityServer (03.2)
                config.OAuthClientId("craftsman.swagger");
                config.OAuthAppName("BlogManagement - Duende PKCE");
                config.OAuthUsePkce();
            });
        }
    }
}