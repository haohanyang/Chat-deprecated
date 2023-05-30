using Chat.Common.Dto;
using Chat.Data;
using Chat.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Chat.CrossCutting.Exceptions;
namespace Chat.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsers()
    {
        var users = await _dbContext.Users.ToListAsync();
        return users.Select(u => u.ToDto());
    }

    public async Task<UserDto> GetUser(string username)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(e => e.UserName == username);
        if (user == null)
        {
            throw new UserNotFoundException(username);
        }
        return user.ToDto();
    }
}