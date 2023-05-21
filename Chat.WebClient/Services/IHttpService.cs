using Chat.Common.Dtos;
using Chat.Common.DTOs;

namespace Chat.WebClient.Services;

public interface IHttpService
{
    public string GetServerUrl();
    public string? GetAccessToken();
    public Task<IEnumerable<UserDto>> GetAllUserContacts();

    public Task<IEnumerable<GroupDto>> GetAllGroupContacts(string username);

    public Task<List<UserMessageDto>> GetUserChat(string username1, string username2);
    public Task<List<GroupMessageDto>> GetGroupChat(int groupId);

    public Task SendUserMessage(UserMessageDto message);

    public Task SendGroupMessage(GroupMessageDto message);

    public Task<UserDto> GetCurrentUser();

    public Task<GroupDto> CreateGroup(string username, string name);
    public Task<MembershipDto> JoinGroup(string username, int groupId);

    public Task<GroupDto?> GetGroup(int groupId);
}