using Chat.Common.Dto;
namespace Chat.Domain;

public class UserChannel : Channel
{
    // The channel between two users is uniquely identified by the two users
    // User1.username < User2.username
    public string User1Id { get; set; } = string.Empty;
    public User User1 { get; set; } = null!;

    public string User2Id { get; set; } = string.Empty;
    public User User2 { get; set; } = null!;

    public IEnumerable<UserMessage> Messages { get; } = new List<UserMessage>();

    public UserChannel(User user1, User user2)
    {
        if (string.Compare(user1.UserName, user2.UserName) < 0)
        {
            User1 = user1;
            User2 = user2;
        }
        else
        {
            User1 = user2;
            User2 = user1;
        }
    }

    public UserChannelDto ToDto()
    {
        return new()
        {
            Id = Id,
            User1 = User1?.ToDto(),
            User2 = User2?.ToDto(),
        };
    }


}