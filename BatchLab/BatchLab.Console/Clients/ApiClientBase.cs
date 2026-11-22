
namespace BatchLab.Console.Clients;

public abstract class ApiClientBase
{
    protected HttpClient Http { get; }

    protected ApiClientBase(HttpClient httpClient)
    {
        Http = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected abstract string RoutePrefix { get; }

    protected string GetResourceUrl(long id)
    {
        return $"{RoutePrefix}/{id}";
    }
}
