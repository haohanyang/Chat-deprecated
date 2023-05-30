
using Chat.Common.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;

namespace Chat.Services.Interface;
public interface IUserService
{
    public Task<IEnumerable<UserDto>> GetAllUsers();
    public Task<UserDto> GetUser(string username);
}
