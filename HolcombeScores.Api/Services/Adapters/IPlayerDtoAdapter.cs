using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IPlayerDtoAdapter
    {
        PlayerDto Adapt(Player player);
        Player Adapt(PlayerDto player);
    }
}