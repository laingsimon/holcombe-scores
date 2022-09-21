using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Contexts;
using HolcombeScores.Test.Http;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

public abstract class StepBase
{
    private readonly ApiScenarioContext _scenarioContext;

    protected StepBase(ApiScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    protected Guid? TestContextId => _scenarioContext.TestContextId;

    protected ITestingConfiguration TestingConfiguration => _scenarioContext.TestingConfiguration;

    protected string Stash
    {
        get => _scenarioContext.Stash;
        set => _scenarioContext.Stash = value;
    }

    protected HttpRequestBuilder RequestBuilder
    {
        get => _scenarioContext.RequestBuilder;
        set => _scenarioContext.RequestBuilder = value;
    }

    protected HttpResponse? Response
    {
        get => _scenarioContext.Response;
        set => _scenarioContext.Response = value;
    }

    protected Table SupplantValues(Table properties, string? adminPassCode = null)
    {
        foreach (var row in properties.Rows)
        {
            foreach (var pair in row)
            {
                row[pair.Key] = SupplantValues(pair.Value, adminPassCode);
            }
        }

        return properties;
    }

    protected string SupplantValues(string value, string? adminPassCode = null)
    {
        if (value.Contains("${AdminPassCode}") && string.IsNullOrEmpty(adminPassCode))
        {
            throw new InvalidOperationException("AdminPassCode requested to be transformed but value not supplied");
        }

        return value
            .Replace("${AdminPassCode}", adminPassCode)
            .Replace("${Stash}", value.Contains("${Stash}") ? Stash : "")
            .Replace("${TestContextId}", TestContextId.ToString());
    }
}