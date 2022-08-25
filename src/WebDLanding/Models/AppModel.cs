namespace WebDLanding.Models;

public class AppModel
{
    public string DatabaseName { get; set; } = default!;
    public string EntryHostScheme { get; set; } = "https";
    public string EntryHostName { get; set; } = default!;

    public Uri EntryHostUri => new($"{EntryHostScheme}://{EntryHostName}");

    public string WebDirectHostScheme { get; set; } = "https";
    public string WebDirectHostName { get; set; } = default!;

    private readonly string _queryString = "?homeurl=https://webdlanding.wizardsoftware.net/home/logoff";

    public Uri WebDirectHostUri => new($"{WebDirectHostScheme}://{WebDirectHostName}");

    public Uri WebDirectFullUri => new(
        uriString: $"{WebDirectHostScheme}://{WebDirectHostName}/fmi/webd/{DatabaseName}{_queryString}"
    );

}