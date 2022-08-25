using WebDLanding.Models;

namespace WebDLanding;

public interface IAppModelFinder
{
    public Task<AppModel> FindByEntryHostAsync(string entryHost);

    public Task<AppModel> FindByDatabaseAsync(string database);

    public Task<AppModel> FindByReferrerUriAsync(Uri referrerUri);
}