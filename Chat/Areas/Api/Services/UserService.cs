using Chat.Areas.Api.Data;
using Chat.Common.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Chat.Areas.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsers()
    {
        var users = await _dbContext.Users.ToListAsync();
        return users.Select(u => u.ToDTO());
    }

    public async Task<bool> UserExists(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        return user != null;
    }

    public async Task<UserDTO> GetUser(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
        {
            throw new ArgumentException($"User {username} doesn't exist");
        }
        return user.ToDTO();
    }
}