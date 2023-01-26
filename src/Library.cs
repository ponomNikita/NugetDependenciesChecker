namespace NugetDependenciesChecker;

internal record Library(string Name, string Version)
{
    public override string ToString()
    {
        return $"{Name}@{Version}";
    }
}