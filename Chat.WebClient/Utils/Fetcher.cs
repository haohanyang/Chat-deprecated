using System.Net.Http.Headers;
using System.Net.Http.Json;
using Chat.Common.DTOs;
using Microsoft.Extensions.Configuration;


namespace Chat.WebClient.Utils;

public class Fetcher
{
    public const string ServerUrl = "http://localhost:5001";

    public static string? GetAccessToken()
    {
        return null;
    }

    public static HttpClient GetHttpClient()
    {
        var token = GetAccessToken();
        var httpClient = new HttpClient();
        if (token != null)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return httpClient;
    }

    public static async Task<IEnumerable<UserDTO>> GetAllContacts()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/user/all"));
        if (response.IsSuccessStatusCode)
        {
            var contacts = await response.Content.ReadFromJsonAsync<IEnumerable<UserDTO>>();
            if (contacts == null) throw new Exception("Failed to fetch contacts");
            return contacts;
        }

        throw new Exception(response.ReasonPhrase);
    }

    /// <summary>
    /// Fetches all messages between the current user and the given contact
    /// </summary>
    public static async Task<List<MessageDTO>> GetChat(string contact)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/chat/user_chat?user=" + contact));

        if (response.IsSuccessStatusCode)
        {
            var messages = (await response.Content.ReadFromJsonAsync<IEnumerable<MessageDTO>>())?.ToList();
            if (messages == null)
            {
                throw new Exception("Failed to fetch messages");
            }
            return messages;
        }
        throw new Exception(response.ReasonPhrase);
    }

    /// <summary>
    /// Send message
    /// </summary>
    public static async Task SendMessage(string contact, MessageDTO message)
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/chat/send_chat"), message);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.ReasonPhrase);
        }
    }

    /// <summary>
    /// Fetch current user info
    /// </summary>
    public static async Task<UserDTO> GetCurrentUser()
    {
        using var httpClient = GetHttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/user/me"));
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null) throw new Exception("Failed to fetch current user");
            return user;
        }

        throw new Exception(response.ReasonPhrase);
    }
}