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
        private readonly IGameDtoAdapter _gameAdapter;

        public GameDtoAdapter(IGoalDtoAdapter goalAdapter, IPlayerDtoAdapter playerAdapter, IGameDtoAdapter gameAdapter)
        {
            _goalAdapter = goalAdapter;
            _playerAdapter = playerAdapter;
            _gameAdapter = gameAdapter;
        }

        public GameDto Adapt(Game game, IEnumerable<GamePlayer> squad)
        {
            if (game == null)
            {
                return null;
            }

            return new GameDto
            {
                TeamId = game.TeamId,
                Date = game.Date,
                Goals = game.Goals.Select(_goalAdapter.Adapt).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = squad.Select(_gameAdapter.AdaptPlayer).ToArray(),
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
                Goals = game.Goals.Select(_goalAdapter.Adapt).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                PlayingAtHome = game.PlayingAtHome,
                TeamId = game.TeamId,
            };
        }
    }
}
