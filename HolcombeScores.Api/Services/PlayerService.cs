using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerDtoAdapter _playerDtoAdapter;
        private readonly IAccessService _accessService;

        public PlayerService(IPlayerRepository playerRepository, IPlayerDtoAdapter playerDtoAdapter, IAccessService accessService)
        {
            _playerRepository = playerRepository;
            _playerDtoAdapter = playerDtoAdapter;
            _accessService = accessService;
        }

        public async IAsyncEnumerable<PlayerDto> GetAllPlayers()
        {
            var access = await _accessService.GetAccess();
            if (access == null)
            {
                yield break;
            }

            await foreach (var player in _playerRepository.GetAll(access.Admin ? null : access.TeamId))
            {
                yield return _playerDtoAdapter.Adapt(player);
            }
        }

        public async Task<ActionResultDto<PlayerDto>> CreateOrUpdatePlayer(PlayerDto playerDto)
        {
            var player = _playerDtoAdapter.Adapt(playerDto);

            if (!await _accessService.CanAccessTeam(playerDto.TeamId))
            {
                return new ActionResultDto<PlayerDto>
                {
                    Success = false,
                    Errors =
                    {
                        "Cannot access team"
                    },
                };
            }

            var existingPlayer = await _playerRepository.GetByNumber(player.TeamId, player.Number);

            if (existingPlayer == null)
            {
                await _playerRepository.AddPlayer(player);
                return new ActionResultDto<PlayerDto>
                {
                    Success = true,
                    Messages =
                    {
                        "Player created"
                    },
                    Outcome = await GetPlayerDto(player.TeamId, player.Number),
                };
            }

            await _playerRepository.UpdatePlayer(player.TeamId, player.Number, player.Name);
            return new ActionResultDto<PlayerDto>
            {
                Success = true,
                Messages =
                {
                    "Player updated"
                },
                Outcome = await GetPlayerDto(player.TeamId, player.Number),
            };
        }

        private async Task<PlayerDto> GetPlayerDto(Guid teamId, int number)
        {
            var existingPlayer = await _playerRepository.GetByNumber(teamId, number);
            return _playerDtoAdapter.Adapt(existingPlayer);
        }
    }
}
