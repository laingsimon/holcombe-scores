using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly TableClient _playerTableClient;

        public PlayerRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _playerTableClient = tableServiceClientFactory.CreateTableClient("Player");
        }

        public IAsyncEnumerable<Player> GetAll(Guid? teamId)
        {
            if (teamId == null)
            {
                return _playerTableClient.QueryAsync<Player>();
            }

            return _playerTableClient.QueryAsync<Player>(p => p.TeamId == teamId);
        }

        public async Task<Player> GetByNumber(Guid? teamId, int number)
        {
            return await _playerTableClient.SingleOrDefaultAsync<Player>(p =>
                (p.TeamId == teamId) && p.Number == number);
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

        public async Task DeletePlayer(Guid teamId, int playerNumber, string playerName)
        {
            var player = await GetByNumber(teamId, playerNumber);

            if (player == null)
            {
                return null;
            }

            await _playerTableClient.DeleteEntityAsync(player, player.ETag);
        }
    }
}
