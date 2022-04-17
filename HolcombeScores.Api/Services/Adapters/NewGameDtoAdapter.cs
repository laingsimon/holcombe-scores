using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class NewGameDtoAdapter : INewGameDtoAdapter
    {
        private readonly IPlayerRepository _playerRepository;

        public NewGameDtoAdapter(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public Game AdaptToGame(NewGameDto newGameDto, ActionResultDto<GameDto> actionResult)
        {
            if (newGameDto == null)
            {
                return null;
            }

            return new Game
            {
                Date = newGameDto.Date ?? DateTime.Today,
                Id = Guid.NewGuid(),
                Opponent = newGameDto.Opponent,
                PlayingAtHome = newGameDto.PlayingAtHome,
            };
        }

        public async IAsyncEnumerable<GamePlayer> AdaptSquad(NewGameDto newGameDto, Guid gameId, ActionResultDto<GameDto> actionResult)
        {
            var knownPlayersLookup = new Dictionary<string, Player>();
            await foreach (var player in _playerRepository.GetAll(newGameDto.TeamId))
            {
                knownPlayersLookup.Add(player.Name, player);
            }

            foreach (var player in newGameDto.Players)
            {
                if (knownPlayersLookup.TryGetValue(player, out var knownPlayer))
                {
                    yield return new GamePlayer
                    {
                        Number = knownPlayer.Number,
                        TeamId = knownPlayer.TeamId,
                        Name = knownPlayer.Name,
                        GameId = gameId,
                    };
                    continue;
                }

                actionResult.Warnings.Add($"Player not found: `{player}`");
            }
        }
    }
}
