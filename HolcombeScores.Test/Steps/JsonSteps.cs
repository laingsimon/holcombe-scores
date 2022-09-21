using System.Net.Mime;
using System.Text.Json;
using HolcombeScores.Test.Constraints;
using HolcombeScores.Test.Contexts;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class JsonSteps : StepBase
{
    public JsonSteps(ApiScenarioContext scenarioContext)
        : base(scenarioContext)
    {
    }

    [Then(@"the response has an array with (\d+) elements")]
    public async Task ThenTheResponseHasAnArrayWithElementCount(int count)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        Assert.That(response.Body, Does.StartWith("["));
        try
        {
            var array = JsonSerializer.Deserialize<object[]>(response.Body!);
            Assert.That(array!.Length, Is.EqualTo(count));
        }
        catch (JsonException exc)
        {
            Assert.Fail(exc.Message + "\n\nStatusCode: " + response.StatusCode + "\n" + response?.Body);
        }
    }

    [Then("the response has the following properties")]
    public async Task ThenTheResponseHasTheFollowingProperties(Table properties)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        Assert.That(response.Body, Does.StartWith("{"));
        Assert.That(response.Body!, new MatchesJsonPropertiesConstraint(SupplantValues(properties), true), () =>
        {
            return $"Response body was {response.Body}";
        });
    }

    [Then("the response has an array element with the following properties")]
    public async Task ThenTheResponseHasAnArrayElementWithTheFollowingProperties(Table properties)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        Assert.That(response.Body, Does.StartWith("["));
        try
        {
            var array = JArray.Parse(response.Body!).Select(t => t.ToString());
            Assert.That(array, Has.Some.MatchingJson(SupplantValues(properties), false));
        }
        catch (JsonException exc)
        {
            Assert.Fail(exc.Message + "\n\nStatusCode: " + response?.StatusCode + "\n" + response?.Body);
        }
    }

    [Then(@"the property (.+) is stashed as (.+)")]
    public async Task ThenThePropertyIsStashed(string propertyPath, string stashName)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));

        var json = JToken.Parse(response.Body!);
        var selectedElement = json.SelectToken(propertyPath);
        Assert.That(selectedElement, Is.Not.Null, $"Json element not found at path: {propertyPath} from {response.Body}");
        Stash[stashName] = selectedElement!.Value<string>()!;
    }
}