using System.Net.Http.Json;
using Chat.Common.DTOs;

namespace Chat.WebClient.Utils;

public class Fetcher
{
    public const string ServerUrl = "http://localhost:5001";

    /// <summary>
    /// Fetches all contacts of the current user
    /// </summary>
    public static async Task<IEnumerable<UserDTO>> GetAllContacts()
    {
        using var httpClient = new HttpClient();
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
    public static async Task<List<MessageDTO>> GetMessages(string contact)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(new Uri(ServerUrl + "/api/message/between?user=" + contact));

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
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsJsonAsync(new Uri(ServerUrl + "/api/message/send"), message);
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
        using var httpClient = new HttpClient();
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