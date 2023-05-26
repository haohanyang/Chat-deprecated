
using Chat.Areas.Api.Models;
using Chat.Common.DTOs;
using Chat.Common.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;

namespace Chat.Areas.Api.Services;
public interface IUserService
{
    public Task<IEnumerable<User>> GetAllUsers();
    /// <summary>
    /// Gets the user with the given username
    /// </summary>
    /// <returns cref="UserDto">User's DTO. Null if the user doesn't exist</returns>
    public Task<UserDto?> GetUser(string username);
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <exception cref="ArgumentException"> If username exists </exception>
    /// <returns>The <see cref="IdentityResult" /> that indicates success or failure.</returns>
    public Task<IdentityResult> Register(RegisterRequest request);
    
    /// <summary>
    /// Tries to log in with credentials.
    /// </summary>
    /// <returns>User entity's DTO and JWT token if login succeeded</returns>
    /// <exception cref="AuthenticationException">If login fails</exception>
    
    public Task<(UserDto, string)> Login(LoginRequest request);
    /// <summary>
    /// Validates the given token
    /// </summary>
    /// <returns cref="TokenValidationResult">Validation result</returns>
    public Task<TokenValidationResult> ValidateToken(string token);
}
