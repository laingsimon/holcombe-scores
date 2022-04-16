using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDtoAdapter
    {
        GameDto Adapt(Game game, IEnumerable<GamePlayer> squad);
        Game Adapt(GameDto game);
    }
}
