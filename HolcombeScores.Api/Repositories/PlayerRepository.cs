using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly IGenericEntityAdapter _genericEntityAdapter;
        private readonly TableClient _playerTableClient;

        public PlayerRepository(IGenericEntityAdapter genericEntityAdapter, ITableServiceClientFactory tableServiceClientFactory)
        {
            _genericEntityAdapter = genericEntityAdapter;
            _playerTableClient = tableServiceClientFactory.CreateTableClient("Player");
        }

        public IAsyncEnumerable<Player> GetAll(Guid? teamId)
        {
            return _genericEntityAdapter.AdaptAll(
                _playerTableClient.QueryAsync<GenericTableEntity<Player>>(gte => gte.Content.TeamId == teamId || teamId == null));
        }

        public async Task<Player> GetByNumber(Guid? teamId, int number)
        {
            return await _playerTableClient.SingleOrDefaultAsync<Player>(gte =>
                (gte.Content.TeamId == teamId || teamId == null) && gte.Content.Number == number);
        }

        public async Task AddPlayer(Player player)
        {
            var entity = _genericEntityAdapter.Adapt(player, player.TeamId, player.Number);
            await _playerTableClient.AddEntityAsync(entity);
        }

        public async Task UpdatePlayer(Guid teamId, int playerNumber, string playerName)
        {
            var player = await GetByNumber(teamId, playerNumber);
            player.Name = playerName;

            var entity = _genericEntityAdapter.Adapt(player, player.TeamId, player.Number);
            await _playerTableClient.UpdateEntityAsync(entity, entity.ETag);
        }
    }
}
