using Chat.Client;

var baseUrl = "http://localhost:5000";
var client = new ChatClient(baseUrl);
await client.Run();