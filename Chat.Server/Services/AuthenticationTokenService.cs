using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Server.Services;

public interface IAuthenticationTokenService
{
    public string? GenerateToken(IdentityUser user);
    public string? VerifyToken(string token);
}

public class AuthenticationTokenService : IAuthenticationTokenService
{
    private readonly int _expirationDay = 30;
    private readonly ILogger<AuthenticationTokenService> _logger;
    private readonly string _secretKey;

    public AuthenticationTokenService(ILogger<AuthenticationTokenService> logger)
    {
        var secretKey = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");
        if (secretKey == null)
            throw new Exception("Environment variable DEV_SECRET_KEY is not set");

        _logger = logger;
        _secretKey = secretKey;
    }

    public string? GenerateToken(IdentityUser user)
    {
        try
        {
            var expiration = DateTime.UtcNow.AddDays(_expirationDay);
            if (user.UserName == null) throw new ArgumentException("UserName is null");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.NameIdentifier, user.UserName)
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                SecurityAlgorithms.HmacSha256Signature
            );

            // Generate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = new JwtSecurityToken("chat", "chat", claims, expires: expiration,
                signingCredentials: signingCredentials);

            var token = tokenHandler.WriteToken(jwtToken);
            _logger.LogInformation("Generated token {}", token);
            return token;
        }
        catch (Exception e)
        {
            _logger.LogError("CreateToken({}) error:{}", user.UserName, e.Message);
            return null;
        }
    }

    public string? VerifyToken(string token)
    {
        return null;
    }
}