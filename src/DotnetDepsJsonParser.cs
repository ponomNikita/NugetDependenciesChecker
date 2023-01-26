using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace NugetDependenciesChecker;

internal class DotnetDepsJson
{
    private const string LibrariesTokenName = "libraries";
    private const string LibraryTypeTokenName = "type";
    private const string PackageType = "package";
    private const string NameVersionSeparator = "/";

    private JObject? _depsJson;

    public void Load(string depsJsonFilePath)
    {
        var jsonText = File.ReadAllText(depsJsonFilePath);
        _depsJson = JObject.Parse(jsonText);
    }


    public IEnumerable<Library> GetLibraries(string expectRegexp)
    {
        Check();

        if (_depsJson!.TryGetValue(LibrariesTokenName, out var libraries))
        {
            foreach (var jProperty in libraries.Children<JProperty>())
            {
                var libName = jProperty.Name;

                var type = jProperty.Value[LibraryTypeTokenName];

                if (type != null && type.ToString() == PackageType)
                {
                    if (!string.IsNullOrWhiteSpace(expectRegexp) && Regex.IsMatch(libName, expectRegexp))
                    {
                        var parts = libName.Split(NameVersionSeparator);
                        yield return new Library(parts[0], parts[1]);
                    }
                }
            }
        }
    }

    private void Check()
    {
        if (_depsJson == null)
        {
            throw new InvalidOperationException($"Load *.deps.json via {nameof(Load)} method before.");
        }
    }
}