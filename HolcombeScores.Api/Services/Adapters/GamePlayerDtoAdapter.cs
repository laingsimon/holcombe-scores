using HolcombeScores.Api.Models;
using HolcombeScores.Models;

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
                Number = player.Number,
                TeamId = player.TeamId,
            };
        }
    }
}
