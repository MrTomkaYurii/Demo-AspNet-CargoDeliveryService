using Duende.IdentityServer.Models;

namespace DuendeIdentityInMemory
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("roles", "User Roles", new[] { "role" }),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                // N-Tier DriversApi scopes
                new ApiScope("DriverApi.read"),
                new ApiScope("DriverApi.write"),

                // Craftsman Microservices scopes
                new ApiScope("drivers_management", "DriversManagement Microservice API"),
                new ApiScope("cargo_ordering", "OrderingManagement Microservice API"),
                new ApiScope("blog_management", "BlogManagement Microservice API"),
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("DriverApi")
            {
                Scopes = new List<string> { "DriverApi.read", "DriverApi.write" },
            },
            new ApiResource("drivers_management")
            {
                Scopes = new List<string> { "drivers_management", "cargo_ordering", "blog_management" },
                UserClaims = { "role" },
            },
            new ApiResource("cargo_ordering")
            {
                Scopes = new List<string> { "cargo_ordering", "drivers_management" },
                UserClaims = { "role" },
            },
            new ApiResource("blog_management")
            {
                Scopes = new List<string> { "blog_management" },
                UserClaims = { "role" },
            }
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "DriverApi.read", "DriverApi.write", "drivers_management", "cargo_ordering", "blog_management" }
                },

                // interactive client using code flow + pkce (N-Tier & Craftsman Swagger UI)
                new Client
                {
                    ClientId = "swagger",
                    ClientName = "Swagger UI for CargoDelivery Platform",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris =
                    {
                        "https://localhost:4002/swagger/oauth2-redirect.html",
                        "https://localhost:5000/swagger/oauth2-redirect.html",
                        "https://localhost:5001/swagger/oauth2-redirect.html",
                        "https://localhost:5002/swagger/oauth2-redirect.html",
                        "https://localhost:5003/swagger/oauth2-redirect.html"
                    },
                    AllowedCorsOrigins =
                    {
                        "https://localhost:4002",
                        "https://localhost:5000",
                        "https://localhost:5001",
                        "https://localhost:5002",
                        "https://localhost:5003"
                    },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "email", "DriverApi.read", "DriverApi.write", "drivers_management", "cargo_ordering", "blog_management" }
                },

                // Craftsman unified Swagger client (PKCE)
                new Client
                {
                    ClientId = "craftsman.swagger",
                    ClientName = "Craftsman Microservices Swagger UI",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RedirectUris = { "https://localhost:5000/swagger/oauth2-redirect.html" },
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "email", "roles", "drivers_management", "cargo_ordering", "blog_management" }
                }
            };
    }
}
