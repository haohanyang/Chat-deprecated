using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public interface IMessageService
{
    public Task<int> SaveMessage(MessageDTO message);
    public Task<IEnumerable<MessageDTO>> GetAllMessages(string username);
    public Task<IEnumerable<MessageDTO>> GetAllMessagesBetween(string username1, string username2);
}
