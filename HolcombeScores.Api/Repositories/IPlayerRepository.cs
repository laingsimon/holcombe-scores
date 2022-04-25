using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public interface IPlayerRepository
    {
        IAsyncEnumerable<Player> GetAll(Guid? teamId);
        Task<Player> GetByNumber(Guid teamId, int number);
        Task AddPlayer(Player player);
        Task UpdatePlayer(Guid teamId, int playerNumber, string playerName);
        Task DeletePlayer(Guid teamId, int playerNumber);
    }
}
