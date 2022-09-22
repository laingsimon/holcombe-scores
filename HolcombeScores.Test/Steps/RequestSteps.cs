using System.Net.Mime;
using HolcombeScores.Test.Contexts;
using HolcombeScores.Test.Http;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class RequestSteps : StepBase
{
    public RequestSteps(ApiScenarioContext scenarioContext)
        : base(scenarioContext)
    {
    }

    [Given(@"a (DELETE|GET) request is sent to the api route ([a-zA-Z0-9/\-_\{\}\$]+)")]
    [When(@"a (DELETE|GET) request is sent to the api route ([a-zA-Z0-9/\-_\{\}\$]+)")]
    [Then(@"a (DELETE|GET) request is sent to the api route ([a-zA-Z0-9/\-_\{\}\$]+)")]
    public void ARequestIsSetToApiRoute(string method, string apiRoute)
    {
        RequestBuilder = NewRequestBuilder()
            .ForUri(TestingConfiguration.ApiAddress, SupplantValues(apiRoute))
            .WithMethod(GetHttpMethod(method));
    }

    [Given(@"a (DELETE|POST|PATCH|PUT) request is sent to the api route (.+) with the following content")]
    [When(@"a (DELETE|POST|PATCH|PUT) request is sent to the api route (.+) with the following content")]
    [Then(@"a (DELETE|POST|PATCH|PUT) request is sent to the api route (.+) with the following content")]
    public void ARequestIsSetToApiRoute(string method, string apiRoute, string body)
    {
        RequestBuilder = NewRequestBuilder()
            .ForUri(TestingConfiguration.ApiAddress,  SupplantValues(apiRoute))
            .WithData(GetHttpMethod(method), MediaTypeNames.Application.Json, SupplantValues(body, TestingConfiguration.AdminPassCode));
    }

    private HttpRequestBuilder NewRequestBuilder()
    {
        return new HttpRequestBuilder(TestContextId, RequestBuilder);
    }

    private static HttpMethod GetHttpMethod(string method)
    {
        return method switch
        {
            "DELETE" => HttpMethod.Delete,
            "GET" => HttpMethod.Get,
            "PATCH" => HttpMethod.Patch,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            _ => throw new ArgumentOutOfRangeException("Unsupported http method " + method)
        };
    }
}