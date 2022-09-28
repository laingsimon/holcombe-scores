using HolcombeScores.Test.Configuration;
using HolcombeScores.Test.Contexts;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

/// <summary>
/// https://github.com/SpecFlowOSS/SpecFlow/issues/2608
///
/// The background isn't awaited, so scenario steps execute before the background has completed.
/// These steps patch this until the issue is resolved.
/// </summary>
[Binding]
public class BackgroundPatchSteps
{
    private readonly CompositeSteps _compositeSteps;
    private readonly ResponseSteps _responseSteps;
    private readonly JsonSteps _jsonSteps;

    public BackgroundPatchSteps(ApiScenarioContext apiScenarioContext,
        TestingConfigurationFactory testingConfigurationFactory)
    {
        _compositeSteps = new CompositeSteps(apiScenarioContext);
        _responseSteps = new ResponseSteps(testingConfigurationFactory, apiScenarioContext);
        _jsonSteps = new JsonSteps(apiScenarioContext);
    }

    [Given("I request admin access to the system SYNC")]
    public void RequestAdminAccessToTheSystem()
    {
        _compositeSteps.RequestAdminAccessToTheSystem().Wait();
    }

    [Then("I create a team SYNC")]
    public void CreateATeam()
    {
        _compositeSteps.CreateATeam().Wait();
    }

    [Then("I create a game SYNC")]
    public void CreateAGame()
    {
        _compositeSteps.CreateAGame().Wait();
    }

    [Then("I create a player SYNC")]
    public void CreateAPlayer()
    {
        _compositeSteps.CreateAPlayer().Wait();
    }

    [Then("I add the player to the game SYNC")]
    public void AddThePlayerToTeGame()
    {
        _compositeSteps.AddThePlayerToTheGame().Wait();
    }

    [Then("the response is ([A-Za-z]+) SYNC")]
    public void TheResponseIs(string statusCode)
    {
        _responseSteps.ThenTheResponseIs(statusCode).Wait();
    }

    [Given(@"the property (.+) is stashed as ([A-Za-z0-9]+) SYNC")]
    [Then(@"the property (.+) is stashed as ([A-Za-z0-9]+) SYNC")]
    public void ThenThePropertyIsStashed(string propertyPath, string stashName)
    {
        _jsonSteps.ThenThePropertyIsStashed(propertyPath, stashName).Wait();
    }

    [Given("the SYNC request was successful with the message (.+)")]
    [Then("the SYNC request was successful with the message (.+)")]
    public void TheRequestWasSuccessful(string message)
    {
        _compositeSteps.TheRequestWasSuccessful(message).Wait();
    }
}