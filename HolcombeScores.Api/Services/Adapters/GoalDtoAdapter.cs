using System;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GoalDtoAdapter : IGoalDtoAdapter
    {
        private readonly IPlayerDtoAdapter _playerDtoAdapter;
        private readonly IPlayerRepository _playerRepository;

        public GoalDtoAdapter(IPlayerDtoAdapter playerDtoAdapter, IPlayerRepository playerRepository)
        {
            _playerDtoAdapter = playerDtoAdapter;
            _playerRepository = playerRepository;
        }

        public async Task<GoalDto> Adapt(Goal goal)
        {
            if (goal == null)
            {
                return null;
            }

            var player = await _playerRepository.GetByNumber(goal.TeamId, goal.PlayerNumber);

            return new GoalDto
            {
                Player = _playerDtoAdapter.Adapt(player),
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
                GameId = goal.GameId,
            };
        }

        public Goal Adapt(GoalDto goal)
        {
            if (goal == null)
            {
                return null;
            }

            return new Goal
            {
                PlayerNumber = goal.Player.Number,
                TeamId = goal.Player.TeamId,
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
                GameId = goal.GameId,
            };
        }
    }
}
