namespace OrderingManagement.Extensions.Application;

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
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderingManagement v1");
                config.DocExpansion(DocExpansion.None);
                // Налаштування PKCE для Swagger-клієнта Keycloak (03.3, Realm DevRealmDrivers)
                config.OAuthClientId("drivers_management.swagger");
                config.OAuthAppName("OrderingManagement - Keycloak PKCE");
                config.OAuthUsePkce();
            });
        }
    }
}