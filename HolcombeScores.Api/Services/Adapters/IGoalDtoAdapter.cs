using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGoalDtoAdapter
    {
        Task<GoalDto> Adapt(Goal goal);
        Goal Adapt(GoalDto goal);
    }
}
