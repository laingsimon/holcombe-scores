using HolcombeScores.Api.Models.Dtos;
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
            return _gameService.GetAllGames(null);
        }

        [HttpGet("/api/Games/{teamId}")]
        public IAsyncEnumerable<GameDto> List(Guid teamId)
        {
            return _gameService.GetAllGames(teamId);
        }

        [HttpGet("/api/Game/{id}")]
        public async Task<GameDto> Get(Guid id)
        {
            return await _gameService.GetGame(id);
        }

        [HttpPost("/api/Game/")]
        public async Task<ActionResultDto<GameDto>> NewGame(GameDetailsDto gameDetailsDto)
        {
            return await _gameService.CreateGame(gameDetailsDto);
        }

        [HttpPatch("/api/Game/")]
        public async Task<ActionResultDto<GameDto>> UpdateGame(ExistingGameDetailsDto gameDetailsDto)
        {
            return await _gameService.UpdateGame(gameDetailsDto);
        }

        [HttpDelete("/api/Game/{id}")]
        public async Task<ActionResultDto<GameDto>> DeleteGame(Guid id)
        {
            return await _gameService.DeleteGame(id);
        }

        [HttpDelete("/api/Game/Player/{gameId}/{playerNumber}")]
        public async Task<ActionResultDto<GameDto>> DeleteGamePlayer(Guid gameId, int playerNumber)
        {
            return await _gameService.DeleteGamePlayer(gameId, playerNumber);
        }

        [HttpDelete("/api/Game/Goal/{gameId}/{goalId}")]
        public async Task<ActionResultDto<GameDto>> DeleteGameGoal(Guid gameId, Guid goalId)
        {
            return await _gameService.DeleteGoal(gameId, goalId);
        }

        [HttpPost("/api/Game/Goal")]
        public async Task<ActionResultDto<GameDto>> RecordGoal(GoalDto goalDto)
        {
            return await _gameService.RecordGoal(goalDto);
        }
    }
}
