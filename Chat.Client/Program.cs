using System.CommandLine;
using Chat.Client;

var usernameOption = new Option<String>(
    name: "-u",
    description: "Username",
    getDefaultValue: () => "defaultUser"
);

var rootCommand = new RootCommand("Simple Chat app");
rootCommand.AddOption(usernameOption);

rootCommand.SetHandler(async (username) =>
{
    var baseUrl = "http://localhost:5101";
    var client = new ChatClient(baseUrl);
    await client.Run();
}, usernameOption);

await rootCommand.InvokeAsync(args);