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
    private static readonly ApiSuiteContext SuiteContext = new ApiSuiteContext();

    [BeforeFeature]
    public static async Task BeforeFeature(TestingConfigurationFactory testingConfigurationFactory, ApiFeatureContext featureContext)
    {
        if (!SuiteContext.TestContextId.HasValue)
        {
            var testingConfiguration = testingConfigurationFactory.Create();
            var requestBuilder = new HttpRequestBuilder(null);
            var json = JsonConvert.SerializeObject(new CreateTestingContextRequestDto
            {
                CopyExistingTables = false,
                SetContextRequiredCookie = true
            });
            var response = await requestBuilder.ForUri(testingConfiguration.ApiAddress, "/api/Testing")
                .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, json)
                .Send();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var result = JsonConvert.DeserializeObject<ActionResultDto<TestingContextCreatedDto>>(response.Body!);
            Assert.That(result!.Success, Is.True);
            SuiteContext.TestContextId = result.Outcome.ContextId;
        }

        SuiteContext.TestContextUsages.Add(featureContext.Name);
        featureContext.TestContextId = SuiteContext.TestContextId.Value;
    }

    [AfterFeature]
    public static async Task AfterFeature(TestingConfigurationFactory testingConfigurationFactory, ApiFeatureContext featureContext)
    {
        SuiteContext.TestContextUsages.Remove(featureContext.Name);
        if (SuiteContext.TestContextId.HasValue && SuiteContext.TestContextUsages.Count == 0)
        {
            var testingConfiguration = testingConfigurationFactory.Create();
            var requestBuilder = new HttpRequestBuilder(null);
            var response = await requestBuilder.ForUri(testingConfiguration.ApiAddress, "/api/Testing")
                .WithMethod(HttpMethod.Delete)
                .Send();

            featureContext.TestContextId = Guid.Empty;
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            SuiteContext.TestContextId = null;
        }
    }
}