using System.Text.RegularExpressions;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;

namespace HolcombeScores.Api.Repositories;

public class SocialDetailRepository : ISocialDetailRepository
{
    private readonly IGameService _gameService;
    private readonly ITeamService _teamService;

    private static readonly Dictionary<string, Func<GameDto, SocialDetail>> GameDetails = new()
    {
        { "^/game/(?<gameId>.+)(/view)?/?$", ViewGame },
        { "^/game/(?<gameId>.+)/play/?$", PlayGame },
        { "^/game/(?<gameId>.+)/availability/?$", GameAvailability },
    };

    private static readonly Dictionary<string, Func<TeamDto, SocialDetail>> TeamDetails = new()
    {
        { "^/team/(?<teamId>.+)/?$", ViewTeamGames },
    };

    public SocialDetailRepository(IGameService gameService, ITeamService teamService)
    {
        _gameService = gameService;
        _teamService = teamService;
    }

    public async Task<SocialDetail> GetSocialDetail(string urlPrefix)
    {
        foreach (var (key, value) in GameDetails)
        {
            var match = Regex.Match(urlPrefix, key);

            if (!match.Success)
            {
                continue;
            }

            var gameId = Guid.Parse(match.Groups["gameId"].Value);
            var game = await _gameService.GetGame(gameId);

            return value(game);
        }

        foreach (var (key, value) in TeamDetails)
        {
            var match = Regex.Match(urlPrefix, key);

            if (!match.Success)
            {
                continue;
            }

            var teamId = Guid.Parse(match.Groups["teamId"].Value);
            var team = await _teamService.GetTeam(teamId);

            return value(team);
        }

        return new SocialDetail();
    }

    private static SocialDetail GameAvailability(GameDto game)
    {
        return new SocialDetail
        {
            Description = game.Training
                ? $"Record availability for training on {game.Date}"
                : $"Record availability for game against {game.Opponent} on {game.Date}",
            Title = game.Training
                ? "Record the availability of players for this training session"
                : "Record the availability of players for this game",
        };
    }

    private static SocialDetail PlayGame(GameDto game)
    {
        return new SocialDetail
        {
            Description = "See the score line and update the goals as they're scored",
            Title = $"Record goals in the game against {game.Opponent}",
        };
    }

    private static SocialDetail ViewGame(GameDto game)
    {
        return new SocialDetail
        {
            Description = game.Training
                ? "Views players"
                : "View players and goals",
            Title = game.Training
                ? $"View training on {game.Date}"
                : $"View game against {game.Opponent}",
        };
    }

    private static SocialDetail ViewTeamGames(TeamDto team)
    {
        return new SocialDetail
        {
            Description = "View games",
            Title = $"{team.Name}",
        };
    }
}