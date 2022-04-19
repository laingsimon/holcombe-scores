using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services
{
    public interface IGameService
    {
        Task<ActionResultDto<GameDto>> CreateGame(NewGameDto newGameDto);
        IAsyncEnumerable<GameDto> GetAllGames();
        Task<GameDto> GetGame(Guid id);
        Task<ActionResultDto<GameDto>> DeleteGame(Guid id);
        Task<ActionResultDto<GameDto>> DeleteGamePlayer(Guid gameId, int playerNumber);
        Task<ActionResultDto<GameDto>> DeleteGoal(Guid gameId, Guid goalId);
        Task<ActionResultDto<GameDto>> RecordGoal(GoalDto goalDto);
    }
}
