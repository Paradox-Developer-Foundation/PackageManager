using System.Net.Http.Json;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace ParadoxPM.Client.Commands;

[Command("package")]
public class PackageCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console) => default;
}

[Command("package search", Description = "Search for a package on the server.")]
public class PackageSearchCommand : ICommand
{
    // Order: 0
    [CommandParameter(0, Name = "keyword", Description = "Search keyword.")]
    public required string Keyword { get; init; }

    // Name: --arch
    // Short name: -a
    [CommandOption("arch", 'a', Description = "Target game such as 'ck3', 'eu4', etc.")]
    public required string Arch { get; init; }

    private static HttpClient sharedClient = new() { BaseAddress = new Uri("https://localhost:7295") };

    public async ValueTask ExecuteAsync(IConsole console)
    {
        using HttpResponseMessage response = await sharedClient.GetAsync(
            $"api/packages/query/search?keyword={Keyword}&arch={Arch}"
        );
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        await console.Output.WriteLineAsync($"{jsonResponse}\n");
    }
}
