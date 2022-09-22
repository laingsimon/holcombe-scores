using System.Net;
using System.Net.Mime;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Contexts;
using HolcombeScores.Test.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class ApiTestContextSteps
{
    private static ApiSuiteContext? _suiteContext;

    [BeforeFeature]
    public static async Task BeforeFeature(TestingConfigurationFactory testingConfigurationFactory,
        ApiFeatureContext featureContext)
    {
        if (_suiteContext != null)
        {
            _suiteContext.TestContextUsages.Add(featureContext.Name);
            featureContext.TestContextId = _suiteContext.TestContextId!.Value;
            featureContext.TestingConfiguration = _suiteContext.TestingConfiguration;
            return;
        }

        var configurations = new Dictionary<string, string>
        {
            { "Testing", "true" },
            { "IndexHtmlPath", @"..\..\..\..\HolcombeScores.Api\ClientApp\public\index.html" }
        };
        var testingConfiguration = testingConfigurationFactory.Create();
        var apiInstance = ApiLauncher.LaunchApi(testingConfiguration.ApiAddress, configurations);
        var requestBuilder = new HttpRequestBuilder(null);
        var json = JsonConvert.SerializeObject(new CreateTestingContextRequestDto
        {
            CopyExistingTables = false,
            SetContextRequiredCookie = true
        });
        var response = await requestBuilder.ForUri(testingConfiguration.ApiAddress, "/api/Testing")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, json)
            .GetResponse();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), string.IsNullOrEmpty(response.Body) ? "<Empty body>" : response.Body);
        var result = JsonConvert.DeserializeObject<ActionResultDto<TestingContextCreatedDto>>(response.Body!);
        Assert.That(result!.Success, Is.True, response.Body);
        _suiteContext = new ApiSuiteContext(result.Outcome.ContextId, testingConfiguration, apiInstance);
        featureContext.TestingConfiguration = _suiteContext.TestingConfiguration;
        featureContext.TestContextId = _suiteContext.TestContextId!.Value;
    }

    [AfterFeature]
    public static async Task AfterFeature(TestingConfigurationFactory testingConfigurationFactory,
        ApiFeatureContext featureContext)
    {
        if (_suiteContext == null)
        {
            return;
        }

        _suiteContext.TestContextUsages.Remove(featureContext.Name);
        if (_suiteContext.TestContextUsages.Count == 0)
        {
            var suiteContext = _suiteContext;
            _suiteContext = null;
            var testingConfiguration = suiteContext.TestingConfiguration;
            var requestBuilder = new HttpRequestBuilder(null);
            var response = await requestBuilder.ForUri(testingConfiguration.ApiAddress, "/api/Testing")
                .WithMethod(HttpMethod.Delete)
                .GetResponse();

            featureContext.TestContextId = Guid.Empty;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), string.IsNullOrEmpty(response.Body) ? "<Empty body>" : response.Body);
            suiteContext.ApiInstance.Dispose();
        }
    }
}
