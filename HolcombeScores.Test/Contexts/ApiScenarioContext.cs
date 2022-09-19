using HolcombeScores.Test.Http;
using HolcombeScores.Test.Steps;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Contexts;

public class ApiScenarioContext
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ApiFeatureContext _featureContext;

    public ApiScenarioContext(ScenarioContext scenarioContext, ApiFeatureContext featureContext)
    {
        _scenarioContext = scenarioContext;
        _featureContext = featureContext;
        RequestBuilder = new HttpRequestBuilder(featureContext.TestContextId);
    }

    public HttpRequestBuilder RequestBuilder
    {
        get => (HttpRequestBuilder)_scenarioContext[nameof(RequestBuilder)];
        set => _scenarioContext[nameof(RequestBuilder)] = value;
    }

    public HttpResponse? Response
    {
        get => _scenarioContext.ContainsKey(nameof(Response))
            ? (HttpResponse)_scenarioContext[nameof(Response)]
            : null;
        set => _scenarioContext[nameof(Response)] = value;
    }

    public Guid? TestContextId => _featureContext.TestContextId;

    public string Stash
    {
        get
        {
            var stashedValue = _scenarioContext.ContainsKey(nameof(Stash))
                ? (string)_scenarioContext[nameof(Stash)]
                : throw new InvalidOperationException("No value has been stashed");
            return stashedValue;
        }
        set => _scenarioContext[nameof(Stash)] = value;
    }
}