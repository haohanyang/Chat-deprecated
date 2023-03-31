using System.Text.RegularExpressions;
using Chat.Common;

namespace Chat.Client.Command;

public class CommandParser
{
    // Register format 
    // Register <username> <password>
    private const string RegisterPattern = @"^register\s+([a-zA-Z0-9\-_]+)\s+(\S+)$"; 
    
    // Login format
    // login <username> <password>
    private const string LoginPattern = @"^login\s+([a-zA-Z0-9\-_]+)\s+(\S+)$"; 
    // Send message format:
    // send u/<user-id> '<message>'
    // send g/<group-id> '<message>'
    private const string SendMessagePattern = @"^send\s+([ug])\/([a-zA-Z0-9\-_]+)\s+'(.+)'$";
    
    // Create group format:
    // create u/<group-id>
    private const string CreateGroupPattern = @"^create\s+g\/([a-zA-Z0-9\-_]+)$";
    
    // Join group format:
    // join u/<group-id>
    private const string JoinGroupPattern = @"^join\s+g\/([a-zA-Z0-9\-_]+)$";
    
    // Leave group format:
    // leave u/<group-id>
    private const string LeaveGroupPattern = @"^leave\s+g\/([a-zA-Z0-9\-_]+)$";

    private const string ExitPattern = @"^([qQ]|exit|quit)$";
    
    public static ICommand? Parse(string? input)
    {
        if (input == null)
        {
            return null;
        }

        input = input.Trim();
        
        var match = Regex.Match(input, RegisterPattern);
        if (match.Success)
        {
            var username = match.Groups[1].Value;
            var password = match.Groups[2].Value;
            return new RegisterCommand { Username = username, Password = password};
        }
        
        match = Regex.Match(input, LoginPattern);
        if (match.Success)
        {
            var username = match.Groups[1].Value;
            var password = match.Groups[2].Value;
            return new LoginCommand { Username = username, Password = password};
        }
        
        match = Regex.Match(input, SendMessagePattern);
        if (match.Success)
        {
            var receiverType = match.Groups[1].Value;
            var receiver = match.Groups[2].Value;
            var message = match.Groups[3].Value;

            if (receiverType == "u")
            {
                return new SendMessageCommand
                    { ReceiverType = ReceiverType.User, Receiver = receiver, Message = message };
            }

            return new SendMessageCommand
                { ReceiverType = ReceiverType.Group, Receiver = receiver, Message = message };
        }

        match = Regex.Match(input, JoinGroupPattern);
        if (match.Success)
        {
            var groupId = match.Groups[1].Value;
            return new JoinGroupCommand { GroupId = groupId };
        }
        
        match = Regex.Match(input, LeaveGroupPattern);
        if (match.Success)
        {
            var groupId = match.Groups[1].Value;
            return new LeaveGroupCommand { GroupId = groupId };
        }

        match = Regex.Match(input, CreateGroupPattern);
        if (match.Success)
        {
            var groupId = match.Groups[1].Value;
            return new CreateGroupCommand { GroupId = groupId };
        }

        match = Regex.Match(input, ExitPattern);
        if (match.Success)
        {
            return new ExitCommand();
        }

        return null;
    }
}