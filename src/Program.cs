using McMaster.Extensions.CommandLineUtils;

namespace NugetDependenciesChecker;

public static class Program
{
    private const string HelpValues = "-?|-h|--help";

    static async Task<int> Main(string[] args)
    {
        var app = new CommandLineApplication();

        app.HelpOption(HelpValues);


        app.Command("check", command =>
        {
            command.HelpOption(HelpValues);
            command.Description =
                "Checks all libraries fom *.deps.json file by sources in Nuget.Config. Prints results to console.";

            var depsJson = command.Argument("deps-json", "Absolute path to *.deps.json file");
            var nugetConfig = command.Argument("nuget-config", "Absolute path to Nuget.Config");
            var excludeRegexPattern = command.Option("-e|--exclude-regexp", 
                "Regular expression pattern for excluding packages from check. " +
                $"Will used default exclude pattern if not specified. Default pattern: {DotnetDepsJsonExtensions.SystemLibrariesRegPattern}", 
                CommandOptionType.SingleValue);

            command.OnExecuteAsync(async cancellationToken =>
            {
                if (!depsJson.HasValue || depsJson.Value == null)
                {
                    Console.WriteLine("Argument missed: deps-json is required");
                    return 1;
                }

                if (!nugetConfig.HasValue || nugetConfig.Value == null)
                {
                    Console.WriteLine("Argument missed: nuget-config is required");
                    return 1;
                }

                try
                {
                    var parser = new DotnetDepsJson();
                    parser.Load(depsJson.Value!);

                    var nugetClient = new NugetClient(nugetConfig.Value!);
                    var checker = new NugetDependenciesChecker(parser, nugetClient);

                    await checker.Check(excludeRegexPattern.HasValue() ? excludeRegexPattern.Value() : null, cancellationToken);

                    Console.WriteLine("Command completed.");
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return 1;
                }
            });
        });


        return await app.ExecuteAsync(args);
    }
}