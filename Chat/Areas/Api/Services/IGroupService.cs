using Chat.Common.Dtos;
using Chat.Common.DTOs;
namespace Chat.Areas.Api.Services;

public interface IGroupService
{
    /// <summary>
    /// Create a group
    /// </summary>
    /// <exception cref="ArgumentException">If the user doesn't exist or group name  doesn't meet the requirements</exception>
    /// <returns>Group entity's DTO</returns>
    public Task<GroupDto> CreateGroup(string username, string groupName);
    /// <summary>
    /// Get a group by id
    /// </summary>
    /// <returns>
    /// Group entity's DTO. Null if group doesn't exist
    /// </returns>
    public Task<GroupDto?> GetGroup(int groupId);
    /// <summary>
    /// Get all groups
    /// </summary>
    /// <returns>A list of groups</returns>
    public Task<IEnumerable<GroupDto>> GetAllGroups();
    /// <summary>
    /// Add a user to a group
    /// </summary>
    /// <exception cref="ArgumentException">
    /// User doesn't exist, group doesn't exist or user is already in the group
    /// </exception>
    /// <returns>Membership id</returns>
    public Task<MembershipDto> AddMember(string username, int groupId);
    /// <summary>
    /// Remove a user from a group
    /// </summary>
    /// <exception cref="ArgumentException">
    /// User doesn't exist, group doesn't exist or user is not in the group
    /// </exception>
    public Task RemoveMember(string username, int groupId);
    /// <summary>
    /// Get all groups that a user has joined
    /// </summary>
    /// <returns>List of groups. Null if the user doesn't exist </returns>
    public Task<IEnumerable<GroupDto>?> GetJoinedGroups(string username);
    /// <summary>
    /// Get all members of a group
    /// </summary>
    /// <returns>List of members. Null if the group doesn't exist</returns>
    public Task<IEnumerable<UserDto>?> GetGroupMembers(int groupId);
}