using System.Text.RegularExpressions;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories;

public class SocialDetailRepository : ISocialDetailRepository
{
    private readonly IGameRepository _gameRepository;
    private readonly ITeamRepository _teamRepository;

    private static readonly SocialDetail Default = new SocialDetail
    {
        Description = "View and record results and availability for games/training",
        Title = "Holcombe Scores - view & record goals as they're scored",
    };

    private static readonly Dictionary<string, Func<Game, SocialDetail>> GameDetails = new()
    {
        { "^/game/(?<gameId>.+?)(/view)?/?$", ViewGame },
        { "^/game/(?<gameId>.+)/play/?$", PlayGame },
        { "^/game/(?<gameId>.+)/availability/?$", GameAvailability },
    };

    private static readonly Dictionary<string, Func<Team, SocialDetail>> TeamDetails = new()
    {
        { "^/team/(?<teamId>.+)/?$", ViewTeamGames },
    };

    public SocialDetailRepository(IGameRepository gameRepository, ITeamRepository teamRepository)
    {
        _gameRepository = gameRepository;
        _teamRepository = teamRepository;
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

            if (!Guid.TryParse(match.Groups["gameId"].Value, out var gameId))
            {
                continue;
            }

            var game = await _gameRepository.Get(gameId);
            if (game != null)
            {
                return value(game);
            }
        }

        foreach (var (key, value) in TeamDetails)
        {
            var match = Regex.Match(urlPrefix, key);

            if (!match.Success)
            {
                continue;
            }

            if (!Guid.TryParse(match.Groups["teamId"].Value, out var teamId))
            {
                continue;
            }

            var team = await _teamRepository.Get(teamId);
            if (team != null)
            {
                return value(team);
            }
        }

        return Default;
    }

    private static SocialDetail GameAvailability(Game game)
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

    private static SocialDetail PlayGame(Game game)
    {
        return new SocialDetail
        {
            Description = "See the score line and update the goals as they're scored",
            Title = $"Record goals in the game against {game.Opponent}",
        };
    }

    private static SocialDetail ViewGame(Game game)
    {
        return new SocialDetail
        {
            Description = game.Training
                ? "Views details of this training session and record availability"
                : "View the squad, record availability and view/record goals as the game is played",
            Title = game.Training
                ? $"View training session on {game.Date}"
                : $"View game against {game.Opponent}",
        };
    }

    private static SocialDetail ViewTeamGames(Team team)
    {
        return new SocialDetail
        {
            Description = $"View games and training sessions for {team.Name}",
            Title = $"Holcombe Scores - {team.Name}",
        };
    }
}