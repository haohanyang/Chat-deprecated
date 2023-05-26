namespace Chat.Common.Dtos;

/// <summary>
/// Base class for all UserDto and GroupDto
/// </summary>
public class ContactDto
{
    /// <summary>
    /// Unique identifier for a contact among all contacts.
    /// This is used only on client side.
    /// </summary>
    public  string ClientId { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name of a user or a group.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Url to the avatar of a user or a group.
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
    
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is ContactDto other)
        {
            return ClientId == other.ClientId;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return ClientId.GetHashCode();
    }
}   