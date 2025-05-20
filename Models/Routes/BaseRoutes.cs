namespace HashtApp.Soft.Client.Utilities;

public abstract class BaseRoutes(string Prefix, string routeBase) : BaseNoneRoute(Prefix, routeBase)
{
    public string Get => $"{Default}/{{Id}}";
    public string Delete => $"{Default}/{{Id}}";
    public string Update => $"{Default}/{{Id}}";
    public string GetPaginated => $"{Default}/paginated";
}
public abstract class BaseGetRoute(string Prefix, string routeBase) : BaseNoneRoute(Prefix, routeBase)
{
    public string Get => $"{Default}/{{Id}}";
    public string GetPaginated => $"{Default}/paginated";
}
public abstract class BaseNoneRoute(string Prefix, string routeBase)
{
    public string Default => $"{Prefix}/{routeBase}";
}