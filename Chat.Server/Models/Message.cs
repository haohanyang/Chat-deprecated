using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Server.Models;

public class UserMessage
{
    public string UserMessageId { get; set; }
    
    [ForeignKey("FromId")]
    public string FromId { get; set; }
    public virtual Member From { get; set; }
    
    [ForeignKey("ToId")]
    public string ToId { get; set; }
    public virtual Member To { get; set; }
    
    public string Content { get; set; }
    public DateTime Time { get; set; }
}

public class GroupMessage
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string GroupMessageId { get; set; }
    
    [ForeignKey("FromId")]
    public string FromId { get; set; }
    public virtual Member From { get; set; }
    
    
    [ForeignKey("ToId")]
    public string ToId { get; set; }
    public virtual Group To { get; set; }
    
    public string Content { get; set; }
    public DateTime Time { get; set; }
}