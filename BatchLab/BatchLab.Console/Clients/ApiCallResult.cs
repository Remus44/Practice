
using System.Net;

namespace BatchLab.Console.Clients;

public sealed record ApiCallResult(
    HttpStatusCode StatusCode,
    string? ErrorBody)
{
    public bool IsSuccess =>
        (int)StatusCode is >= 200 and <= 299;
}
