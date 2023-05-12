
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Chat.Areas.Api.Services;
public interface IUserService
{
    public Task<IEnumerable<UserDTO>> GetAllUsers();
    public Task<bool> UserExists(string username);
    public Task<UserDTO> GetUser(string username);
    public Task<IdentityResult> Register(RegistrationRequest request);

    public Task<string> Login(LoginRequest request);
    public Task<TokenValidationResult> ValidateToken(string token);
}