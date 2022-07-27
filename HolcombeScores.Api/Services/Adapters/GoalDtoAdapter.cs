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

            var player = goal.HolcombeGoal && goal.PlayerId.HasValue
                ? await _playerRepository.Get(goal.PlayerId.Value)
                : null;

            return new GoalDto
            {
                Player = _playerAdapter.Adapt(player),
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
                GameId = goal.GameId,
                GoalId = goal.GoalId,
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
                GoalId = goal.GoalId,
            };
        }
    }
}
