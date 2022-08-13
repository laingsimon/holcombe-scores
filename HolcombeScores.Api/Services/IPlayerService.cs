using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services
{
    public interface IPlayerService
    {
        Task<ActionResultDto<PlayerDto>> CreateOrUpdatePlayer(PlayerDto player);
        IAsyncEnumerable<PlayerDto> GetAllPlayers(Guid teamId);
        Task<ActionResultDto<PlayerDto>> DeletePlayer(Guid playerId);
        Task<ActionResultDto<PlayerDto>> TransferPlayer(TransferPlayerDto transferDto);
        IAsyncEnumerable<PlayerDto> GetPlayers(Guid teamId);
    }
}
