using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Chat.Client.Command;
using Chat.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client;

public class ChatClient
{
    private readonly string _baseUrl; //"localhost:5000";
    private HubConnection? _connection;
    private string? _token;
    private string? _username;
    // private string _serverAddress = "localhost:5000";

    public ChatClient(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    private static void PrintError(string message)
    {
        Console.WriteLine($"{Colors.RED}{message}{Colors.NORMAL}");
    }

    private static void PrintWarning(string message)
    {
        Console.WriteLine($"{Colors.YELLOW}{message}{Colors.NORMAL}");
    }

    private static void PrintSuccess(string message)
    {
        Console.WriteLine($"{Colors.GREEN}{message}{Colors.NORMAL}");
    }

    private async Task<string?> GetToken(string username, string password)
    {
        using var httpClient = new HttpClient();
        var request = new AuthenticationRequest { Username = username, Password = password };
        var response = await httpClient.PostAsJsonAsync(_baseUrl + "/api/login",
            request);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            return authResponse?.Token;
        }

        return null;
    }

    private async Task Login(string username, string password)
    {
        try
        {
            using var httpClient = new HttpClient();
            var request = new AuthenticationRequest { Username = username, Password = password };
            var response = await httpClient.PostAsJsonAsync( _baseUrl + "/api/login",
                request);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                _token = authResponse!.Token;
                _username = username;
                _connection = new HubConnectionBuilder().WithUrl(_baseUrl + "/chat", options =>
                {
                    options.AccessTokenProvider = () => 
                    Task.FromResult(GetToken(username, password).Result);
                }).Build();
                
                Subscribe(_connection);
                await _connection.StartAsync();
                PrintSuccess("ok");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                PrintError(errorMessage);
            }
        }
        catch (Exception e)
        {
            PrintError("Unexpected error: " + e.Message);
        }
    }


    private async Task Register(string username, string password)
    {
        try
        {
            using var httpClient = new HttpClient();
            var request = new AuthenticationRequest { Username = username, Password = password };
            var response = await httpClient.PostAsJsonAsync(  _baseUrl + "/api/register",
                request);

            if (response.IsSuccessStatusCode)
            {
                PrintSuccess("Ok");
                return;
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            PrintError(errorMessage);
        }
        catch (Exception e)
        {
            PrintError("Unexpected error: "+e.Message);
        }
    }

    private async Task Exit()
    {
        if (_connection != null) 
            await _connection.StopAsync();
    }

    private static void Subscribe(HubConnection connection)
    {
        connection.On<Message>(ChatEvents.ReceiveMessage, message =>
        {
            var stringMessage = message.Type == MessageType.UserMessage
                ? $"{message.Time} u/{message.Sender}:{message.Content}"
                : $"{message.Time} g/{message.Receiver} u/{message.Sender}:{message.Content}";

            Console.Write(new Rune(0x1f4ac));
            Console.WriteLine(" " + stringMessage);
        });

        connection.On<Notification>(ChatEvents.ReceiveNotification, notification =>
        {
            Console.Write(new Rune(0x2139));
            Console.WriteLine(notification.Content);
        });

        connection.On<RpcResponse>(ChatEvents.RpcResponse, response =>
        {
            if (response.Status == RpcResponseStatus.Error)
                PrintError(response.Message);
            else if (response.Status == RpcResponseStatus.Warning)
                PrintWarning(response.Message);
            else
                PrintSuccess("ok");
        });
    }

    private bool IsConnected()
    {
        return _connection?.State == HubConnectionState.Connected;
    }

    private bool IsAuthenticated()
    {
        return _username != null && _token != null;
    }

    
    private HttpClient GetHttpClient()
    {
        var httpClient = new HttpClient();
        if (IsAuthenticated())
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer", _token);
        return httpClient;
    }

    private async Task MainLoop()
    {
        while (true)
        {
            var input = Console.ReadLine();
            var command = CommandParser.Parse(input);

            if (command == null)
            {
                Console.WriteLine("Invalid input");
                continue;
            }

            if (command is RegisterCommand registerCommand)
            {
                var password = registerCommand.Password;
                await Register(registerCommand.Username, password!);
            }

            if (command is LoginCommand loginCommand) await Login(loginCommand.Username, loginCommand.Password);

            if (command is ExitCommand _)
            {
                await Exit();
                Console.WriteLine("bye");
                break;
            }

            if (command is SendMessageCommand sendMessageCommand)
                await SendMessage(sendMessageCommand.Receiver, sendMessageCommand.MessageType,
                    sendMessageCommand.Message);

            if (command is CreateGroupCommand createGroupCommand) await CreateGroup(createGroupCommand.GroupId);

            if (command is JoinGroupCommand joinGroupCommand) await JoinGroup(joinGroupCommand.GroupId);

            if (command is LeaveGroupCommand leaveGroupCommand) await LeaveGroup(leaveGroupCommand.GroupId);
        }
    }

    private async Task SendMessage(string receiver, MessageType type, string content)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            PrintError("You haven't logged in");
            return;
        }

        if (type == MessageType.UserMessage)
            await _connection!.InvokeAsync("SendUserMessage", receiver, content);
        else
            await _connection!.InvokeAsync("SendGroupMessage", receiver, content);
    }


    private async Task CreateGroup(string groupId)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            PrintError("You haven't logged in");
            return;
        }

        await _connection!.InvokeAsync("CreateGroup", groupId);
    }

    private async Task JoinGroup(string groupId)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            PrintError("You haven't logged in");
            return;
        }

        await _connection!.InvokeAsync("JoinGroup", groupId);
    }

    private async Task LeaveGroup(string groupId)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            PrintError("You haven't logged in");
            return;
        }

        await _connection!.InvokeAsync("LeaveGroup", groupId);
    }

    public async Task Run()
    {
        Console.WriteLine("Chat client");
        await MainLoop();
    }
}