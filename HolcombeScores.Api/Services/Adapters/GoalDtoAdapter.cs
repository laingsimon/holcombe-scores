using System;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GoalDtoAdapter : IGoalDtoAdapter
    {
        private readonly IPlayerDtoAdapter _playerAdapter;
        private readonly IPlayerRepository _playerRepository;

        public GoalDtoAdapter(IPlayerDtoAdapter playerAdapter, IPlayerRepository playerRepository)
        {
            _playerAdapter = playerAdapter;
            _playerRepository = playerRepository;
        }

        public async Task<GoalDto> Adapt(Goal goal)
        {
            if (goal == null)
            {
                return null;
            }

            var player = goal.HolcombeGoal 
                ? await _playerRepository.GetByNumber(goal.TeamId ?? Guid.Empty, goal.PlayerNumber ?? -1)
                : null;

            return new GoalDto
            {
                Player = _playerAdapter.Adapt(player),
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
                GameId = goal.GameId,
            };
        }

        public Goal Adapt(GoalDto goal, Game game)
        {
            if (goal == null)
            {
                return null;
            }

            return new Goal
            {
                PlayerNumber = goal.HolcombeGoal ? goal.Player.Number : null,
                TeamId = goal.HolcombeGoal ? game.TeamId : null,
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
                GameId = goal.GameId,
            };
        }
    }
}
