using Chat.Client.Command;
using Chat.Common;
using Chat.Common.Configs;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client;

public class ChatClient
{
    private List<string> _messages = new List<string>();
    private string _userId;
    private HubConnection? _connection;

    private readonly object _messagesLock = new object();

    public ChatClient(string userId)
    {
        _userId = userId;
    }

    private void InitConnection(string url)
    {
        _connection = new HubConnectionBuilder().WithUrl(url).Build();

        _connection.On<string>("RpcSucceeds", (message) =>
        {
            Console.ForegroundColor = ClientConfigs.RpcSucceedColor;
            Console.WriteLine(message);
            Console.ResetColor();
        });

        _connection.On<string>("RpcFails", (message) =>
        {
            Console.ForegroundColor = ClientConfigs.RpcFailColor;
            Console.WriteLine(message);
            Console.ResetColor();
        });

        _connection.On<string>("RpcWarns", (message) =>
        {
            Console.ForegroundColor = ClientConfigs.RpcFailColor;
            Console.WriteLine(message);
            Console.ResetColor();
        });

        _connection.On<Notification>("ReceiveNotification", notification =>
        {
            Console.ForegroundColor = ClientConfigs.NotificationColor;
            Console.WriteLine($"{notification.Time} {notification.Content}");
            Console.ResetColor();
        });

        _connection.On<Message>("ReceiveUserMessage", message =>
        {
            var newMessage = $"{message.Time} u/{message.From}:{message.Content}";

            Console.ResetColor();
            Console.Write(ClientConfigs.SpeechBalloon);
            if (ClientConfigs.MessageColor != ConsoleColor.Black)
            {
                Console.ForegroundColor = ClientConfigs.MessageColor;
            }

            Console.WriteLine(" " + newMessage);
            if (ClientConfigs.MessageColor != ConsoleColor.Black)
            {
                Console.ResetColor();
            }
        });

        _connection.On<Message>("ReceiveGroupMessage", message =>
        {
            var newMessage = $"{message.Time} g/{message.To} u/{message.From}:{message.Content}";
            Console.ForegroundColor = ClientConfigs.MessageColor;
            Console.ResetColor();
            Console.Write(ClientConfigs.SpeechBalloon);
            Console.ForegroundColor = ClientConfigs.MessageColor;
            Console.WriteLine(" " + newMessage);
            Console.ResetColor();
        });
    }

    private async Task Connect()
    {
        if (_connection != null)
        {
            await _connection.StartAsync();
        }
    }


    private async Task MessageInputLoop()
    {
        while (_connection is not null && _connection.State == HubConnectionState.Connected)
        {
            var input = Console.ReadLine();
            ICommand? command = CommandParser.parse(input!.Trim());

            if (command is null)
            {
                Console.WriteLine("Invalid input");
                continue;
            }


            if (command is ExitCommand _)
            {
                await _connection.StopAsync();
                break;
            }

            if (command is SendMessageCommand sendMessageCommand)
            {
                await SendMessage(sendMessageCommand.Receiver, sendMessageCommand.ReceiverType,
                    sendMessageCommand.Message);
            }

            if (command is CreateGroupCommand createGroupCommand)
            {
                await CreateGroup(createGroupCommand.GroupId);
            }

            if (command is JoinGroupCommand joinGroupCommand)
            {
                await JoinGroup(joinGroupCommand.GroupId);
            }
        }
    }

    public async Task SendMessage(string receiverId, ReceiverType type, string message)
    {
        //var m = new Message(_userId, receiverId, DateTime.Now, type,message);
        if (type == ReceiverType.User)
        {
            await _connection.InvokeAsync("SendUserMessage", receiverId, message);
        }
        else
        {
            await _connection.InvokeAsync("SendGroupMessage", receiverId, message);
        }
    }

    public async Task CreateGroup(string groupId)
    {
        await _connection.InvokeAsync("CreateGroup", groupId);
    }

    public async Task JoinGroup(string groupId)
    {
        await _connection.InvokeAsync("JoinGroup", groupId);
    }

    public async Task LeaveGroup(string groupId)
    {
        await _connection.InvokeAsync("LeaveGroup", groupId);
    }

    public async Task Run()
    {
        var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (url is null)
        {
            Console.Error.WriteLine("Env ASPNETCORE_URLS is not set");
            return;
        }

        InitConnection($"{url}/Chat?username={_userId}");
        try
        {
            await Connect();
            await MessageInputLoop();
        }
        catch (HttpRequestException)
        {
            Console.Error.WriteLine("Connection to server fails");
        }
    }
}