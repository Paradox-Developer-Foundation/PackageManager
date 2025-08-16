using CliFx;

namespace ParadoxPM.Client;

public static class Program
{
    
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
}