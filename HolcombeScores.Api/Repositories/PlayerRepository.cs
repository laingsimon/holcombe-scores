using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly TypedTableClient<Player> _playerTableClient;

        public PlayerRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _playerTableClient = new TypedTableClient<Player>(tableServiceClientFactory.CreateTableClient("Player"));
        }

        public IAsyncEnumerable<Player> GetAll(Guid? teamId)
        {
            if (teamId == null)
            {
                return _playerTableClient.QueryAsync();
            }

            return _playerTableClient.QueryAsync(p => p.TeamId == teamId);
        }

        public async Task<Player> GetByNumber(Guid teamId, int number)
        {
            return await _playerTableClient.SingleOrDefaultAsync(p =>
                p.TeamId == teamId && p.Number == number);
        }

        public async Task AddPlayer(Player player)
        {
            player.Timestamp = DateTimeOffset.UtcNow;
            player.PartitionKey = player.TeamId.ToString();
            player.RowKey = player.Number.ToString();
            player.ETag = ETag.All;

            await _playerTableClient.AddEntityAsync(player);
        }

        public async Task UpdatePlayer(Guid teamId, int playerNumber, string playerName)
        {
            var player = await GetByNumber(teamId, playerNumber);
            player.Name = playerName;
            player.Timestamp = DateTimeOffset.UtcNow;

            await _playerTableClient.UpdateEntityAsync(player, player.ETag);
        }

        public async Task DeletePlayer(Guid teamId, int playerNumber)
        {
            var player = await GetByNumber(teamId, playerNumber);

            if (player == null)
            {
                return;
            }

            await _playerTableClient.DeleteEntityAsync(player.PartitionKey, player.RowKey);
        }
    }
}
