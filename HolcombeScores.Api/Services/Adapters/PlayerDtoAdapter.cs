using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class PlayerDtoAdapter : IPlayerDtoAdapter
    {
        public PlayerDto Adapt(Player player)
        {
            if (player == null)
            {
                return null;
            }

            return new PlayerDto
            {
                Name = player.Name,
                Number = player.Number,
                TeamId = player.TeamId,
                Id = player.Id,
            };
        }

        public Player Adapt(PlayerDto player)
        {
            if (player == null)
            {
                return null;
            }

            return new Player
            {
                Name = player.Name,
                Number = player.Number,
                TeamId = player.TeamId,
                Id = player.Id,
            };
        }
    }
}