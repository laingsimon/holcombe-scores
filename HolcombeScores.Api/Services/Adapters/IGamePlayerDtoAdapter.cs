using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGamePlayerDtoAdapter
    {
        PlayerDto Adapt(GamePlayer player);
    }
}
