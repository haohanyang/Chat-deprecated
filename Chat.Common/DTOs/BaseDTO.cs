namespace Chat.Common.DTOs;

public class BaseDTO : IEquatable<BaseDTO>

{
    public string ClientId { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public bool Equals(BaseDTO? other)
    {
        if (other is null)
            return false;

        if (Object.ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.GetType() != other.GetType())
        {
            return false;
        }

        return this.ClientId == other.ClientId;
    }
}