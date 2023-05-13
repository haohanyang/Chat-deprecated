using System.ComponentModel.DataAnnotations;

namespace Chat.Common.DTOs;



public class CreateGroupRequest
{
    [Required]
    [MinLength(4)]
    public string GroupName { get; set; } = string.Empty;
    public IEnumerable<string> Members { get; set; } = Enumerable.Empty<string>();
}