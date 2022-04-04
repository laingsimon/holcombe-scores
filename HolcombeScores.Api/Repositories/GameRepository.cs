using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly IGenericEntityAdapter _genericEntityAdapter;
        private readonly TableClient _gameTableClient;

        public GameRepository(IGenericEntityAdapter genericEntityAdapter, ITableServiceClientFactory tableServiceClientFactory)
        {
            _genericEntityAdapter = genericEntityAdapter;
            _gameTableClient = tableServiceClientFactory.CreateTableClient("Game");
        }

        public IAsyncEnumerable<Game> GetAll(Guid? teamId)
        {
            return _genericEntityAdapter.AdaptAll(_gameTableClient.QueryAsync<GenericTableEntity<Game>>(gte => gte.Content.TeamId == teamId || teamId == null));
        }

        public async Task<Game> Get(Guid id)
        {
            return await _gameTableClient.SingleOrDefaultAsync<Game>(gte => gte.Content.Id == id);
        }

        public async Task Add(Game game)
        {
            var genericEntity = _genericEntityAdapter.Adapt(game, game.TeamId, game.Id);
            await _gameTableClient.AddEntityAsync(genericEntity);
        }
    }
}
