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
        Response ??= await RequestBuilder.Send();

        foreach (var headerRow in SupplantValues(headers).Rows)
        {
            var headerName = headerRow["Header"];
            var headerValue = headerRow["Value"];

            Assert.That(Response.Headers.Keys, Has.Member(headerName));
            Assert.That(Response.Headers[headerName], Is.EqualTo(headerValue));
        }
    }

    [Then("the response set the following cookies")]
    public async Task ThenTheResponseHasTheFollowingCookies(Table cookieSpec)
    {
        Response ??= await RequestBuilder.Send();

        foreach (var cookieSpecRow in SupplantValues(cookieSpec).Rows)
        {
            var name = cookieSpecRow["Name"];
            var valueRegex = SupplantValues(cookieSpecRow["ValueRegex"]);
            var httpOnly = bool.Parse(cookieSpecRow["HttpOnly"]);
            var secure = bool.Parse(cookieSpecRow["Secure"]);

            var cookie = Response.Cookies.SingleOrDefault(c => c.Name.Equals(name));
            Assert.That(cookie, Is.Not.Null);
            Assert.That(cookie!.Value, Does.Match(valueRegex), $"Cookie {name} does not have correct value - was " + cookie.Value);
            Assert.That(cookie.HttpOnly, Is.EqualTo(httpOnly), $"Cookie {name} does not have correct HttpOnly value");
            Assert.That(cookie.Secure, Is.EqualTo(secure), $"Cookie {name} does not have correct Secure value");
        }
    }

    [Then("the response is (.+)")]
    public async Task ThenTheResponseIs(string statusCodeName)
    {
        Response ??= await RequestBuilder.Send();

        Assert.That(Response.StatusCode.ToString(), Is.EqualTo(statusCodeName), () =>
        {
            return @$"Expected: ""{statusCodeName}""
But was:  ""{Response.StatusCode}""
With the body:
{Response.Body}";
        });
    }

    [Then("the cookie (.+) is stashed")]
    public async Task ThenTheCookieIsStashed(string cookieName)
    {
        Response ??= await RequestBuilder.Send();

        var cookie = Response.Cookies.SingleOrDefault(c => c.Name.Equals(cookieName));
        Assert.That(cookie, Is.Not.Null, $"Cookie {cookieName} was not found");
        Stash = cookie!.Value;
    }
}