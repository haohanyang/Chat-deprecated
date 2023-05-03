using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public interface IMessageService
{
    public Task<int> SaveMessage(MessageDTO message);
}
