﻿using System;
using System.Collections.Generic;
using System.Linq;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services.Adapters
{
    public class GameDetailsDetailsDtoAdapter : IGameDetailsDtoAdapter
    {
        private readonly IPlayerRepository _playerRepository;

        public GameDetailsDetailsDtoAdapter(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
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

        public async IAsyncEnumerable<GamePlayer> AdaptSquad(GameDetailsDto gameDetailsDto, Guid gameId, List<string> missingPlayers)
        {
            var knownPlayersLookup = (await _playerRepository.GetAll(gameDetailsDto.TeamId).ToEnumerable()).ToDictionary(p => p.Name);

            foreach (var player in gameDetailsDto.Players)
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