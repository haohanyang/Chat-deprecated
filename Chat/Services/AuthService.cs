using Chat.Common.Dto;
using Chat.Common.Http;
using Chat.Data;
using Chat.Domain;
using Chat.CrossCutting.Exceptions;
using Chat.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly string? _secretKey = Environment.GetEnvironmentVariable("DEV_SECRET_KEY");
    private const int ExpirationDay = 30;

    public AuthService(ApplicationDbContext dbContext, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task<string> Authenticate(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<UserDto> CreateUser(RegisterRequest request)
    {
        if (await _userManager.FindByNameAsync(request.Username) != null)
        {
            throw new ArgumentException("Username " + request.Username + " already exists");
        }
        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AvatarUrl = "https://api.dicebear.com/6.x/initials/svg?seed=" + request.FirstName[0] + request.LastName[0]
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            return user.ToDto();
        }
        else
        {
            var errors = string.Join(";", result.Errors.Select(e => e.Description));
            throw new ArgumentException(errors);
        }
    }

    public Task<TokenValidationResult> ValidateToken(string token)
    {
        throw new NotImplementedException();
    }
}