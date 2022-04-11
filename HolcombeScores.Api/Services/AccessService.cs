using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;
using HolcombeScores.Models;
using Microsoft.AspNetCore.Http;

namespace HolcombeScores.Api.Services
{
    public class AccessService : IAccessService
    {
        private const string CookieName = "HS_UserId";

        private readonly IAccessRepository _accessRepository;
        private readonly IAccessRequestedDtoAdapter _accessRequestedDtoAdapter;
        private readonly IAccessRequestDtoAdapter _accessRequestDtoAdapter;
        private readonly IAccessDtoAdapter _accessDtoAdapter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRecoverAccessDtoAdapter _recoverAccessDtoAdapter;
        private readonly string _adminPassCode;

        public AccessService(
            IAccessRepository accessRepository,
            IAccessRequestedDtoAdapter accessRequestedDtoAdapter,
            IAccessRequestDtoAdapter accessRequestDtoAdapter,
            IAccessDtoAdapter accessDtoAdapter,
            IHttpContextAccessor httpContextAccessor,
            IRecoverAccessDtoAdapter recoverAccessDtoAdapter)
        {
            _accessRepository = accessRepository;
            _accessRequestedDtoAdapter = accessRequestedDtoAdapter;
            _accessRequestDtoAdapter = accessRequestDtoAdapter;
            _accessDtoAdapter = accessDtoAdapter;
            _httpContextAccessor = httpContextAccessor;
            _recoverAccessDtoAdapter = recoverAccessDtoAdapter;
            _adminPassCode = "test";
        }

        public async Task<MyAccessDto> GetMyAccess()
        {
            var userId = GetUserId();
            var access = userId == null ? null : await GetAccess();
            var accessRequest = userId == null ? null : await _accessRepository.GetAccessRequest(userId.Value);

            return new MyAccessDto
            {
                UserId = userId,
                Access = access == null ? null : _accessDtoAdapter.Adapt(access),
                Request = accessRequest == null ? null : _accessRequestDtoAdapter.Adapt(accessRequest),
            };
        }

        public async IAsyncEnumerable<RecoverAccessDto> GetAccessForRecovery()
        {
            await foreach (var access in _accessRepository.GetAllAccess())
            {
               yield return _recoverAccessDtoAdapter.Adapt(access);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RecoverAccess(RecoverAccessDto recoverAccessDto, string adminPassCode)
        {
            if (_adminPassCode != adminPassCode)
            {
                return new ActionResultDto<AccessDto>
                {
                    Warnings = 
                    {
                        "Admin pass code mismatch"
                    }
                };
            }

            await foreach (var access in _accessRepository.GetAllAccess())
            {
                var adapted = _recoverAccessDtoAdapter.Adapt(access);
                if (adapted.RecoveryId == recoverAccessDto.RecoveryId)
                {
                    return await RecoverAccess(access);
                }
            }

            return new ActionResultDto<AccessDto>
            {
                Warnings = 
                {
                   "Access not found"
                }
            };
        }

        public async IAsyncEnumerable<AccessDto> GetAllAccess()
        {
            if (!await IsAdmin())
            {
                yield break;
            }

            await foreach (var access in _accessRepository.GetAllAccess())
            {
                yield return _accessDtoAdapter.Adapt(access);
            }
        }

        /// <summary>
        /// Get the access for the current HTTP request, return null if no access
        /// </summary>
        /// <returns></returns>
        public async Task<Access> GetAccess()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return null;
            }

            // TODO return a different object rather than the data object, expose IsAdmin and CanAccessTeam() and DefaultTeamId
            return await _accessRepository.GetAccess(userId.Value);
        }

        public async Task<bool> IsAdmin()
        {
            var access = await GetAccess();
            return access?.Admin ?? false;
        }

        public async Task<bool> CanAccessTeam(Guid teamId)
        {
            var access = await GetAccess();
            return access.Admin || access.TeamId == teamId;
        }

        public Guid? GetUserId()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(CookieName, out var userIdString))
            {
                return Guid.TryParse(userIdString, out var userId)
                    ? userId
                    : null;
            }

