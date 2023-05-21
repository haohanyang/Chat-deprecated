namespace Chat.Common.DTOs;

public class GroupDTO : BaseDTO
{
    public int Id { get; set; }
    public UserDTO Owner { get; set; } = new();
    public List<UserDTO> Members { get; set; } = new List<UserDTO>();
    public DateTime CreatedAt { get; set; }
}