// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

// ====================================================================================
// 👥 ТЕСТОВІ КОРИСТУВАЧІ (IN-MEMORY) З РОЛЯМИ ДЛЯ ДЕМОНСТРАЦІЇ 03.2
// ------------------------------------------------------------------------------------
// Описує тестові облікові записи для Duende IdentityServer (замість реальної БД):
//   • alice / alice — адміністратор (SubjectId = "1", ролі: SuperAdmin, User)
//   • bob   / bob   — звичайний користувач (SubjectId = "2", роль: User)
// ====================================================================================

using IdentityModel;
using System.Security.Claims;
using System.Text.Json;
using Duende.IdentityServer;
using Duende.IdentityServer.Test;

namespace DuendeIdentityInMemory;

public class TestUsers
{
    public static List<TestUser> Users
    {
        get
        {
            // Тестова структура адреси, яка серіалізується як JSON-клейм "address" (стандарт OIDC)
            var address = new
            {
                street_address = "One Hacker Way",
                locality = "Heidelberg",
                postal_code = 69118,
                country = "Germany"
            };

            return new List<TestUser>
            {
                // Адміністратор: має обидві ролі (SuperAdmin + User), проходить політику RequireAdminRole
                new TestUser
                {
                    SubjectId = "1",     // Потрапляє в клейм "sub" (унікальний ідентифікатор користувача)
                    Username = "alice",  // Логін на сторінці /Account/Login
                    Password = "alice",  // Пароль (у відкритому вигляді лише для In-Memory демо!)
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json),
                        // Клейми ролей: потрапляють в Access Token завдяки ApiResource.UserClaims = { "role" } у Config.cs
                        new Claim(JwtClaimTypes.Role, "SuperAdmin"),
                        new Claim(JwtClaimTypes.Role, "User")
                    }
                },
                // Звичайний користувач: має лише роль User (на адмін-ендпоінтах отримає 403 Forbidden)
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "bob",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim(JwtClaimTypes.Role, "User")
                    }
                }
            };
        }
    }
}
