using WebDLanding.Models;

namespace WebDLanding;

public class AppSettingsAppModelFinder : IAppModelFinder
{
    private readonly List<AppModel> _entries;

    /// <summary>
    /// Constructor for testing. Starts with a list provided.
    /// </summary>
    /// <param name="entries">The view model list to operate with.</param>
    public AppSettingsAppModelFinder(List<AppModel> entries)
    {
        _entries = entries;
    }

    /// <summary>
    /// DI Constructor, used to pass in an appsettings.json file for loading the view models from.
    /// </summary>
    /// <param name="configuration">The configuration instance to load data from.</param>
    public AppSettingsAppModelFinder(IConfiguration configuration)
    {
        _entries = configuration.GetSection("AppModels").Get<List<AppModel>>();
    }

    public Task<AppModel> FindByReferrerUriAsync(Uri referrerUri)
    {
        var localReferrerPath = referrerUri.LocalPath.ToString();
        var startSub = localReferrerPath.LastIndexOf("/") + 1;
        var subLength = localReferrerPath.Length - startSub;
        var database = localReferrerPath.Substring(startSub, subLength);

        // find our database that has the same web direct host
        var vm = _entries.Single(h =>
            h.DatabaseName.Equals(database, StringComparison.OrdinalIgnoreCase)
            && h.WebDirectHostName.Equals(referrerUri.Host, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(vm);
    }

    public Task<AppModel> FindByDatabaseAsync(string database)
    {
        return Task.FromResult(
            _entries.Single(e => e.DatabaseName.Equals(database, StringComparison.OrdinalIgnoreCase))
        );
    }

    public Task<AppModel> FindByEntryHostAsync(string entryHost)
    {
        return Task.FromResult(
            _entries.Single(e => e.EntryHostName.Equals(entryHost, StringComparison.OrdinalIgnoreCase))
        );
    }
}