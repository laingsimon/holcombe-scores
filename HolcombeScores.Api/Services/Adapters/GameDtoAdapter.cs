using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDtoAdapter : IGameDtoAdapter
    {
        private readonly IGoalDtoAdapter _goalAdapter;
        private readonly IGamePlayerDtoAdapter _gamePlayerAdapter;
        private readonly IConfiguration _configuration;

        public GameDtoAdapter(IGoalDtoAdapter goalAdapter, IGamePlayerDtoAdapter gamePlayerAdapter, IConfiguration configuration)
        {
            _goalAdapter = goalAdapter;
            _gamePlayerAdapter = gamePlayerAdapter;
            _configuration = configuration;
        }

        public async Task<GameDto> Adapt(Game game, IEnumerable<GamePlayer> squad, IEnumerable<Goal> goals, AdapterContext context)
        {
            if (game == null)
            {
                return null;
            }

            return new GameDto
            {
                TeamId = game.TeamId,
                Date = game.Date,
                Goals = (await goals.SelectAsync(g => _goalAdapter.Adapt(g))).ToArray(),
                Id = game.Id,
                Opponent = game.Opponent,
                Squad = squad.Select(_gamePlayerAdapter.Adapt).ToArray(),
                PlayingAtHome = game.PlayingAtHome,
                ReadOnly = context.ReadOnly,
                Playable = context.Playable,
                Training = game.Training,
                RecordGoalToken = context.RecordGoalToken,
                Started = context.HasStarted,
                Address = game.PlayingAtHome ? _configuration["HOME_ADDRESS"] : game.Address,
                Postponed = game.Postponed,
                GoogleMapsApiKey = _configuration["GOOGLE_MAPS_API_KEY"],
                HasStarted = context.HasStarted,
                Friendly = game.Friendly,
            };
        }

        public Game Adapt(GameDto game)
        {
            if (game == null)
            {
                return null;
            }

            return new Game
            {
                Date = game.Date,
                Id = game.Id,
                Opponent = game.Opponent,
                PlayingAtHome = game.PlayingAtHome,
                TeamId = game.TeamId,
                Training = game.Training,
                Address = game.Address,
                Postponed = game.Postponed,
                Friendly = game.Friendly,
            };
        }

        public class AdapterContext
        {
            public string RecordGoalToken { get; }
            public bool Playable { get; }
            public bool ReadOnly { get; }
            public bool HasStarted { get; }

            public AdapterContext(string recordGoalToken, bool playable, bool readOnly, bool hasStarted)
            {
                RecordGoalToken = recordGoalToken;
                Playable = playable;
                ReadOnly = readOnly;
                HasStarted = hasStarted;
            }
        }
    }
}
