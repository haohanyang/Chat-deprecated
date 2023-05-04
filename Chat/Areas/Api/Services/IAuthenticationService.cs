using Chat.Common.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace Chat.Areas.Api.Services;

public interface IAuthenticationService
{
    public Task<IdentityResult> Register(AuthenticationRequest request);

    public Task<string> Login(AuthenticationRequest request);
    public Task<TokenValidationResult> ValidateToken(string token);
}