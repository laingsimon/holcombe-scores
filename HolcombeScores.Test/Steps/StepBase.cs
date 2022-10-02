using System.Text.RegularExpressions;
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

    protected string? TestContextId => _scenarioContext.TestContextId;

    protected ITestingConfiguration TestingConfiguration => _scenarioContext.TestingConfiguration;

    protected Dictionary<string, string> Stash => _scenarioContext.Stash;

    protected HttpRequestBuilder RequestBuilder
    {
        get => _scenarioContext.RequestBuilder;
        set => _scenarioContext.RequestBuilder = value;
    }

    protected async Task<HttpResponse> GetResponse()
    {
        return await _scenarioContext.RequestBuilder.GetResponse();
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
        var utcNow = DateTime.UtcNow;
        const string utcDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        var replacementValues = new Dictionary<string, string>(Stash, StringComparer.OrdinalIgnoreCase)
        {
            { "ScenarioUniqueId", _scenarioContext.ScenarioUniqueId },
            { "UniqueId", Guid.NewGuid().ToString() },
            { "TestContextId", TestContextId ?? "" },
            { "UtcNow-1hr", utcNow.AddHours(-1).ToString(utcDateTimeFormat) },
            { "UtcNow-2hr", utcNow.AddHours(-2).ToString(utcDateTimeFormat) },
            { "UtcNow", utcNow.ToString(utcDateTimeFormat) },
            { "UtcNow+1hr", utcNow.AddHours(1).ToString(utcDateTimeFormat) },
            { "UtcNow+2hr", utcNow.AddHours(2).ToString(utcDateTimeFormat) },
        };

        if (!string.IsNullOrEmpty(adminPassCode))
        {
            replacementValues.Add("AdminPassCode", adminPassCode);
        }

        return Regex.Replace(value, @"\$\{([a-z0-9-+]+)\}", group =>
            replacementValues.TryGetValue(group.Groups[1].Value, out value!)
                ? value
                : group.Name, RegexOptions.IgnoreCase);
    }
}