using Chat.Common.DTOs;
namespace Chat.Areas.Api.Services;

public interface IGroupService
{
    public Task<int> CreateGroup(string username, string groupName);
    public Task<GroupDTO> GetGroup(string groupName);
    public Task<IEnumerable<GroupDTO>> GetAllGroups();
    public Task LeaveGroup(string username, string groupName);
    public Task<int> JoinGroup(string username, string groupName);
    public Task<IEnumerable<string>> GetJoinedGroups(string username);
    public Task<IEnumerable<string>> GetGroupMembers(string groupName);
    public Task<bool> GroupExists(string groupName);
}