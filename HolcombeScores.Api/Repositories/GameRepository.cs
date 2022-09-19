using Azure;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Services;

namespace HolcombeScores.Api.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly TypedTableClient<Game> _gameTableClient;
        private readonly TypedTableClient<GamePlayer> _gamePlayerTableClient;
        private readonly TypedTableClient<Goal> _goalTableClient;

        public GameRepository(ITableClientFactory tableClientFactory)
        {
            _gameTableClient = new TypedTableClient<Game>(tableClientFactory.CreateTableClient("Game"));
            _gamePlayerTableClient = new TypedTableClient<GamePlayer>(tableClientFactory.CreateTableClient("GamePlayer"));
            _goalTableClient = new TypedTableClient<Goal>(tableClientFactory.CreateTableClient("Goal"));
        }

        public IAsyncEnumerable<Game> GetAll(Guid? teamId)
        {
            return teamId == null
                ? _gameTableClient.QueryAsync()
                : _gameTableClient.QueryAsync(g => g.TeamId == teamId);
        }

        public async Task<Game> Get(Guid id)
        {
            return await _gameTableClient.SingleOrDefaultAsync(gte => gte.Id == id);
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
            return await _gamePlayerTableClient.QueryAsync(g => g.GameId == gameId).ToEnumerable();
        }

        public async Task<ICollection<Goal>> GetGoals(Guid gameId)
        {
            return await _goalTableClient.QueryAsync(g => g.GameId == gameId).ToListAsync();
        }

        public async Task AddGamePlayer(GamePlayer gamePlayer)
        {
            gamePlayer.PartitionKey = gamePlayer.GameId.ToString();
            gamePlayer.RowKey = gamePlayer.Number.ToString();
            gamePlayer.ETag = ETag.All;

            await _gamePlayerTableClient.AddEntityAsync(gamePlayer);
        }

        public async Task AddGoal(Goal goal)
        {
            goal.GoalId = Guid.NewGuid();
            goal.PartitionKey = goal.GameId.ToString();
            goal.RowKey = goal.GoalId.ToString();
            goal.ETag = ETag.All;

            await _goalTableClient.AddEntityAsync(goal);
        }

        public async Task DeleteGame(Guid id)
        {
            var game = await Get(id);
            if (game == null)
            {
                return;
            }

            await _gameTableClient.DeleteEntityAsync(game.PartitionKey, game.RowKey);
        }

        public async Task DeleteGamePlayer(Guid gameId, Guid playerId)
        {
            var players = await GetPlayers(gameId);
            var player = players.SingleOrDefault(p => p.PlayerId == playerId);

            if (player == null)
            {
                return;
            }

            await _gamePlayerTableClient.DeleteEntityAsync(player.PartitionKey, player.RowKey);
        }

        public async Task DeleteGoal(Guid gameId, Guid goalId)
        {
            var goals = await GetGoals(gameId);
            var goal = goals.SingleOrDefault(g => g.RowKey == goalId.ToString());

            if (goal == null)
            {
                return;
            }

            await _goalTableClient.DeleteEntityAsync(goal.PartitionKey, goal.RowKey);
        }

        public async Task Update(Game game)
        {
            await _gameTableClient.UpdateEntityAsync(game, ETag.All);
        }
    }
}
