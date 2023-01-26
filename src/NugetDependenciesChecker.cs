using NuGet.Versioning;

namespace NugetDependenciesChecker;

internal class NugetDependenciesChecker
{
    private readonly DotnetDepsJson _parser;
    private readonly NugetClient _nugetClient;

    public NugetDependenciesChecker(DotnetDepsJson parser, NugetClient nugetClient)
    {
        _parser = parser;
        _nugetClient = nugetClient;
    }

    public async Task Check(string? excludeRegexPattern, CancellationToken cancellationToken)
    {
        var libraries = excludeRegexPattern != null
            ? _parser.GetLibraries(excludeRegexPattern)
            : _parser.GetNotSystemLibraries();

        foreach (var library in libraries)
        {
            var versions = _nugetClient.GetVersions(library, cancellationToken).ConfigureAwait(false);
            if (NuGetVersion.TryParse(library.Version, out var libVersion))
            {
                var approximateBestMatch = new Dictionary<NuGetVersion, string>();
                var wasFound = false;

                await foreach (var (version, source) in versions)
                {
                    if (version == libVersion)
                    {
                        Info($"{library}: was found in {source}");
                        wasFound = true;
                        break;
                    }

                    if (version > libVersion)
                    {
                        approximateBestMatch.TryAdd(version, source);
                    }
                }

                if (approximateBestMatch.Any())
                {
                    var minVersion = approximateBestMatch.MinBy(it => it.Key);
                    Warning(
                        $"{library}: was not found in {minVersion.Value}. An approximate best match of {library.Name} is {minVersion.Key}");
                }
                else if (!wasFound)
                {
                    Error($"{library}: was not found");
                }
            }
            else
            {
                Info($"{library}: {string.Join(", ", versions)}");
            }
        }
    }

    private void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}