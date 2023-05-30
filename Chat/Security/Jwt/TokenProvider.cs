using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using Chat.CrossCutting.Exceptions;
using System.Text;

namespace Chat.Security.Jwt;

public interface ITokenProvider
{
    string CreateToken(IPrincipal principal, bool rememberMe);
}

public class TokenProvider : ITokenProvider
{
    private const string AuthoritiesKey = "auth";

    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

    private readonly ILogger<TokenProvider> _logger;

    private SigningCredentials _key;

    private long _tokenValidityInSeconds = 24 * 3600; // 24 hours

    private long _tokenValidityInSecondsForRememberMe;


    public TokenProvider(ILogger<TokenProvider> logger)
    {
        _logger = logger;
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        Init();
    }

    public string CreateToken(IPrincipal principal, bool rememberMe)
    {
        var subject = CreateSubject(principal);
        var validity =
            DateTime.UtcNow.AddSeconds(_tokenValidityInSeconds);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = validity,
            SigningCredentials = _key
        };

        var token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
        return _jwtSecurityTokenHandler.WriteToken(token);
    }

    private void Init()
    {
        byte[] keyBytes;
        var secret = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");

        if (secret == null)
        {
            throw new EnvironmentVariableException("DEV_SECRET_KEY");
        }

        keyBytes = Encoding.ASCII.GetBytes(secret);

        _key = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature);
    }

    private static ClaimsIdentity CreateSubject(IPrincipal principal)
    {
        var username = principal.Identity.Name;
        var roles = GetRoles(principal);
        var authValue = string.Join(",", roles.Select(it => it.Value));
        return new ClaimsIdentity(new[] {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(AuthoritiesKey, authValue)
        });
    }

    private static IEnumerable<Claim> GetRoles(IPrincipal principal)
    {
        return principal is ClaimsPrincipal user
            ? user.FindAll(it => it.Type == ClaimTypes.Role)
            : Enumerable.Empty<Claim>();
    }
}