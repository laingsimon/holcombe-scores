using System.Net;

namespace HolcombeScores.Test.Http;

public class HttpResponse
{
    public HttpStatusCode StatusCode { get; }
    public Dictionary<string, string> Headers { get; }
    public string? Body { get; }
    public CookieCollection Cookies { get; }

    public HttpResponse(HttpStatusCode statusCode, Dictionary<string, string> headers, string? body,
        CookieCollection cookies)
    {
        StatusCode = statusCode;
        Headers = headers;
        Body = body;
        Cookies = cookies;
    }
}