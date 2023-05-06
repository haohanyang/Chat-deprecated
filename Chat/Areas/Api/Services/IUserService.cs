
using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;
public interface IUserService
{
    public Task<IEnumerable<UserDTO>> GetAllUsers();
    public Task<bool> UserExists(string username);
}