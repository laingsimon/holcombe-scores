using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class GameController : Controller
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet("/api/Games")]
        public IAsyncEnumerable<GameDto> List()
        {
            return _gameService.GetAllGames();
        }

        [HttpGet("/api/Game/{id}")]
        public async Task<GameDto> Get(Guid id)
        {
            return await _gameService.GetGame(id);
        }

        [HttpPost("/api/Game/")]
        public async Task<ActionResultDto<GameDto>> NewGame(NewGameDto newGameDto)
        {
            return await _gameService.CreateGame(newGameDto);
        }
    }
}