using System;
using System.Collections.Generic;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface INewGameDtoAdapter
    {
        Game AdaptToGame(NewGameDto newGameDto, ActionResultDto<GameDto> actionResult);
        IAsyncEnumerable<GamePlayer> AdaptSquad(NewGameDto newGameDto, Guid gameId, ActionResultDto<GameDto> actionResult);
    }
}
