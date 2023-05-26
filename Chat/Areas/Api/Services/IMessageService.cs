using Chat.Common.DTOs;

namespace Chat.Areas.Api.Services;

public interface IMessageService
{
    /// <summary>
    /// Save the given message to the database
    /// </summary>
    /// <exception cref="ArgumentException">If sender or receiver doesn't exist</exception>
    /// <returns>Message entity's DTO</returns>
    public Task<MessageDto> SaveMessage(MessageDto message);
    
    /// <summary>
    /// Get the chat between the two users
    /// </summary>
    /// <returns>A list of messages. Null if any user doesn't exist</returns>
    public Task<IEnumerable<UserMessageDto>?> GetUserChat(string username1, string username2);
    
    /// <summary>
    /// Get the chat in the group
    /// </summary>
    /// <returns>A list of messages. Null if the group doesn't exist</returns>
    public Task<IEnumerable<GroupMessageDto>?> GetGroupChat(int id);
}
