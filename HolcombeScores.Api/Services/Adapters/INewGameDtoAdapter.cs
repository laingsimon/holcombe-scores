using System;
using System.Collections.Generic;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface INewGameDtoAdapter
    {
        Game AdaptToGame(NewGameDto newGameDto);
        IAsyncEnumerable<GamePlayer> AdaptSquad(NewGameDto newGameDto, Guid gameId, List<string> missingPlayers);
    }
}