            return null;
        }

        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto accessRequestDto)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                // access already requested
                var existingAccessRequest = await _accessRepository.GetAccessRequest(userId.Value);
                if (existingAccessRequest != null)
                {
                    return _accessRequestedDtoAdapter.Adapt(existingAccessRequest);
                }
            }

            var accessRequest = _accessRequestDtoAdapter.Adapt(accessRequestDto);
            accessRequest.UserId = Guid.NewGuid();

            await _accessRepository.AddAccessRequest(accessRequest);
            return _accessRequestedDtoAdapter.Adapt(accessRequest);
        }

        public async Task<ActionResultDto<AccessDto>> RespondToRequest(AccessResponseDto response)
        {
            var resultDto = new ActionResultDto<AccessDto>();

            if (!await IsAdmin())
            {
                // not an admin, not permitted to respond to requests
                resultDto.Errors.Add("Not an admin");
                return resultDto;
            }

            var accessRequest = await _accessRepository.GetAccessRequest(response.UserId);
            if (accessRequest == null)
            {
                // access request doesn't exist, don't grant access
                resultDto.Errors.Add("Access request not found");
                return resultDto;
            }

            var existingAccess = await _accessRepository.GetAccess(response.UserId);
            if (existingAccess != null)
            {
                // access already granted/revoked don't overwrite
                resultDto.Warnings.Add("Access already exists");
                resultDto.Success = true; // technically not true, but access does exist
                resultDto.Outcome = _accessDtoAdapter.Adapt(existingAccess);
                return resultDto;
            }

            if (response.Allow)
            {
                var newAccess = new Access
                {
                    Admin = true,
                    Granted = DateTime.UtcNow,
                    Name = accessRequest.Name,
                    Revoked = null,
                    RevokedReason = null,
                    TeamId = response.TeamId,
                    UserId = response.UserId,
                };
                await _accessRepository.AddAccess(newAccess);

                resultDto.Success = true;
                resultDto.Messages.Add("Access granted");
                resultDto.Outcome = _accessDtoAdapter.Adapt(newAccess);

                // clean up the access request
                await _accessRepository.RemoveAccessRequest(response.UserId);

                SetUserId(response.UserId);

                return resultDto;
            }

            resultDto.Messages.Add("Access not granted");
            return resultDto;
        }

        public async IAsyncEnumerable<AccessRequestDto> GetAccessRequests()
        {
            if (!await IsAdmin())
            {
                yield break;
            }

            await foreach (var item in _accessRepository.GetAllAccessRequests())
            {
                yield return _accessRequestDtoAdapter.Adapt(item);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RevokeAccess(AccessResponseDto accessResponseDto)
        {
            var resultDto = new ActionResultDto<AccessDto>();
            if (!await IsAdmin())
            {
                resultDto.Errors.Add("Not an admin");
                return resultDto;
            }

            var access = await _accessRepository.GetAccess(accessResponseDto.UserId);
            if (access == null)
            {
                resultDto.Errors.Add("Access not found");
                return resultDto;
            }

            if (access.Revoked != null)
            {
                resultDto.Warnings.Add("Access already revoked");
                resultDto.Outcome = _accessDtoAdapter.Adapt(access);
                resultDto.Success = true;
                return resultDto;
            }

            access.Revoked = DateTime.UtcNow;
            access.RevokedReason = accessResponseDto.Reason;
            await _accessRepository.UpdateAccess(access);

            resultDto.Success = true;
            resultDto.Messages.Add("Access revoked");
            resultDto.Outcome = _accessDtoAdapter.Adapt(access);
            return resultDto;
        }

        private void SetUserId(Guid teamId)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Append(CookieName, teamId.ToString());
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(Access access)
        {
            var oldId = access.UserId;
            await _accessRepository.DeleteAccess(access);
            access.UserId = Guid.NewGuid();

            await _accessRepository.AddAccess(access);
            SetUserId(access.UserId);

            return new ActionResultDto<AccessDto>
            {
                Outcome = _accessDtoAdapter.Adapt(access),
                Success = true,
                Messages = 
                {
                    $"User superseded: {oldId}",
                }
            };
        }
    }
}
