using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public interface IPlayerRepository
    {
        IAsyncEnumerable<Player> GetAll(Guid? teamId);
        Task<Player> Get(Guid id);
        Task<Player> GetByNumber(Guid teamId, int number);
        Task<Player> AddPlayer(Player player);
        Task UpdatePlayer(Guid id, Guid teamId, int? playerNumber, string playerName);
        Task DeletePlayer(Guid id);
    }
}
