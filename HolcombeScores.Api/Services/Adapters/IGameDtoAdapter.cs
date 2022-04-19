using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDtoAdapter
    {
        Task<GameDto> Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals);
        Game Adapt(GameDto game);
    }
}
