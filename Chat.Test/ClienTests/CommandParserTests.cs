using Chat.Client.Command;
using Chat.Common;
using NUnit.Framework;

namespace Chat.Test.ClienTests;

public class CommandParserTests
{
    [Test]
    public void LoginCommandTest()
    {
        const string commandStr1 = "login abc";
        var command1 = CommandParser.parse(commandStr1) as LoginCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command1);
            Assert.IsNull(command1.Password);
            Assert.That(command1!.Username, Is.EqualTo("abc"));
        });
        
        const string commandStr2 = "login username pass@word";
        var command2 = CommandParser.parse(commandStr2) as LoginCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command2);
            Assert.That(command2!.Username, Is.EqualTo("username"));
            Assert.That(command2!.Password, Is.EqualTo("pass@word"));
        });

    }
    
    [Test]
    public void SendCommandTest()
    {
        const string commandStr1 = " send   u/user1  'my-message' ";
        var command1 = CommandParser.parse(commandStr1) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command1);
            Assert.That(command1!.ReceiverType, Is.EqualTo(ReceiverType.User));
            Assert.That(command1.Receiver, Is.EqualTo("user1"));
            Assert.That(command1.Message, Is.EqualTo("my-message"));
        });

        const string commandStr2 = "send   g/group1 'this' ";
        var command2 = CommandParser.parse(commandStr2) as SendMessageCommand;
        Assert.Multiple(() =>
        {
            Assert.NotNull(command2);
            Assert.That(command2!.ReceiverType, Is.EqualTo(ReceiverType.Group));
            Assert.That(command2.Receiver, Is.EqualTo("group1"));
            Assert.That(command2.Message, Is.EqualTo("this"));
        });
    }
    
    [Test]
    public void CreateGroupCommandTest()
    {
        const string commandStr = "create  g/this-is-my-group1 ";
        var command = CommandParser.parse(commandStr) as CreateGroupCommand;
        Assert.NotNull(command);
        Assert.That(command!.GroupId, Is.EqualTo("this-is-my-group1"));
    }
}