namespace Chat.Common.DTOs;

public enum RedirectMessageType {
    ERROR,
    SUCCESS
}

public class RedirectMessage {
   public RedirectMessageType Type {get; set;}
   public string Message {get;set;} = string.Empty;
}