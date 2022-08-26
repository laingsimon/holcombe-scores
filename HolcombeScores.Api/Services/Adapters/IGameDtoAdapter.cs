using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDtoAdapter
    {
        Task<GameDto> Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals, bool readOnly,
            string recordGoalToken);
        Game Adapt(GameDto game);
    }
}
