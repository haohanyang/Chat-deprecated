using Chat.Client.Command;
using Chat.Common;
using NUnit.Framework;

namespace Chat.Test.ClienTests;

public class CommandParserTests
{
    private const string SendUserCommand1 = "send u/user1 'my-message'";
    private const string SendGroupCommand1 = "send g/this-is-my-group1 'my-message'";

    private const string CreateGroupCommand1 = "create g/this-is-my-group1";
    private const string JoinGroupCommand1 = "join g/this-is-my-group1";

    [Test]
    public void SendUserCommandTest1()
    {
        var command = CommandParser.parse(SendUserCommand1) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command);
            Assert.That(command!.ReceiverType, Is.EqualTo(ReceiverType.User));
            Assert.That(command.Receiver, Is.EqualTo("user1"));
            Assert.That(command.Message, Is.EqualTo("my-message"));
        });
    }

    [Test]
    public void SendGroupCommandTest1()
    {
        var command = CommandParser.parse(SendGroupCommand1) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command);
            Assert.That(command!.ReceiverType, Is.EqualTo(ReceiverType.Group));
            Assert.That(command.Receiver, Is.EqualTo("this-is-my-group1"));
            Assert.That(command.Message, Is.EqualTo("my-message"));
        });
    }

    [Test]
    public void CreateGroupCommandTest1()
    {
        var command = CommandParser.parse(CreateGroupCommand1) as CreateGroupCommand;
        Assert.NotNull(command);
        Assert.That(command!.GroupId, Is.EqualTo("this-is-my-group1"));
    }

    [Test]
    public void JoinGroupCommandTest1()
    {
        var command = CommandParser.parse(JoinGroupCommand1) as JoinGroupCommand;
        Assert.NotNull(command);
        Assert.That(command!.GroupId, Is.EqualTo("this-is-my-group1"));
    }
}