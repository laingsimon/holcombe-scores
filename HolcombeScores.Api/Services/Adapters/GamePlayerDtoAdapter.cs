using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GamePlayerDtoAdapter : IGamePlayerDtoAdapter
    {
        public PlayerDto Adapt(GamePlayer player)
        {
            if (player == null)
            {
                return null;
            }

            return new PlayerDto
            {
                Name = player.Name,
                Number = player.Number == PlayerDtoAdapter.NoNumber ? null : player.Number,
                TeamId = player.TeamId,
                Id = player.PlayerId,
            };
        }
    }
}
