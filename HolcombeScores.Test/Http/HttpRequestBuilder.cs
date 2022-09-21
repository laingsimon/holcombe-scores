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
    private readonly Dictionary<string, string> _cookies;
    private string? _baseUri;
    private string? _contentType;
    private HttpResponse? _response;

    public HttpRequestBuilder(Guid? contextId, HttpRequestBuilder? requestBuilder = null)
        :this(contextId, GetCookies(requestBuilder))
    { }

    private static Dictionary<string, string> GetCookies(HttpRequestBuilder? previousRequest)
    {
        if (previousRequest == null)
        {
            return new Dictionary<string, string>();
        }

        var previousResponseCookies = previousRequest._response?.Cookies;
        var previousCookies = previousRequest._cookies;

        var nextRequestCookies = new Dictionary<string, string>();
        foreach (var previousCookie in previousCookies)
        {
            nextRequestCookies[previousCookie.Key] = previousCookie.Value;
        }

        if (previousResponseCookies != null)
        {
            foreach (Cookie previousResponseCookie in previousResponseCookies)
            {
                nextRequestCookies[previousResponseCookie.Name] = previousResponseCookie.Value;
            }
        }

        return nextRequestCookies;
    }

    private HttpRequestBuilder(Guid? contextId, Dictionary<string, string> cookies)
    {
        _contextId = contextId;
        _cookies = cookies;
    }

    public string? RelativeUri { get; private set; }
    public HttpMethod Method { get; private set; } = HttpMethod.Get;
    public string? Content { get; private set; }

    public HttpRequestBuilder ForUri(string baseUri, string relativeUri)
    {
        return new HttpRequestBuilder(_contextId, _cookies)
        {
            _baseUri = baseUri,
            Content = Content,
            _contentType = _contentType,
            Method = Method,
            RelativeUri = relativeUri,
        };
    }

    public HttpRequestBuilder WithData(HttpMethod method, string contentType, string content)
    {
        return new HttpRequestBuilder(_contextId, _cookies)
        {
            _baseUri = _baseUri,
            Content = content,
            _contentType = contentType,
            Method = method,
            RelativeUri = RelativeUri,
        };
    }

    public HttpRequestBuilder WithMethod(HttpMethod method)
    {
        return new HttpRequestBuilder(_contextId, _cookies)
        {
            _baseUri = _baseUri,
            Content = Content,
            _contentType = _contentType,
            Method = method,
            RelativeUri = RelativeUri,
        };
    }

    public async Task<HttpResponse> Send()
    {
        var cookies = new CookieContainer();
        var httpClient = new HttpClient(new HttpClientHandler
        {
            CookieContainer = cookies
        });
        var request = new HttpRequestMessage(Method, $"{_baseUri}{RelativeUri}");

        if (_contextId != null)
        {
            request.Headers.Add("Cookie", $"{TestingContext.ContextIdCookieName}={_contextId.Value}");
            request.Headers.Add("Cookie", $"{TestingContextFactory.TestingContextRequiredName}=true");

            foreach (var cookie in _cookies)
            {
                if (cookie.Key is TestingContext.ContextIdCookieName or TestingContextFactory.TestingContextRequiredName)
                {
                    continue;
                }

                request.Headers.Add("Cookie", $"{cookie.Key}={cookie.Value}");
            }
        }

        if (Content != null)
        {
            request.Content = new StringContent(Content, Encoding.UTF8, _contentType);
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

    public async Task<HttpResponse> GetResponse()
    {
        return _response ??= await Send();
    }
}