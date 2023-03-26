using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Chat.Client.Command;
using Chat.Common;
using Microsoft.AspNetCore.SignalR.Client;

namespace Chat.Client;

public class ChatClient
{
    private readonly string _baseUrl;
    private string? _username;
    private string? _token;
    private HubConnection? _connection;
 
    
    public ChatClient(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    private static async Task<string?> Login(string username, string password)
    {
        using var httpClient = new HttpClient();
        var request = new AuthenticationRequest { Username = username, Password = password };
        var response = await httpClient.PostAsJsonAsync("http://localhost:5101/login",
            request);
        
        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            return authResponse.Token;
        }
        
        await Console.Error.WriteLineAsync("Username or password is incorrect");
        return null;
    }

    private void RegisterCallbacks()
    {
        _connection!.On<Message>(ChatEvents.ReceiveMessage, message =>
        {
            var stringMessage = message.Type == ReceiverType.User ? 
                $"{message.Time} u/{message.From}:{message.Content}" : 
                $"{message.Time} g/{message.To} u/{message.From}:{message.Content}";
            
            Console.Write(new Rune(0x1f4ac));
            Console.WriteLine(" " + stringMessage);
        });

        _connection?.On<RpcResponse>(ChatEvents.RpcResponse, response =>
        {
            if (response.Status == RpcResponseStatus.Error)
            {
                Console.Error.WriteLine(response.Message);
            }
            else
            {
                Console.WriteLine(response.Message);
            }
        });
    }
    
    private bool InitConnection(string? username, string? password)
    {
        if (username == null || password == null)
        {
            return false;
        }
        var token = Login(username, password);
        _connection = new HubConnectionBuilder().WithUrl(_baseUrl + "/chat", options => 
            options.AccessTokenProvider = () => 
                Task.FromResult(token.Result)
            ).Build();
        
        if (token.Result == null)
        {
            return false;
        }
        _token = token.Result;
        _username = username;
        return true;
    }

    private async Task Connect()
    {
        if (_connection != null)
        {
            await _connection.StartAsync();
        }
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
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", _token);
        return httpClient;
    }

    private async Task MainLoop()
    {
        while (true)
        {
            var input = Console.ReadLine();
            var command = CommandParser.parse(input);
            
            if (command == null)
            {
                Console.WriteLine("Invalid input");
                continue;
            }

            if (command is LoginCommand loginCommand)
            {
                if (IsAuthenticated())
                {
                    Console.WriteLine("You have already logged in");
                    continue;
                }
                
                Console.Write("Password?");
                var password = Console.ReadLine();
                var loginSuccess = InitConnection(loginCommand.Username, password);
                
                if (!loginSuccess)
                {
                    Console.WriteLine("Username or password is incorrect");
                    continue;
                }
                
                // Connect to hub server
                try
                {
                    RegisterCallbacks();
                    await Connect();
                    Console.WriteLine("Login succeeds");
                }
                catch (Exception e)
                {
                    await Console.Error.WriteLineAsync(e.Message);
                }
            }
            
            if (command is ExitCommand _)
            {
                if (IsConnected())
                {
                    await _connection!.StopAsync();
                }
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

            if (command is LeaveGroupCommand leaveGroupCommand)
            {
                await LeaveGroup(leaveGroupCommand.GroupId);
            }
        }
    }

    private async Task SendMessage(string receiver, ReceiverType type, string content)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            await Console.Error.WriteLineAsync("You haven't logged in");
            return;
        }
        
        if (type == ReceiverType.User)
        {
            await _connection!.InvokeAsync("SendUserMessage", receiver, content);
        }
        else
        {
            await _connection!.InvokeAsync("SendGroupMessage", receiver, content);
        }
    }
    

    private async Task CreateGroup(string groupId)
    {
        // using var httpClient = GetHttpClient();
        // var response = await httpClient.PostAsJsonAsync(_baseUrl + "/create_group", groupId);
        //
        // if (!response.IsSuccessStatusCode)
        // {
        //     await Console.Error.WriteLineAsync(response.Content.ReadAsStringAsync().Result);
        // }
        
        if (!IsConnected() || !IsAuthenticated())
        {
            await Console.Error.WriteLineAsync("You haven't logged in");
            return;
        }
        await _connection!.InvokeAsync("CreateGroup", groupId);
    }

    private async Task JoinGroup(string groupId)
    {
        // using var httpClient = GetHttpClient();
        // var response = await httpClient.PostAsJsonAsync(_baseUrl + "/join_group", groupId);
        //
        // if (!response.IsSuccessStatusCode)
        // {
        //     await Console.Error.WriteLineAsync(response.Content.ReadAsStringAsync().Result);
        // }
        
        if (!IsConnected() || !IsAuthenticated())
        {
            await Console.Error.WriteLineAsync("You haven't logged in");
            return;
        }
        
        await _connection!.InvokeAsync("JoinGroup", groupId);
    }
    
    private async Task LeaveGroup(string groupId)
    {
        if (!IsConnected() || !IsAuthenticated())
        {
            await Console.Error.WriteLineAsync("You haven't logged in");
            return;
        }
        await _connection!.InvokeAsync("LeaveGroup", groupId);
    }
    
    public async Task Run()
    {
        Console.WriteLine("Chat client");
        // InitConnection("as2asfM", "asf-0PA>f");
        await MainLoop();
    }
}