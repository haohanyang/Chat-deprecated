using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chat.Common.Dtos;
using Chat.Common.DTOs;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Chat.WebClient.Services;

public class HttpService : IHttpService
{
    private const string ServerUrl = "http://localhost:5001";

    public string? GetAccessToken()
    {
        return null;
    }

    public string GetServerUrl()
    {
        return ServerUrl;
    }

    private HttpClient GetHttpClient()
    {
        var token = GetAccessToken();
        var httpClient = new HttpClient();
        if (token != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return httpClient;
    }

    public async Task<IEnumerable<UserDto>> GetAllUserContacts()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/users"));
        if (response.IsSuccessStatusCode)
        {
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
            if (contacts == null) throw new Exception("Failed to fetch contacts");
            return contacts;
        }

        throw new Exception(response.ReasonPhrase);
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroupContacts(string username)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/users/{username}/groups"));
        if (response.IsSuccessStatusCode)
        {
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<GroupDto>>();
            if (contacts == null) throw new Exception("Failed to fetch contacts");
            return contacts;
        }

        throw new Exception(response.ReasonPhrase);
    }

    public async Task<List<UserMessageDto>> GetUserChat(string username1, string username2)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/chats/users/{username1}/{username2}"));

        if (response.IsSuccessStatusCode)
        {
            var messages = (await response.Content.ReadFromJsonAsync<IEnumerable<UserMessageDto>>())?.ToList();
            if (messages == null)
            {
                throw new Exception("Failed to fetch user chats");
            }
            return messages;
        }
        throw new Exception(response.ReasonPhrase);
    }

    public async Task<List<GroupMessageDto>> GetGroupChat(int groupId)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/chats/groups/{groupId}"));

        if (response.IsSuccessStatusCode)
        {
            var messages = (await response.Content.ReadFromJsonAsync<IEnumerable<GroupMessageDto>>())?.ToList();
            if (messages == null)
            {
                throw new Exception("Failed to fetch messages");
            }
            return messages;
        }
        throw new Exception(response.ReasonPhrase);
    }

    public async Task SendUserMessage(UserMessageDto message)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/chats/users"), message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ReasonPhrase);
        }
    }

    public async Task SendGroupMessage(GroupMessageDto message)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/chats/groups"), message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ReasonPhrase);
        }
    }

    public async Task<UserDto> GetCurrentUser()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/auth/"));

        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            if (user == null) throw new Exception("Failed to fetch the current user");
            return user;
        }

        throw new Exception(response.ReasonPhrase);
    }

    public async Task<GroupDto> CreateGroup(string username, string groupName)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/groups"), new GroupDto
        {
            Creator = new UserDto { Username = username },
            Name = groupName
        });
        if (response.IsSuccessStatusCode)
        {
            var newGroup = await response.Content.ReadFromJsonAsync<GroupDto>();
            if (newGroup == null)
            {
                Console.Error.WriteLine("Failed to deserialize the response");
                throw new Exception("Unknown error");
            }
            return newGroup;
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception(error);
    }

    public async Task<MembershipDto> JoinGroup(string username, int id)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + $"/api/groups/{id}/memberships"), new MembershipDto
        {
            Member = new UserDto { Username = username },
            Group = new GroupDto { Id = id }
        });
        if (response.IsSuccessStatusCode)
        {
            var membership = await response.Content.ReadFromJsonAsync<MembershipDto>();
            if (membership == null)
            {
                Console.Error.WriteLine("Failed to deserialize the response");
                throw new Exception("Unknown error");
            }
            return membership;
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new Exception(error);
    }

    public async Task<GroupDto?> GetGroup(int id)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/groups/{id}"));
        if (response.IsSuccessStatusCode)
        {
            var group = await response.Content.ReadFromJsonAsync<GroupDto>();
            if (group == null)
            {
                Console.Error.WriteLine("Failed to deserialize the response");
                throw new Exception("Unknown error");
            }
            return group;
        }
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        throw new Exception(response.ReasonPhrase);
    }
}