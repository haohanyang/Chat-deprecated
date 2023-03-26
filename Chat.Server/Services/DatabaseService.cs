using System.Security.Claims;
using Chat.Common;
using Chat.Server.Data;
using Chat.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Server.Services;

public interface IDatabaseService
{
    public void CreateGroup(string username, string groupId);
    public void JoinGroup(string username, string groupId);
}
public class DatabaseService : IDatabaseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(ApplicationDbContext dbContext, ILogger<DatabaseService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public void CreateGroup(string username, string groupId)
    {
        try
        {
            _dbContext.Groups.Add(new Group { GroupId = groupId });
            _dbContext.SaveChanges();
            _logger.LogInformation("u/{} creates group g/{}", username, groupId);
        }
        catch (Exception e)
        {
            _logger.LogError("CreateGroup Error: {}", e.Message);
        }
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
            
            _dbContext.Memberships.Add(new Membership
            {
                MemberId = username,
                GroupId = groupId
            });
            
            _dbContext.SaveChanges();
            _logger.LogInformation("u/{} joins group /g{}", username, groupId);
            
        }
        catch (Exception e)
        {
            _logger.LogError("JoinGroup Error: {}", e.Message);
        }
    }
}