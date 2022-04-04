using HolcombeScores.Api.Models;
using HolcombeScores.Models;

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
            };
        }
    }
}