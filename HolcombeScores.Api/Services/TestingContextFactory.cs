namespace HolcombeScores.Api.Services;

public class TestingContextFactory : ITestingContextFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TestingContextFactory(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ITestingContext Create()
    {
        return new TestingContext
        {
            ContextId = GetContextCookie(),
        };
    }

    private Guid? GetContextCookie()
    {
        var requestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[TestingContext.ContextIdCookieName];
        if (!string.IsNullOrEmpty(requestCookie) && Guid.TryParse(requestCookie, out var contextId))
        {
            return contextId;
        }

        return null;
    }

    public static ITestingContext Create(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<ITestingContextFactory>()!.Create();
    }
}