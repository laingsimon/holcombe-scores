using HolcombeScores.Test.Configuration;

namespace HolcombeScores.Test.Contexts;

public class ApiSuiteContext
{
    public string TestContextId { get; }
    public HashSet<string> TestContextUsages { get; } = new HashSet<string>();
    public ITestingConfiguration TestingConfiguration { get; }
    public IDisposable ApiInstance { get; }

    public ApiSuiteContext(string testContextId, ITestingConfiguration testingConfiguration, IDisposable apiInstance)
    {
        TestContextId = testContextId;
        TestingConfiguration = testingConfiguration;
        ApiInstance = apiInstance;
    }
}