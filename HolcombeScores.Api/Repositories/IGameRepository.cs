using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public interface IGameRepository
    {
        IAsyncEnumerable<Game> GetAll(Guid? teamId);
        Task<Game> Get(Guid id);
        Task Add(Game game);
        Task<IEnumerable<GamePlayer>> GetPlayers(Guid gameId);
        Task AddGamePlayer(GamePlayer gamePlayer);
        Task<IEnumerable<Goal>> GetGoals(Guid gameId);
        Task AddGoal(Goal goal);
        Task DeleteGame(Guid id);
        Task DeleteGamePlayer(Guid gameId, int playerNumber);
        Task DeleteGoal(Guid gameId, Guid goalId);
        Task Update(Game game);
    }
}
