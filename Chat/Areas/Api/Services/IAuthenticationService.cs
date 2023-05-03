using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace Chat.Areas.Api.Services;

public interface IAuthenticationService
{
    public Task<IdentityResult> Register(string username, string email, string password);

    public Task<string> Login(string username, string password);
    public Task<TokenValidationResult> ValidateToken(string token);
}