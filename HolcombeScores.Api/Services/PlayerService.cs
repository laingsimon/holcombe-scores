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
        private readonly ITeamRepository _teamRepository;

        public PlayerService(
            IPlayerRepository playerRepository,
            IPlayerDtoAdapter playerDtoAdapter,
            IAccessService accessService,
            ITeamRepository teamRepository)
        {
            _playerRepository = playerRepository;
            _playerDtoAdapter = playerDtoAdapter;
            _accessService = accessService;
            _teamRepository = teamRepository;
        }

        public async IAsyncEnumerable<PlayerDto> GetAllPlayers()
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
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

        public async Task<ActionResultDto<PlayerDto>> DeletePlayer(PlayerDto playerDto)
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
                return new ActionResultDto<PlayerDto>
                {
                    Success = false,
                    Warnings =
                    {
                        "Player not found"
                    },
                };
            }

            await _playerRepository.DeletePlayer(existingPlayer.TeamId, existingPlayer.Number);

            return new ActionResultDto<PlayerDto>
            {
                Success = true,
                Warnings =
                {
                    "Player deleted"
                },
                Outcome = _playerDtoAdapter.Adapt(existingPlayer),
            };
        }

        public async Task<ActionResultDto<PlayerDto>> TransferPlayer(TransferPlayerDto transferDto)
        {
            if (!await _accessService.CanAccessTeam(transferDto.CurrentTeamId))
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

            var newTeam = await _teamRepository.Get(transferDto.NewTeamId);
            if (newTeam == null)
            {
                return new ActionResultDto<PlayerDto>
                {
                    Success = false,
                    Errors =
                    {
                        "New team not found"
                    },
                };
            }

            var playerToTransfer = await _playerRepository.GetByNumber(transferDto.CurrentTeamId, transferDto.CurrentNumber);

            if (playerToTransfer == null)
            {
                return new ActionResultDto<PlayerDto>
                {
                    Success = false,
                    Warnings =
                    {
                        "Player not found",
                    },
                };
            }

            var newPlayer = _playerDtoAdapter.Adapt(playerToTransfer);
            newPlayer.Number = transferDto.NewNumber ?? transferDto.CurrentNumber;
            newPlayer.TeamId = transferDto.NewTeamId;
            var transferredPlayer = await _playerRepository.GetByNumber(transferDto.NewTeamId, newPlayer.Number);

            if (transferredPlayer == null)
            {
                return new ActionResultDto<PlayerDto>
                {
                    Success = false,
                    Warnings =
                    {
                        $"Player already exists with this number in team {newTeam.Name}",
                    },
                };
            }

            await _playerRepository.AddPlayer(newPlayer);
            await _playerRepository.DeletePlayer(transferDto.CurrentTeamId, transferDto.CurrentNumber);

            return new ActionResultDto<PlayerDto>
            {
                Success = true,
                Warnings =
                {
                    $"Player transferred to {newTeam.Name}",
                },
                Outcome = _playerDtoAdapter.Adapt(newPlayer),
            };
        }

        private async Task<PlayerDto> GetPlayerDto(Guid teamId, int number)
        {
            var existingPlayer = await _playerRepository.GetByNumber(teamId, number);
            return _playerDtoAdapter.Adapt(existingPlayer);
        }
    }
}
