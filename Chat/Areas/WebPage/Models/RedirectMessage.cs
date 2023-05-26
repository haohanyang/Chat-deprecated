namespace Chat.Areas.WebPage.Models;

public enum RedirectMessageType {
    Error,
    Success
}

public class RedirectMessage
{
    public RedirectMessageType Type {get; set;}
    public string Message {get;set;} = string.Empty;
}