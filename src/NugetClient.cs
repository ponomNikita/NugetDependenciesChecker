using System.Runtime.CompilerServices;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NugetDependenciesChecker;

internal class NugetClient
{
    private readonly List<PackageSource> _sources;

    public NugetClient(string nugetConfigPath)
    {
        var fileInfo = new FileInfo(nugetConfigPath);
        var packageSourceProvider = new PackageSourceProvider(new Settings(fileInfo.Directory!.FullName, fileInfo.Name));
        _sources = packageSourceProvider.LoadPackageSources().Where(it => it.IsEnabled).ToList();
    }

    public async IAsyncEnumerable<(NuGetVersion Version, string Source)> GetVersions(Library library,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = new HashSet<NuGetVersion>();
        
        foreach (var packageSource in _sources)
        {
            var repository = Repository.Factory.GetCoreV3(packageSource);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

            var versions = await resource.GetAllVersionsAsync(library.Name, NullSourceCacheContext.Instance, NullLogger.Instance,
                    cancellationToken)
                .ConfigureAwait(false);
            
            foreach (var nugetVersion in versions)
            {
                if (result.Add(nugetVersion))
                {
                    yield return (nugetVersion, packageSource.Name);
                }
            }
        }
    }
}