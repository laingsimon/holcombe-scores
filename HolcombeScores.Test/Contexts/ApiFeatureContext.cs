using HolcombeScores.Test.Configuration;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Contexts;

// ReSharper disable once ClassNeverInstantiated.Global
public class ApiFeatureContext
{
    private readonly FeatureContext _featureContext;

    public ApiFeatureContext(FeatureContext featureContext)
    {
        _featureContext = featureContext;
    }

    public Guid TestContextId
    {
        get => (Guid)_featureContext[nameof(TestContextId)];
        set => _featureContext[nameof(TestContextId)] = value;
    }

    public string Name => _featureContext.FeatureInfo.Title;

    public ITestingConfiguration TestingConfiguration
    {
        get => (ITestingConfiguration)_featureContext[nameof(TestingConfiguration)];
        set => _featureContext[nameof(TestingConfiguration)] = value;
    }
}