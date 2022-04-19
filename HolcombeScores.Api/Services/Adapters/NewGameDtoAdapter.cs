using System;
using System.Collections.Generic;
using System.Linq;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services.Adapters
{
    public class NewGameDtoAdapter : INewGameDtoAdapter
    {
        private readonly IPlayerRepository _playerRepository;

        public NewGameDtoAdapter(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public Game AdaptToGame(NewGameDto newGameDto)
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
                TeamId = newGameDto.TeamId,
            };
        }

        public async IAsyncEnumerable<GamePlayer> AdaptSquad(NewGameDto newGameDto, Guid gameId, List<string> missingPlayers)
        {
            var knownPlayersLookup = (await _playerRepository.GetAll(newGameDto.TeamId).ToEnumerable()).ToDictionary(p => p.Name);

            foreach (var player in newGameDto.Players)
            {
                if (!knownPlayersLookup.TryGetValue(player, out var knownPlayer))
                {
                    missingPlayers.Add(player);
                    continue;
                }

                yield return new GamePlayer
                {
                    Number = knownPlayer.Number,
                    TeamId = knownPlayer.TeamId,
                    Name = knownPlayer.Name,
                    GameId = gameId,
                };
            }
        }
    }
}
