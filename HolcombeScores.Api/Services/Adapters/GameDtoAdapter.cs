using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDtoAdapter : IGameDtoAdapter
    {
        private readonly IGoalDtoAdapter _goalAdapter;
        private readonly IGamePlayerDtoAdapter _gamePlayerAdapter;

        public GameDtoAdapter(IGoalDtoAdapter goalAdapter, IGamePlayerDtoAdapter gamePlayerAdapter)
        {
            _goalAdapter = goalAdapter;
            _gamePlayerAdapter = gamePlayerAdapter;
        }

        public async Task<GameDto> Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals, bool readOnly, string recordGoalToken)
        {
            if (game == null)
            {
                return null;
            }

            return new GameDto
            {
                TeamId = game.TeamId,
                Date = game.Date,
                Goals = (await goals.SelectAsync(g => _goalAdapter.Adapt(g))).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = squad.Select(_gamePlayerAdapter.Adapt).ToArray(),
                PlayingAtHome = game.PlayingAtHome,
                ReadOnly = readOnly,
                Training = game.Training,
                RecordGoalToken = recordGoalToken,
            };
        }

        public Game Adapt(GameDto game)
        {
            if (game == null)
            {
                return null;
            }

            return new Game
            {
                Date = game.Date,
                Id = game.Id,
                Opponent = game.Opponent,
                PlayingAtHome = game.PlayingAtHome,
                TeamId = game.TeamId,
                Training = game.Training,
            };
        }
    }
}
