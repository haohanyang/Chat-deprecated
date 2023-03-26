using Chat.Client;

var baseUrl = "http://localhost:5101";
var client = new ChatClient(baseUrl);
await client.Run();