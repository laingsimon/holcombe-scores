using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDetailsDtoAdapter : IGameDetailsDtoAdapter
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IConfiguration _configuration;

        public GameDetailsDtoAdapter(IPlayerRepository playerRepository, IConfiguration configuration)
        {
            _playerRepository = playerRepository;
            _configuration = configuration;
        }

        public Game AdaptToGame(GameDetailsDto gameDetailsDto)
        {
            if (gameDetailsDto == null)
            {
                return null;
            }

            return new Game
            {
                Date = gameDetailsDto.Date ?? DateTime.Today,
                Id = Guid.NewGuid(),
                Opponent = gameDetailsDto.Opponent,
                PlayingAtHome = gameDetailsDto.PlayingAtHome,
                TeamId = gameDetailsDto.TeamId,
                Training = gameDetailsDto.Training,
                Address = gameDetailsDto.Address,
                Postponed = gameDetailsDto.Postponed,
                GoogleMapsApiKey = _configuration["GoogleMapsApiKey"],
            };
        }

        public Game AdaptToGame(ExistingGameDetailsDto gameDetailsDto)
        {
            if (gameDetailsDto == null)
            {
                return null;
            }

            var game = AdaptToGame((GameDetailsDto) gameDetailsDto);
            game.Id = gameDetailsDto.Id;

            return game;
        }

        public async IAsyncEnumerable<GamePlayer> AdaptSquad(GameDetailsDto gameDetailsDto, Guid gameId, List<Guid> missingPlayers)
        {
            var knownPlayersLookup = (await _playerRepository.GetAll(gameDetailsDto.TeamId).ToEnumerable()).ToDictionary(p => p.Id);

            foreach (var player in gameDetailsDto.PlayerIds)
            {
                if (!knownPlayersLookup.TryGetValue(player, out var knownPlayer))
                {
                    missingPlayers.Add(player);
                    continue;
                }

                yield return new GamePlayer
                {
                    PlayerId = knownPlayer.Id,
                    Number = knownPlayer.Number,
                    TeamId = knownPlayer.TeamId,
                    Name = knownPlayer.Name,
                    GameId = gameId,
                };
            }
        }
    }
}
