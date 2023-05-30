using Chat.Common.Dto;

namespace Chat.Services.Interface;

public interface IMessageService
{

    public Task<MessageDto> SaveUserMessage(MessageDto message);
    public Task<MessageDto> SaveGroupMessage(MessageDto message);

    public Task<IEnumerable<MessageDto>> GetUserMessages(int channelId);

    public Task<IEnumerable<MessageDto>> GetGroupMessages(int channelId);
}
