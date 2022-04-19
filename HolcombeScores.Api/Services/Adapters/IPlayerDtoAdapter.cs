using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IPlayerDtoAdapter
    {
        PlayerDto Adapt(Player player);
        Player Adapt(PlayerDto player);
    }
}