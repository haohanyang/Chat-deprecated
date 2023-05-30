using Chat.Common.Dto;
using Chat.Common.Http;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Services.Interface;

public interface IAuthService
{
    Task<UserDto> CreateUser(RegisterRequest request);
    Task<AuthResponse> Authenticate(LoginRequest request);
    public Task<TokenValidationResult> ValidateToken(string token);
}