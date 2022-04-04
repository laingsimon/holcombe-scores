using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services
{
    public interface IGameService
    {
        Task<ActionResultDto<GameDto>> CreateGame(NewGameDto newGameDto);
        IAsyncEnumerable<GameDto> GetAllGames();
        Task<GameDto> GetGame(Guid id);
    }
}