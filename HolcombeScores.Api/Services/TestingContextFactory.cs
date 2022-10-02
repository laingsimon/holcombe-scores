using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services;

public class TestingContextFactory : ITestingContextFactory
{
    public const string TestingContextRequiredName = "Testing-Context-Required";

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
            TestingContextRequired = GetTestingContextRequired(),
        };
    }

    private bool GetTestingContextRequired()
    {
        var requestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[TestingContextRequiredName];
        if (!string.IsNullOrEmpty(requestCookie) && bool.TryParse(requestCookie, out var requiredViaCookie))
        {
            return requiredViaCookie;
        }

        return false;
    }

    private string GetContextCookie()
    {
        var requestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[TestingContext.ContextIdCookieName];
        return !string.IsNullOrEmpty(requestCookie)
            ? requestCookie
            : null;
    }

    public static ITestingContext Create(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<ITestingContextFactory>()!.Create();
    }
}