using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public interface IMessageService
{
    public Task<int> SaveMessage(MessageDTO message);
    public Task<IEnumerable<UserMessageDTO>> GetUserChat(string username1, string username2);
    public Task<IEnumerable<GroupMessageDTO>> GetGroupChat(int id);
}
