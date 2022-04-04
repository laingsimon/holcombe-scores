using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
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
    }
}