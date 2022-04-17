using System.Collections.Generic;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDtoAdapter
    {
        GameDto Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals);
        Game Adapt(GameDto game);
    }
}
