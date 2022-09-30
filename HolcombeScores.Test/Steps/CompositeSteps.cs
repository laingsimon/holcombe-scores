using System.Net;
using System.Net.Mime;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Test.Contexts;
using HolcombeScores.Test.Http;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

[Binding]
public class CompositeSteps : StepBase
{
    private readonly ApiScenarioContext _scenarioContext;

    public CompositeSteps(ApiScenarioContext scenarioContext)
        : base(scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given("the request was successful with the message (.+)")]
    [Then("the request was successful with the message (.+)")]
    public async Task TheRequestWasSuccessful(string message)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        var result = JsonConvert.DeserializeObject<ActionResultDto<object>>(response.Body!);
        Assert.That(result!.Success, Is.True, response.Body);
        Assert.That(result.Messages, Has.Member(message));
    }

    [Given("the request failed with the error (.+)")]
    [Then("the request failed with the error (.+)")]
    public async Task TheRequestFailedWithError(string error)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        var result = JsonConvert.DeserializeObject<ActionResultDto<object>>(response.Body!);
        Assert.That(result!.Success, Is.False, response.Body);
        Assert.That(result.Errors, Has.Member(error));
    }

    [Given("the request failed with the warning (.+)")]
    [Then("the request failed with the warning (.+)")]
    public async Task TheRequestFailedWithWarning(string warning)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        var result = JsonConvert.DeserializeObject<ActionResultDto<object>>(response.Body!);
        Assert.That(result!.Success, Is.False, response.Body);
        Assert.That(result.Warnings, Has.Member(warning));
    }

    [Given("the request failed with the message (.+)")]
    [Then("the request failed with the message (.+)")]
    public async Task TheRequestFailedWithMessage(string message)
    {
        var response = await GetResponse();

        Assert.That(response.Headers["Content-Type"], Contains.Substring(MediaTypeNames.Application.Json));
        Assert.That(response.Body, Is.Not.Null.Or.Empty);
        var result = JsonConvert.DeserializeObject<ActionResultDto<object>>(response.Body!);
        Assert.That(result!.Success, Is.False, response.Body);
        Assert.That(result.Messages, Has.Member(message));
    }

    [Given("I request admin access to the system")]
    [Then("I request admin access to the system")]
    public async Task RequestAdminAccessToTheSystem()
    {
        var accessRequest = new AccessRequestDto { Name = $"{_scenarioContext.Name}_{TestContextId}" };
        var accessRequested = await GetResponse(NewRequestBuilder("/api/Access/Request")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(accessRequest)));
        Assert.That(accessRequested.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Access could not be requested");

        var accessesForRecovery = await GetResponse(NewRequestBuilder("/api/Access/Recover")
            .WithMethod(HttpMethod.Get));
        Assert.That(accessesForRecovery.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "Access for recovery couldn't be retrieved");

        var recoveryIds = JsonConvert.DeserializeObject<RecoverAccessDto[]>(accessesForRecovery.Body!);
        var recoveryId = recoveryIds!.First().RecoveryId;
        Stash["recoveryId"] = recoveryId;

        var accessRecovered = await GetResponse(NewRequestBuilder("/api/Access/Recover")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(new RecoverAccessDto
            {
                RecoveryId = recoveryId,
                Name = accessRequest.Name,
                Type = "AccessRequest",
                AdminPassCode = TestingConfiguration.AdminPassCode,
                Admin = true,
            })));
        Assert.That(accessRecovered.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Access was not recovered");

        var accessRecoveredResult = JsonConvert.DeserializeObject<ActionResultDto<AccessDto>>(accessRecovered.Body!);
        Assert.That(accessRecoveredResult!.Success, Is.True, "Access was not recovered");
        Assert.That(accessRecoveredResult.Outcome.Admin, Is.True, "Access is not an admin");
        Stash["userId"] = accessRecovered.Cookies["HS_User"]!.Value;
    }

    [Given("I create a team")]
    [Then("I create a team")]
    public async Task CreateATeam()
    {
        var team = new TeamDto
        {
            Name = $"{_scenarioContext.Name}_{_scenarioContext.ScenarioUniqueId}",
            Coach = _scenarioContext.Name,
        };
        var createTeamResponse = await GetResponse(NewRequestBuilder("/api/Team")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(team)));
        Assert.That(createTeamResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Team was not created");

        var result = JsonConvert.DeserializeObject<ActionResultDto<TeamDto>>(createTeamResponse.Body!);
        Assert.That(result!.Success, Is.True, $"Team was not created: {createTeamResponse.Body}");
        Stash["teamId"] = result.Outcome.Id.ToString();
    }

    [Given("I create a game")]
    [Then("I create a game")]
    public async Task CreateAGame()
    {
        var game = new GameDetailsDto
        {
            TeamId = Guid.Parse(Stash["teamId"]),
            Date = DateTime.UtcNow,
            Opponent = $"{_scenarioContext.Name}_{_scenarioContext.ScenarioUniqueId}",
            PlayingAtHome = true,
            PlayerIds = Array.Empty<Guid>(),
            Training = false
        };
        var createGameResponse = await GetResponse(NewRequestBuilder("/api/Game")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(game)));
        Assert.That(createGameResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Game was not created");

        var result = JsonConvert.DeserializeObject<ActionResultDto<GameDto>>(createGameResponse.Body!);
        Assert.That(result!.Success, Is.True, "Game was not created");
        Stash["gameId"] = result.Outcome.Id.ToString();
    }

    [Given("I create a player")]
    [Then("I create a player")]
    public async Task CreateAPlayer()
    {
        var player = new PlayerDto
        {
            Name = $"{_scenarioContext.Name}_Player",
            Number = 1,
            TeamId = Guid.Parse(Stash["teamId"]),
        };
        var createPlayerResponse = await GetResponse(NewRequestBuilder("/api/Player")
            .WithData(HttpMethod.Put, MediaTypeNames.Application.Json, Json(player)));
        Assert.That(createPlayerResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Player was not created");

        var result = JsonConvert.DeserializeObject<ActionResultDto<PlayerDto>>(createPlayerResponse.Body!);
        Assert.That(result!.Success, Is.True, "Player was not created");
        Stash["playerId"] = result.Outcome.Id.ToString();
    }

    [Given("I add the player to the game")]
    [Then("I add the player to the game")]
    public async Task AddThePlayerToTheGame()
    {
        var game = new ExistingGameDetailsDto
        {
            Id = Guid.Parse(Stash["gameId"]),
            TeamId = Guid.Parse(Stash["teamId"]),
            Opponent = $"{_scenarioContext.Name}_{_scenarioContext.ScenarioUniqueId}",
            PlayingAtHome = true,
            PlayerIds = new[]
            {
                Guid.Parse(Stash["playerId"]),
            },
            Training = false,
            Date = DateTime.UtcNow,
        };
        var response = await GetResponse(NewRequestBuilder("/api/Game")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(game)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Player was not added to game");
        var result = JsonConvert.DeserializeObject<ActionResultDto<GameDto>>(response.Body!);
        Assert.That(result!.Errors, Is.Empty);
        Assert.That(result.Warnings, Is.Empty);
    }

    [Given("I record a holcombe goal")]
    [When("I record a holcombe goal")]
    [Then("I record a holcombe goal")]
    public async Task RecordAHolcombeGoal()
    {
        await RecordAGoal(true);
    }

    [Given("I record an opponent goal")]
    [When("I record an opponent goal")]
    [Then("I record an opponent goal")]
    public async Task RecordAnOpponentGoal()
    {
        await RecordAGoal(false);
    }

    [Given("I request and grant access to the team")]
    [When("I request and grant access to the team")]
    [Then("I request and grant access to the team")]
    public async Task RequestAndGrantAccessToTheTeam()
    {
        var teamAccessRequest = new AccessRequestDto
        {
            Name = $"{_scenarioContext.Name}_{_scenarioContext.ScenarioUniqueId}",
            UserId = Guid.Parse(Stash["userId"]),
            TeamId = Guid.Parse(Stash["teamId"]),
        };

        var accessRequestResponse = await GetResponse(NewRequestBuilder("/api/Access/Request")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(teamAccessRequest)));
        Assert.That(accessRequestResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Could not request access");

        var grantAccess = new AccessResponseDto
        {
            UserId = Guid.Parse(Stash["userId"]),
            Allow = true,
            Reason = $"{_scenarioContext.Name}_{_scenarioContext.ScenarioUniqueId}",
            TeamId = Guid.Parse(Stash["teamId"]),
        };

        var grantAccessResponse = await GetResponse(NewRequestBuilder("/api/Access/Respond")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(grantAccess)));
        Assert.That(grantAccessResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Could not grant access");
    }

    [Given("I change my access to that of a manager")]
    [When("I change my access to that of a manager")]
    [Then("I change my access to that of a manager")]
    public async Task ChangeMyAccessToManager()
    {
        var access = new AccessDto
        {
            UserId = Guid.Parse(Stash["userId"]),
            Admin = false,
            Manager = true,
        };
        var response = await GetResponse(NewRequestBuilder("/api/Access")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(access)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Given("I change my access to that of a user")]
    [When("I change my access to that of a user")]
    [Then("I change my access to that of a user")]
    public async Task ChangeMyAccessToUser()
    {
        var access = new AccessDto
        {
            UserId = Guid.Parse(Stash["userId"]),
            Admin = false,
            Manager = false,
        };
        var response = await GetResponse(NewRequestBuilder("/api/Access")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(access)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Given("the game has started")]
    [When("the game has started")]
    [Then("the game has started")]
    public async Task TheGameHasStarted()
    {
        var game = new ExistingGameDetailsDto
        {
            Id = Guid.Parse(Stash["gameId"]),
            TeamId = Guid.Parse(Stash["teamId"]),
            Date = DateTime.UtcNow,
            PlayingAtHome = true,
            PlayerIds = new[]
            {
                Guid.Parse(Stash["playerId"])
            }
        };
        var response = await GetResponse(NewRequestBuilder("/api/Game")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(game)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Given("the game has finished")]
    [When("the game has finished")]
    [Then("the game has finished")]
    public async Task TheGameHasFinished()
    {
        var game = new ExistingGameDetailsDto
        {
            Id = Guid.Parse(Stash["gameId"]),
            TeamId = Guid.Parse(Stash["teamId"]),
            Date = DateTime.UtcNow.AddHours(-5),
            PlayingAtHome = true,
            PlayerIds = new[]
            {
                Guid.Parse(Stash["playerId"])
            }
        };
        var response = await GetResponse(NewRequestBuilder("/api/Game")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(game)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Given("the game hasn't started")]
    [When("the game hasn't started")]
    [Then("the game hasn't started")]
    public async Task TheGameHasNotStarted()
    {
        var game = new ExistingGameDetailsDto
        {
            Id = Guid.Parse(Stash["gameId"]),
            TeamId = Guid.Parse(Stash["teamId"]),
            Date = DateTime.UtcNow.AddHours(+5),
            PlayingAtHome = true,
            PlayerIds = new[]
            {
                Guid.Parse(Stash["playerId"])
            }
        };
        var response = await GetResponse(NewRequestBuilder("/api/Game")
            .WithData(HttpMethod.Patch, MediaTypeNames.Application.Json, Json(game)));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private async Task RecordAGoal(bool holcombeGoal)
    {
        var goal = new GoalDto
        {
            HolcombeGoal = holcombeGoal,
            Player = new PlayerDto
            {
                Name = "player",
                Number = 1,
                TeamId = Guid.Parse(Stash["teamId"]),
                Id = Guid.Parse(Stash["playerId"]),
            },
            GameId = Guid.Parse(Stash["gameId"]),
            RecordGoalToken = Stash["goalRecordToken"],
            Time = DateTime.UtcNow,
        };
        var response = await GetResponse(NewRequestBuilder("/api/Game/Goal")
            .WithData(HttpMethod.Post, MediaTypeNames.Application.Json, Json(goal)));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Goal was not recorded");
    }

    private HttpRequestBuilder NewRequestBuilder(string relativeUri)
    {
        return new HttpRequestBuilder(TestContextId, RequestBuilder)
            .ForUri(TestingConfiguration.ApiAddress, relativeUri);
    }

    private async Task<HttpResponse> GetResponse(HttpRequestBuilder requestBuilder)
    {
        RequestBuilder = requestBuilder;
        return await RequestBuilder.GetResponse();
    }

    private string Json(object value)
    {
        return JsonConvert.SerializeObject(value);
    }
}