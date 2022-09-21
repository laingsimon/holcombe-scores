using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Http;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Contexts;

// ReSharper disable once ClassNeverInstantiated.Global
public class ApiScenarioContext
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ApiFeatureContext _featureContext;

    public ApiScenarioContext(ScenarioContext scenarioContext, ApiFeatureContext featureContext)
    {
        _scenarioContext = scenarioContext;
        _featureContext = featureContext;
        RequestBuilder = new HttpRequestBuilder(featureContext.TestContextId);
        Stash = new Dictionary<string, string>();
        ScenarioUniqueId = Guid.NewGuid().ToString();
    }

    public string ScenarioUniqueId
    {
        get => (string)_scenarioContext[nameof(ScenarioUniqueId)];
        set => _scenarioContext[nameof(ScenarioUniqueId)] = value;
    }

    public HttpRequestBuilder RequestBuilder
    {
        get => (HttpRequestBuilder)_scenarioContext[nameof(RequestBuilder)];
        set => _scenarioContext[nameof(RequestBuilder)] = value;
    }

    public ITestingConfiguration TestingConfiguration => _featureContext.TestingConfiguration;

    public Guid? TestContextId => _featureContext.TestContextId;

    public Dictionary<string, string> Stash
    {
        get
        {
            var stashedValue = _scenarioContext.ContainsKey(nameof(Stash))
                ? (Dictionary<string, string>)_scenarioContext[nameof(Stash)]
                : throw new InvalidOperationException("No value has been stashed");
            return stashedValue;
        }
        private set => _scenarioContext[nameof(Stash)] = value;
    }
}