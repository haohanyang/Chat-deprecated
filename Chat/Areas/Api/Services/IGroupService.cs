using Chat.Common.DTOs;
namespace Chat.Areas.Api.Services;

public interface IGroupService
{
    public Task<int> CreateGroup(string username, string groupName);
    public Task<GroupDTO> GetGroup(int groupId);
    public Task<IEnumerable<GroupDTO>> GetAllGroups();
    public Task LeaveGroup(string username, int groupId);
    public Task<int> JoinGroup(string username, int groupId);
    public Task<IEnumerable<GroupDTO>> GetJoinedGroups(string username);
    public Task<IEnumerable<UserDTO>> GetGroupMembers(int groupId);
}