using System.Net.Mime;
using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Contexts;
using HolcombeScores.Test.Http;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class RequestSteps : StepBase
{
    private readonly ITestingConfiguration _testingConfiguration;

    public RequestSteps(TestingConfigurationFactory testingConfigurationFactory, ApiScenarioContext scenarioContext)
        : base(scenarioContext)
    {
        _testingConfiguration = testingConfigurationFactory.Create();
    }

    [Given(@"a (DELETE|GET) request is sent to the api route ([a-zA-Z0-9/\-_]+)")]
    [Then(@"a (DELETE|GET) request is sent to the api route ([a-zA-Z0-9/\-_]+)")]
    public void ARequestIsSetToApiRoute(string method, string apiRoute)
    {
        RequestBuilder = NewRequestBuilder()
            .ForUri(_testingConfiguration.ApiAddress, apiRoute)
            .WithMethod(GetHttpMethod(method));
    }

    [Given(@"a (DELETE|POST|PATCH|PUT) request is sent to the api route (.+) with the following content")]
    [Then(@"a (DELETE|POST|PATCH|PUT) request is sent to the api route (.+) with the following content")]
    public void ARequestIsSetToApiRoute(string method, string apiRoute, string body)
    {
        RequestBuilder = NewRequestBuilder()
            .ForUri(_testingConfiguration.ApiAddress, apiRoute)
            .WithData(GetHttpMethod(method), MediaTypeNames.Application.Json, SupplantValues(body, _testingConfiguration.AdminPassCode));
    }

    private HttpRequestBuilder NewRequestBuilder()
    {
        Response = null;
        return new HttpRequestBuilder(TestContextId);
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