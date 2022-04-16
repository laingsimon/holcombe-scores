using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly TableClient _gameTableClient;
        private readonly TableClient _gamePlayerTableClient;

        public GameRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _gameTableClient = tableServiceClientFactory.CreateTableClient("Game");
            _gamePlayerTableClient = tableServiceClientFactory.CreateTableClient("GamePlayer"); 
        }

        public IAsyncEnumerable<Game> GetAll(Guid? teamId)
        {
            if (teamId == null)
            {
                return _gameTableClient.QueryAsync<Game>();
            }
            return _gameTableClient.QueryAsync<Game>(g => g.TeamId == teamId);
        }

        public async Task<Game> Get(Guid id)
        {
            return await _gameTableClient.SingleOrDefaultAsync<Game>(gte => gte.Id == id);
        }

        public async Task Add(Game game)
        {
            game.Timestamp = DateTimeOffset.UtcNow;
            game.PartitionKey = game.TeamId.ToString();
            game.RowKey = game.Id.ToString();
            game.ETag = ETag.All;

            await _gameTableClient.AddEntityAsync(game);
        }

        public async Task<IEnumerable<GamePlayer>> GetPlayers(Guid gameId)
        {
            var players = new List<GamePlayer>();
            await foreach (var gamePlayer in _gameTableClient.QueryAsync<Game>(g => g.Id == gameId))
            {
                players.Add(gamePlayer);
            }

            return players;
        }

        public async Task AddGamePlayer(GamePlayer gamePlayer)
        {
            gamePlayer.PartitionKey = gamePlayer.GameId.ToString();
            gamePlayer.RowKey = gamePlayer.Number.ToString();
            gamePlayer.ETag = ETag.All;

            await _gameTableClient.AddEntityAsync(gamePlayer);
        }
    }
}
