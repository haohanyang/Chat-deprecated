using Chat.Client.Command;
using Chat.Common;
using NUnit.Framework;

namespace Chat.Test.ClienTests;

public class CommandParserTests
{
    [Test]
    public void LoginCommandTest()
    {
        const string commandStr = "login username pass@word";
        var command = CommandParser.Parse(commandStr) as LoginCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command);
            Assert.That(command!.Username, Is.EqualTo("username"));
            Assert.That(command.Password, Is.EqualTo("pass@word"));
        });
    }

    [Test]
    public void SendCommandTest()
    {
        const string commandStr1 = " send   u/user1  'my-message' ";
        var command1 = CommandParser.Parse(commandStr1) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command1);
            Assert.That(command1!.MessageType, Is.EqualTo(MessageType.UserMessage));
            Assert.That(command1.Receiver, Is.EqualTo("user1"));
            Assert.That(command1.Message, Is.EqualTo("my-message"));
        });

        const string commandStr2 = "send   g/group1 'this' ";
        var command2 = CommandParser.Parse(commandStr2) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command2);
            Assert.That(command2!.MessageType, Is.EqualTo(MessageType.GroupMessage));
            Assert.That(command2.Receiver, Is.EqualTo("group1"));
            Assert.That(command2.Message, Is.EqualTo("this"));
        });
    }

    [Test]
    public void CreateGroupCommandTest()
    {
        const string commandStr = "create  g/this-is-my-group1 ";
        var command = CommandParser.Parse(commandStr) as CreateGroupCommand;
        Assert.NotNull(command);
        Assert.That(command!.GroupId, Is.EqualTo("this-is-my-group1"));
    }
}