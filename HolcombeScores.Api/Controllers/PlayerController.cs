using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class PlayerController : Controller
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("/api/Players")]
        public IAsyncEnumerable<PlayerDto> List()
        {
            return _playerService.GetAllPlayers();
        }

        [HttpPut("/api/Player")]
        public async Task<ActionResultDto<PlayerDto>> Upsert(PlayerDto player)
        {
            return await _playerService.CreateOrUpdatePlayer(player);
        }

        [HttpDelete("/api/Player/{teamId}/{number}")]
        public async Task<ActionResultDto<PlayerDto>> Delete(Guid teamId, int number)
        {
            return await _playerService.DeletePlayer(teamId, number);
        }

        [HttpPost("/api/Player/Transfer")]
        public async Task<ActionResultDto<PlayerDto>> Delete(TransferPlayerDto transferDto)
        {
            return await _playerService.TransferPlayer(transferDto);
        }
    }
}
