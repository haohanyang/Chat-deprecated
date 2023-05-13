using Chat.Common.DTOs;

namespace Chat.WebClient.Services;

public interface IHttpService
{
    public string GetServerUrl();
    public string? GetAccessToken();
    public Task<IEnumerable<UserDTO>> GetAllUserContacts();

    public Task<IEnumerable<GroupDTO>> GetAllGroupContacts(string username);

    public Task<List<UserMessageDTO>> GetUserChat(string username1, string username2);
    public Task<List<GroupMessageDTO>> GetGroupChat(int groupId);

    public Task SendUserMessage(UserMessageDTO message);

    public Task SendGroupMessage(GroupMessageDTO message);

    public Task<UserDTO> GetCurrentUser();
}