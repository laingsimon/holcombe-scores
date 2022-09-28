using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Contexts;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class ResponseSteps : StepBase
{
    public ResponseSteps(TestingConfigurationFactory testingConfigurationFactory, ApiScenarioContext scenarioContext)
        : base(scenarioContext)
    { }

    [Then(@"the response has the following headers")]
    public async Task ThenTheResponseHasTheFollowingHeaders(Table headers)
    {
        var response = await GetResponse();

        foreach (var headerRow in SupplantValues(headers).Rows)
        {
            var headerName = headerRow["Header"];
            var headerValue = headerRow["Value"];

            Assert.That(response.Headers.Keys, Has.Member(headerName));
            Assert.That(response.Headers[headerName], Is.EqualTo(headerValue));
        }
    }

    [Then("the response set the following cookies")]
    public async Task ThenTheResponseHasTheFollowingCookies(Table cookieSpec)
    {
        var response = await GetResponse();

        foreach (var cookieSpecRow in SupplantValues(cookieSpec).Rows)
        {
            var name = cookieSpecRow["Name"];
            var valueRegex = SupplantValues(cookieSpecRow["ValueRegex"]);
            var httpOnly = bool.Parse(cookieSpecRow["HttpOnly"]);
            var secure = bool.Parse(cookieSpecRow["Secure"]);

            var cookie = response.Cookies.SingleOrDefault(c => c.Name.Equals(name));
            Assert.That(cookie, Is.Not.Null, $"Cookie not found with name {name}");
            Assert.That(cookie!.Value, Does.Match(valueRegex), $"Cookie {name} does not have correct value - was " + cookie.Value);
            Assert.That(cookie.HttpOnly, Is.EqualTo(httpOnly), $"Cookie {name} does not have correct HttpOnly value");
            Assert.That(cookie.Secure, Is.EqualTo(secure), $"Cookie {name} does not have correct Secure value");
        }
    }

    [Then("the response is ([A-Za-z]+)")]
    public async Task ThenTheResponseIs(string statusCodeName)
    {
        var response = await GetResponse();

        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.Not.Null);
        Assert.That(response.StatusCode.ToString(), Is.EqualTo(statusCodeName), () =>
        {
            return @$"Expected: ""{statusCodeName}""
But was:  ""{response.StatusCode}""
With a {RequestBuilder.Method} request to {RequestBuilder.RelativeUri} with the request body:
{RequestBuilder.Content}
With the response body:
{response.Body}";
        });
    }

    [Then("the cookie (.+) is stashed as (.+)")]
    public async Task ThenTheCookieIsStashed(string cookieName, string stashName)
    {
        var response = await GetResponse();

        var cookie = response.Cookies.SingleOrDefault(c => c.Name.Equals(cookieName));
        Assert.That(cookie, Is.Not.Null, $"Cookie {cookieName} was not found");
        Stash[stashName] = cookie!.Value;
    }
}