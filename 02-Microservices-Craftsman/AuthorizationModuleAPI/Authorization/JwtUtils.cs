namespace AuthorizationModuleAPI.Authorization;

using AuthorizationModuleAPI.Data;
using AuthorizationModuleAPI.Entities;
using AuthorizationModuleAPI.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public interface IJwtUtils
{
    public string GenerateJwtToken(Account account);
    public int? ValidateJwtToken(string token);
    public RefreshToken GenerateRefreshToken(string ipAddress);
}

public class JwtUtils : IJwtUtils
{
    private readonly DataContext _context;
    private readonly AppSettings _appSettings;

    public JwtUtils(
        DataContext context,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _appSettings = appSettings.Value;
    }

    public string GenerateJwtToken(Account account)
    {
        // Генеруємо Access Token (термін дії 60 хв) із симетричним підписом HMAC-SHA256 (Модуль 03.1).
        // Клейми дублюються у стандартних JWT-іменах (sub, email) та ClaimTypes.* для повної сумісності
        // з ICurrentUserService і UserPolicyHandler у бізнес-мікросервісі DriversManagement.
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email ?? string.Empty),
                new Claim(ClaimTypes.Email, account.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.GivenName, account.FirstName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, account.FirstName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.FamilyName, account.LastName ?? string.Empty),
                new Claim(ClaimTypes.Surname, account.LastName ?? string.Empty),
                new Claim("preferred_username", account.Email ?? $"user-{account.Id}"),
                new Claim(ClaimTypes.Role, account.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            // Спільний секрет (Secret), Issuer та Audience мають збігатися з налаштуваннями JwtBearer у DriversManagement
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _appSettings.Issuer,
            Audience = _appSettings.Audience
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int? ValidateJwtToken(string token)
    {
        if (token == null)
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _appSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _appSettings.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            // return account id from JWT token if validation successful
            return accountId;
        }
        catch
        {
            // return null if validation fails
            return null;
        }
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            // token is a cryptographically strong random sequence of values
            Token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
            // token is valid for 7 days
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        // ensure token is unique by checking against db
        var tokenIsUnique = !_context.Accounts.Any(a => a.RefreshTokens.Any(t => t.Token == refreshToken.Token));

        if (!tokenIsUnique)
            return GenerateRefreshToken(ipAddress);

        return refreshToken;
    }
}