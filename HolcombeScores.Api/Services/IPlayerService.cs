using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services
{
    public interface IPlayerService
    {
        Task<ActionResultDto<PlayerDto>> CreateOrUpdatePlayer(PlayerDto player);
        IAsyncEnumerable<PlayerDto> GetAllPlayers();
        Task<ActionResultDto<PlayerDto>> DeletePlayer(PlayerDto player);
    }
}
