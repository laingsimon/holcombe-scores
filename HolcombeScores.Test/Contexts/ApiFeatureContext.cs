using System.Diagnostics;
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

    public string? TestContextId
    {
        [DebuggerStepThrough]
        get => (string)_featureContext[nameof(TestContextId)];
        [DebuggerStepThrough]
        set => _featureContext[nameof(TestContextId)] = value;
    }

    public string Name => _featureContext.FeatureInfo.Title;

    public ITestingConfiguration TestingConfiguration
    {
        [DebuggerStepThrough]
        get => (ITestingConfiguration)_featureContext[nameof(TestingConfiguration)];
        [DebuggerStepThrough]
        set => _featureContext[nameof(TestingConfiguration)] = value;
    }
}