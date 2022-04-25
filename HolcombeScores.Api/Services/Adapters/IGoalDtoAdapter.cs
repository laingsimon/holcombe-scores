using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGoalDtoAdapter
    {
        Task<GoalDto> Adapt(Goal goal);
        Goal Adapt(GoalDto goal, Game game);
    }
}
