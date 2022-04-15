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
                return CannotAccessTeam();
            }

            var existingPlayer = await _playerRepository.GetByNumber(player.TeamId, player.Number);

            if (existingPlayer == null)
            {
                await _playerRepository.AddPlayer(player);
                return Success("Player created", await GetPlayerDto(player.TeamId, player.Number));
            }

            await _playerRepository.UpdatePlayer(player.TeamId, player.Number, player.Name);
            return Success("Player updated", await GetPlayerDto(player.TeamId, player.Number));
        }

        public async Task<ActionResultDto<PlayerDto>> DeletePlayer(PlayerDto playerDto)
        {
            var player = _playerDtoAdapter.Adapt(playerDto);

            if (!await _accessService.CanAccessTeam(playerDto.TeamId))
            {
                return CannotAccessTeam();
            }

            var existingPlayer = await _playerRepository.GetByNumber(player.TeamId, player.Number);

            if (existingPlayer == null)
            {
                return NotFound("Player not found");
            }

            await _playerRepository.DeletePlayer(existingPlayer.TeamId, existingPlayer.Number);

            return Success("Player deleted", _playerDtoAdapter.Adapt(existingPlayer));
        }

        public async Task<ActionResultDto<PlayerDto>> TransferPlayer(TransferPlayerDto transferDto)
        {
            if (!await _accessService.CanAccessTeam(transferDto.CurrentTeamId))
            {
                return CannotAccessTeam();
            }

            var newTeam = await _teamRepository.Get(transferDto.NewTeamId);
            if (newTeam == null)
            {
                return NotFound("New team not found");
            }

            var playerToTransfer = await _playerRepository.GetByNumber(transferDto.CurrentTeamId, transferDto.CurrentNumber);
            if (playerToTransfer == null)
            {
                return NotFound("Player not found");
            }

            var newPlayer = await _playerRepository.GetByNumber(transferDto.CurrentTeamId, transferDto.CurrentNumber);
            newPlayer.Number = transferDto.NewNumber ?? transferDto.CurrentNumber;
            newPlayer.TeamId = transferDto.NewTeamId;

            if (await _playerRepository.GetByNumber(transferDto.NewTeamId, newPlayer.Number) != null)
            {
                return NotSuccess($"Player already exists with this number in team {newTeam.Name}");
            }

            await _playerRepository.AddPlayer(newPlayer);
            await _playerRepository.DeletePlayer(transferDto.CurrentTeamId, transferDto.CurrentNumber);

            return Success($"Player transferred to {newTeam.Name}", _playerDtoAdapter.Adapt(newPlayer));
        }

        private async Task<PlayerDto> GetPlayerDto(Guid teamId, int number)
        {
            var existingPlayer = await _playerRepository.GetByNumber(teamId, number);
            return _playerDtoAdapter.Adapt(existingPlayer);
        }

        private static ActionResultDto<PlayerDto> CannotAccessTeam()
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

        private static ActionResultDto<PlayerDto> NotSuccess (string message)
        {
            return new ActionResultDto<PlayerDto>
            {
                Success = false,
                Errors =
                {
                    message,
                },
            };
        }


        private static ActionResultDto<PlayerDto> NotFound(string message)
        {
            return new ActionResultDto<PlayerDto>
            {
                Success = false,
                Errors =
                {
                    message,
                },
            };
        }

        private static ActionResultDto<PlayerDto> Success(string message, PlayerDto outcome = null)
        {
            return new ActionResultDto<PlayerDto>
            {
                Success = true,
                Messages =
                {
                    message,
                },
                Outcome = outcome,
            };
        }
    }
}
