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
        Response ??= await RequestBuilder.Send();

        Assert.That(Response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(Response.Body, Is.Not.Null.Or.Empty);
        Assert.That(Response.Body, Does.StartWith("["));
        try
        {
            var array = JsonSerializer.Deserialize<object[]>(Response.Body!);
            Assert.That(array!.Length, Is.EqualTo(count));
        }
        catch (JsonException exc)
        {
            Assert.Fail(exc.Message + "\n\nStatusCode: " + Response?.StatusCode + "\n" + Response?.Body);
        }
    }

    [Then("the response has the following properties")]
    public async Task ThenTheResponseHasTheFollowingProperties(Table properties)
    {
        Response ??= await RequestBuilder.Send();

        Assert.That(Response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(Response.Body, Is.Not.Null.Or.Empty);
        Assert.That(Response.Body, Does.StartWith("{"));
        Assert.That(Response.Body!, new MatchesJsonPropertiesConstraint(SupplantValues(properties), true));
    }

    [Then("the response has an array element with the following properties")]
    public async Task ThenTheResponseHasAnArrayElementWithTheFollowingProperties(Table properties)
    {
        Response ??= await RequestBuilder.Send();

        Assert.That(Response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(Response.Body, Is.Not.Null.Or.Empty);
        Assert.That(Response.Body, Does.StartWith("["));
        try
        {
            var array = JArray.Parse(Response.Body!).Select(t => t.ToString());
            Assert.That(array, Has.Some.MatchingJson(SupplantValues(properties), false));
        }
        catch (JsonException exc)
        {
            Assert.Fail(exc.Message + "\n\nStatusCode: " + Response?.StatusCode + "\n" + Response?.Body);
        }
    }

    [Then(@"the property (.+) is stashed")]
    public async Task ThenThePropertyIsStashed(string propertyPath)
    {
        Response ??= await RequestBuilder.Send();

        Assert.That(Response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));

        var json = JToken.Parse(Response.Body!);
        Stash = json.SelectToken(propertyPath)!.Value<string>()!;
    }
}