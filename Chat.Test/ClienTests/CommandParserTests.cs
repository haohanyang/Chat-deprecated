using Chat.Client.Command;
using Chat.Common;
using Xunit;

namespace Chat.Test.ClienTests;

public class CommandParserTests
{
    [Fact]
    public void LoginCommandTest()
    {
        const string commandStr = "login username pass@word";
        var command = CommandParser.Parse(commandStr) as LoginCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command);
            Assert.Equal("username", command!.Username);
            Assert.Equal("pass@word", command.Password);
        });
    }

    [Fact]
    public void SendCommandTest()
    {
        const string commandStr1 = " send   u/user1  'my-message' ";
        var command1 = CommandParser.Parse(commandStr1) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command1);
            Assert.Equal(MessageType.UserMessage, command1!.MessageType);
            Assert.Equal("user1", command1.Receiver);
            Assert.Equal("my-message", command1.Message);
        });

        const string commandStr2 = "send   g/group1 'this' ";
        var command2 = CommandParser.Parse(commandStr2) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command2);
            Assert.Equal(MessageType.GroupMessage, command2!.MessageType);
            Assert.Equal("group1", command2.Receiver);
            Assert.Equal("this", command2.Message);
        });
    }

    [Fact]
    public void CreateGroupCommandTest()
    {
        const string commandStr = "create  g/this-is-my-group1 ";
        var command = CommandParser.Parse(commandStr) as CreateGroupCommand;
        Assert.NotNull(command);
        Assert.Equal("this-is-my-group1", command!.GroupId);
    }
}