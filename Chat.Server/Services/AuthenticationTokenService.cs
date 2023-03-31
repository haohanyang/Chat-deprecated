namespace Chat.Server.Services;

using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface IAuthenticationService
{
    public string? CreateToken(IdentityUser user);
}

public class AuthenticationService : IAuthenticationService
{
    private const int ExpirationDay = 30;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(ILogger<AuthenticationService> logger)
    {
        _logger = logger;
    }
    
    private List<Claim> CreateClaims(IdentityUser user)
    {
        _ = user.UserName ?? throw new ArgumentException("Username is empty");
        return new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
        };
    }

    private SigningCredentials CreateCredentials()
    {
        var key = Encoding.ASCII.GetBytes("!SomethingSecret!");
        return new SigningCredentials(
            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature
        );
    }
    
    public string? CreateToken(IdentityUser user)
    {
        var expiration = DateTime.UtcNow.AddDays(ExpirationDay);
        try
        {
            // Claims
            var claims = CreateClaims(user);

            // Signing credentials
            var signingCredentials = CreateCredentials();
            
            // Generate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = new JwtSecurityToken("apiWithAuthBackend", "apiWithAuthBackend", claims, expires: expiration,
                signingCredentials: signingCredentials);

            return tokenHandler.WriteToken(jwtToken);
        }
        catch (ArgumentException e)
        {
            _logger.LogError("CreateToken error: Username of user(id:{id}) doesn't exist", user.Id);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError("CreateToken error: {message}", e.Message);
            return null;
        }
    }
}