using System.CommandLine;
using System.Text;
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
    var client = new ChatClient(username);
    await client.Run();
}, usernameOption);

await rootCommand.InvokeAsync(args);