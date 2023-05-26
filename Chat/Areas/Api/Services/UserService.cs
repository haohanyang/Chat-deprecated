using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using Chat.Areas.Api.Data;
using Chat.Areas.Api.Models;
using Chat.Common.DTOs;
using Chat.Common.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Areas.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private const int ExpirationDay = 30;
    private readonly string? _secretKey = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");
    private readonly UserManager<User> _userManager;

    public UserService(ApplicationDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        var users = await _dbContext.Users.ToListAsync();
        return users;
    }
    
    public async Task<UserDto?> GetUser(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        return user?.ToDto();
    }


    public async Task<IdentityResult> Register(RegisterRequest request)
    {

        if (await _userManager.FindByNameAsync(request.Username) != null)
        {
            throw new ArgumentException("Username " + request.Username + " already exists");
        }
        
        var result = await _userManager.CreateAsync(
            new User
            {
                UserName = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Avatar = "https://api.dicebear.com/6.x/initials/svg?seed=" + request.FirstName[0] + request.LastName[0]
            },
            request.Password);
        return result;
    }

    /// <summary>
    ///     Tries to login with the given username and password. Retrieves the token if the authentication succeeds.
    /// </summary>
    /// <returns>A JSON Web Token that authenticates the user</returns>
    public async Task<(UserDto, string)> Login(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null) throw new AuthenticationException("The username or password is incorrect.");

        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result) throw new AuthenticationException("The username or password is incorrect.");
        return (user.ToDto(), GenerateToken(user));
    }

    /// <summary>
    /// Generate the JWT token associated with <see cref="IdentityUser"/>
    /// </summary>
    /// <param name="user">user</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string GenerateToken(User user)
    {
        var expiration = DateTime.UtcNow.AddDays(ExpirationDay);
        if (user.UserName == null)
            throw new ArgumentException("Username is null");
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Iss, "chat"),
            new(JwtRegisteredClaimNames.Aud, "chat"),
            new(JwtRegisteredClaimNames.Sub, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
        };

        if (_secretKey == null)
            throw new ArgumentException("DEV_SECRET_KEY is not set");

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            SecurityAlgorithms.HmacSha256Signature
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = new JwtSecurityToken("chat", "chat", claims, expires: expiration,
            signingCredentials: signingCredentials);

        var token = tokenHandler.WriteToken(jwtToken);
        return token;
    }
    
    public async Task<TokenValidationResult> ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = GetValidationParameters();

        var result = await tokenHandler.ValidateTokenAsync(token, validationParameters);
        return result;
    }

    private TokenValidationParameters GetValidationParameters()
    {
        if (_secretKey == null)
            throw new ArgumentException("DEV_SECRET_KEY is not set");
        return new TokenValidationParameters()
        {
            ValidateLifetime = false,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = "chat",
            ValidAudience = "chat",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey!)) // The same key as the one that generate the token
        };
    }
}