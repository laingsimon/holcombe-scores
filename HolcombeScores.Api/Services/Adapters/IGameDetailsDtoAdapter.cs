using System;
using System.Collections.Generic;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IGameDetailsDtoAdapter
    {
        Game AdaptToGame(GameDetailsDto gameDetailsDto);
        IAsyncEnumerable<GamePlayer> AdaptSquad(GameDetailsDto gameDetailsDto, Guid gameId, List<string> missingPlayers);
        Game AdaptToGame(ExistingGameDetailsDto gameDetailsDto);
    }
}
