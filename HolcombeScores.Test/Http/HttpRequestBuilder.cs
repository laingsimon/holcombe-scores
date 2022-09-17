using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using HolcombeScores.Api.Services;

namespace HolcombeScores.Test.Http;

[SuppressMessage("ReSharper", "ParameterHidesMember")]
public class HttpRequestBuilder
{
    private readonly Guid? _contextId;
    private string? _baseUri;
    private string? _relativeUri;
    private HttpMethod _method = HttpMethod.Get;
    private string? _content;
    private string? _contentType;

    public HttpRequestBuilder(Guid? contextId)
    {
        _contextId = contextId;
    }

    public HttpRequestBuilder ForUri(string baseUri, string relativeUri)
    {
        return new HttpRequestBuilder(_contextId)
        {
            _baseUri = baseUri,
            _content = _content,
            _contentType = _contentType,
            _method = _method,
            _relativeUri = relativeUri,
        };
    }

    public HttpRequestBuilder WithData(HttpMethod method, string contentType, string content)
    {
        return new HttpRequestBuilder(_contextId)
        {
            _baseUri = _baseUri,
            _content = content,
            _contentType = contentType,
            _method = method,
            _relativeUri = _relativeUri,
        };
    }

    public HttpRequestBuilder WithMethod(HttpMethod method)
    {
        return new HttpRequestBuilder(_contextId)
        {
            _baseUri = _baseUri,
            _content = _content,
            _contentType = _contentType,
            _method = method,
            _relativeUri = _relativeUri,
        };
    }

    public async Task<HttpResponse> Send()
    {
        var cookies = new CookieContainer();
        var httpClient = new HttpClient(new HttpClientHandler
        {
            CookieContainer = cookies
        });
        var request = new HttpRequestMessage(_method, $"{_baseUri}{_relativeUri}");

        if (_contextId != null)
        {
            request.Headers.Add("Cookie", $"{TestingContext.ContextIdCookieName}={_contextId.Value}");
            request.Headers.Add("Cookie", $"{TestingContextFactory.TestingContextRequiredName}=true");
        }

        if (_content != null)
        {
            request.Content = new StringContent(_content, Encoding.UTF8, _contentType);
        }

        var response = await httpClient.SendAsync(request);

        return new HttpResponse(
            response.StatusCode,
            ToDictionary(response.Headers, response.Content.Headers),
            await response.Content.ReadAsStringAsync(),
            cookies.GetCookies(request.RequestUri!));
    }

    private static Dictionary<string, string> ToDictionary(
        HttpResponseHeaders responseHeaders,
        HttpContentHeaders? contentHeaders)
    {
        var contentHeadersDict = contentHeaders?.ToDictionary(pair => pair.Key, pair => string.Join(",", pair.Value));
        var responseHeadersDict = responseHeaders.ToDictionary(pair => pair.Key, pair => string.Join(",", pair.Value));

        return contentHeadersDict == null
            ? responseHeadersDict
            : contentHeadersDict.Concat(responseHeadersDict).ToDictionary(p => p.Key, p => p.Value);
    }
}