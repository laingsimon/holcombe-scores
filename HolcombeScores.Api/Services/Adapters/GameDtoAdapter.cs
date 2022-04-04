using System.Linq;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDtoAdapter : IGameDtoAdapter
    {
        private readonly IGoalDtoAdapter _goalAdapter;
        private readonly IPlayerDtoAdapter _playerAdapter;

        public GameDtoAdapter(IGoalDtoAdapter goalAdapter, IPlayerDtoAdapter playerAdapter)
        {
            _goalAdapter = goalAdapter;
            _playerAdapter = playerAdapter;
        }

        public GameDto Adapt(Game game)
        {
            return new GameDto
            {
                TeamId = game.TeamId,
                Date = game.Date,
                Goals = game.Goals.Select(_goalAdapter.Adapt).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = game.Squad.Select(_playerAdapter.Adapt).ToArray(),
                PlayingAtHome = game.PlayingAtHome,
            };
        }

        public Game Adapt(GameDto game)
        {
            return new Game
            {
                Date = game.Date,
                Goals = game.Goals.Select(_goalAdapter.Adapt).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = game.Squad.Select(_playerAdapter.Adapt).ToArray(),
                PlayingAtHome = game.PlayingAtHome,
                TeamId = game.TeamId,
            };
        }
    }
}