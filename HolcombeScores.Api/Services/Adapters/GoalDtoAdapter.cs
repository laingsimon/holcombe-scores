using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GoalDtoAdapter : IGoalDtoAdapter
    {
        public GoalDto Adapt(Goal goal)
        {
            if (goal == null)
            {
                return null;
            }

            return new GoalDto
            {
                Player = new PlayerDto
                {
                    Number = goal.PlayerNumber,
                    TeamId = goal.TeamId,
                    Name = "Todo",
                },
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
