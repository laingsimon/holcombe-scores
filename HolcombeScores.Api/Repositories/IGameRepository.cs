using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public interface IGameRepository
    {
        IAsyncEnumerable<Game> GetAll(Guid? teamId);
        Task<Game> Get(Guid id);
        Task Add(Game game);
        Task<IEnumerable<GamePlayer>> GetPlayers(Guid gameId);
    }
}
