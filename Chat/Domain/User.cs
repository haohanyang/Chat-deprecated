using Chat.Common.Dto;
using Microsoft.AspNetCore.Identity;

namespace Chat.Domain;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;
    public IEnumerable<GroupChannelMembership> GroupChannelMemberships { get; } = new List<GroupChannelMembership>();
    public IEnumerable<UserChannel> UserChannels1 { get; } = new List<UserChannel>();
    public IEnumerable<UserChannel> UserChannels2 { get; } = new List<UserChannel>();
    public IEnumerable<GroupChannel> CreatedGroupChannels { get; } = new List<GroupChannel>();

    public UserDto ToDto()
    {
        return new()
        {
            Id = Id,
            Username = UserName!,
            Name = $"{FirstName} {LastName}",
            AvatarUrl = AvatarUrl,
            IsOnline = false
        };
    }
}