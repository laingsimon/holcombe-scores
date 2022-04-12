using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GoalDtoAdapter : IGoalDtoAdapter
    {
        private readonly IPlayerDtoAdapter _playerAdapter;

        public GoalDtoAdapter(IPlayerDtoAdapter playerAdapter)
        {
            _playerAdapter = playerAdapter;
        }

        public GoalDto Adapt(Goal goal)
        {
            if (goal == null)
            {
                return null;
            }

            return new GoalDto
            {
                Player = _playerAdapter.Adapt(goal.Player),
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
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
                Player = _playerAdapter.Adapt(goal.Player),
                Time = goal.Time,
                HolcombeGoal = goal.HolcombeGoal,
            };
        }
    }
}
