using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDtoAdapter
    {
        Task<GameDto> Adapt(Game game, IReadOnlyCollection<GamePlayer> squad, IEnumerable<Goal> goals,
            GameDtoAdapter.AdapterContext context);
        Game Adapt(GameDto game);
    }
}
