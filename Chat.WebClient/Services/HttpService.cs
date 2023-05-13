using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<IEnumerable<UserDTO>> GetAllUserContacts()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/users"));
        if (response.IsSuccessStatusCode)
        {
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<UserDTO>>();
            if (contacts == null) throw new Exception("Failed to fetch contacts");
            return contacts;
        }

        throw new Exception(response.ReasonPhrase);
    }

    public async Task<IEnumerable<GroupDTO>> GetAllGroupContacts(string username)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/users/{username}/groups"));
        if (response.IsSuccessStatusCode)
        {
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<GroupDTO>>();
            if (contacts == null) throw new Exception("Failed to fetch contacts");
            return contacts;
        }

        throw new Exception(response.ReasonPhrase);
    }

    public async Task<List<UserMessageDTO>> GetUserChat(string username1, string username2)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/chats/user/{username1}/{username2}"));

        if (response.IsSuccessStatusCode)
        {
            var messages = (await response.Content.ReadFromJsonAsync<IEnumerable<UserMessageDTO>>())?.ToList();
            if (messages == null)
            {
                throw new Exception("Failed to fetch user chats");
            }
            return messages;
        }
        throw new Exception(response.ReasonPhrase);
    }

    public async Task<List<GroupMessageDTO>> GetGroupChat(int groupId)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + $"/api/chats/group/{groupId}"));

        if (response.IsSuccessStatusCode)
        {
            var messages = (await response.Content.ReadFromJsonAsync<IEnumerable<GroupMessageDTO>>())?.ToList();
            if (messages == null)
            {
                throw new Exception("Failed to fetch messages");
            }
            return messages;
        }
        throw new Exception(response.ReasonPhrase);
    }

    public async Task SendUserMessage(UserMessageDTO message)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/chats/user/message"), message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ReasonPhrase);
        }
    }

    public async Task SendGroupMessage(GroupMessageDTO message)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/chats/group/message"), message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ReasonPhrase);
        }
    }

    public async Task<UserDTO> GetCurrentUser()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/users/me"));
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null) throw new Exception("Failed to fetch the current user");
            return user;
        }

        throw new Exception(response.ReasonPhrase);
    }




}