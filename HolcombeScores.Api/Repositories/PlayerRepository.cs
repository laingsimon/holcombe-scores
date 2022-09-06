using Azure;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly TypedTableClient<Player> _playerTableClient;

        public PlayerRepository(ITableClientFactory tableClientFactory)
        {
            _playerTableClient = new TypedTableClient<Player>(tableClientFactory.CreateTableClient("Player"));
        }

        public IAsyncEnumerable<Player> GetAll(Guid? teamId)
        {
            return teamId == null
                ? _playerTableClient.QueryAsync()
                : _playerTableClient.QueryAsync(p => p.TeamId == teamId);
        }

        public async Task<Player> GetByNumber(Guid teamId, int number)
        {
            return await _playerTableClient.SingleOrDefaultAsync(p =>
                p.TeamId == teamId && p.Number == number);
        }

        public async Task<Player> Get(Guid id)
        {
            return await _playerTableClient.SingleOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Player> AddPlayer(Player player)
        {
            player.Timestamp = DateTimeOffset.UtcNow;
            player.Id = Guid.NewGuid();
            player.ETag = ETag.All;
            player.PartitionKey = player.TeamId.ToString();
            player.RowKey = player.Id.ToString();

            await _playerTableClient.AddEntityAsync(player);
            return player;
        }

        public async Task UpdatePlayer(Guid playerId, Guid teamId, int? playerNumber, string playerName)
        {
            var player = await Get(playerId);
            player.Number = playerNumber;
            player.Name = playerName;
            player.Timestamp = DateTimeOffset.UtcNow;

            await _playerTableClient.UpdateEntityAsync(player, player.ETag);
        }

        public async Task DeletePlayer(Guid playerId)
        {
            var player = await Get(playerId);

            if (player == null)
            {
                return;
            }

            await _playerTableClient.DeleteEntityAsync(player.PartitionKey, player.RowKey);
        }
    }
}
