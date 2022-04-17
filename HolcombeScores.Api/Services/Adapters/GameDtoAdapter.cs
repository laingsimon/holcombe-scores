using System.Collections.Generic;
using System.Linq;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDtoAdapter : IGameDtoAdapter
    {
        private readonly IGoalDtoAdapter _goalAdapter;
        private readonly IPlayerDtoAdapter _playerAdapter;
        private readonly IGamePlayerDtoAdapter _gamePlayerAdapter;

        public GameDtoAdapter(IGoalDtoAdapter goalAdapter, IPlayerDtoAdapter playerAdapter, IGamePlayerDtoAdapter gamePlayerAdapter)
        {
            _goalAdapter = goalAdapter;
            _playerAdapter = playerAdapter;
            _gamePlayerAdapter = gamePlayerAdapter;
        }

        public GameDto Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals)
        {
            if (game == null)
            {
                return null;
            }

            return new GameDto
            {
                TeamId = game.TeamId,
                Date = game.Date,
                Goals = goals.Select(g => _goalAdapter.Adapt(g).Result).ToArray(), // TODO improve this, make this method Async too?
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = squad.Select(_gamePlayerAdapter.Adapt).ToArray(),
                PlayingAtHome = game.PlayingAtHome,
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
            };
        }
    }
}
