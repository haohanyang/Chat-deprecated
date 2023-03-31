using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Chat.Server.Services;

public interface IDatabaseService
{
    //public void CreateGroup(string username, string groupId);
    public void JoinGroup(string username, string groupId);
    public Group? GetGroup(string groupId);
    public ApplicationUser? GetUser(string username);
}

public class DatabaseService : IDatabaseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public DatabaseService(ApplicationDbContext dbContext,
        ILogger<DatabaseService> logger, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
    }

    public void JoinGroup(string username, string groupId)
    {
        try
        {
            // Can probably skip this due to the foreign key constraint 
            var group = _dbContext.Groups.Find(groupId);
            if (group == null)
            {
                _logger.LogError("JoinGroup Error: g/{} doesn't exist", groupId);
                throw new ArgumentException("g/" + groupId + " doesn't exist");
            }
            //
            // _dbContext.Memberships.Add(new Membership
            // {
            //     MemberId = username,
            //     GroupId = groupId
            // });

            _dbContext.SaveChanges();
            _logger.LogInformation("u/{} joins group /g{}", username, groupId);
        }
        catch (Exception e)
        {
            _logger.LogError("JoinGroup Error: {}", e.Message);
        }
    }

    public Group? GetGroup(string groupId)
    {
        return _dbContext.Groups.Find(groupId);
    }

    public ApplicationUser? GetUser(string username)
    {
        return _userManager.FindByNameAsync(username).Result;
    }

    public void CreateGroup(string groupId)
    {
        try
        {
            _dbContext.Groups.Add(new Group { Id = groupId });
            _dbContext.SaveChanges();
            _logger.LogInformation("g/{} is created", groupId);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateGroup Error: {}", e.Message);
        }
    }
}