namespace NugetDependenciesChecker;

internal static class DotnetDepsJsonExtensions
{
    public const string SystemLibrariesRegPattern =
        "^(?!.*runtime)(?!.*System)(?!.*NETStandard)(?!.*NETCore)(?!.*Win32)(?!.*Bcl).*$";

    public static IEnumerable<Library> GetNotSystemLibraries(this DotnetDepsJson dotnetDepsJson)
    {
        return dotnetDepsJson.GetLibraries(SystemLibrariesRegPattern);
    }

    public static IEnumerable<Library> GetLibraries(this DotnetDepsJson dotnetDepsJson) =>
        dotnetDepsJson.GetLibraries(string.Empty);
}