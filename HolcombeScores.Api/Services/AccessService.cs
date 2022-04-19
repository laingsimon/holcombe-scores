using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HolcombeScores.Api.Services
{
    public class AccessService : IAccessService
    {
        private const string CookieName = "HS_Token";

        private readonly IAccessRepository _accessRepository;
        private readonly IAccessRequestedDtoAdapter _accessRequestedDtoAdapter;
        private readonly IAccessRequestDtoAdapter _accessRequestDtoAdapter;
        private readonly IAccessDtoAdapter _accessDtoAdapter;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRecoverAccessDtoAdapter _recoverAccessDtoAdapter;
        private readonly string _adminPassCode;
        private readonly IMyAccessDtoAdapter _myAccessDtoAdapter;
        private readonly IServiceHelper _serviceHelper;

        public AccessService(
            IAccessRepository accessRepository,
            IAccessRequestedDtoAdapter accessRequestedDtoAdapter,
            IAccessRequestDtoAdapter accessRequestDtoAdapter,
            IAccessDtoAdapter accessDtoAdapter,
            IHttpContextAccessor httpContextAccessor,
            IRecoverAccessDtoAdapter recoverAccessDtoAdapter,
            IMyAccessDtoAdapter myAccessDtoAdapter,
            IConfiguration configuration,
            IServiceHelper serviceHelper)
        {
            _accessRepository = accessRepository;
            _accessRequestedDtoAdapter = accessRequestedDtoAdapter;
            _accessRequestDtoAdapter = accessRequestDtoAdapter;
            _accessDtoAdapter = accessDtoAdapter;
            _httpContextAccessor = httpContextAccessor;
            _recoverAccessDtoAdapter = recoverAccessDtoAdapter;
            _adminPassCode = configuration["AdminPassCode"];
            _myAccessDtoAdapter = myAccessDtoAdapter;
            _serviceHelper = serviceHelper;
        }

        public async Task<MyAccessDto> GetMyAccess()
        {
            var token = GetToken();
            var access = token == null ? null : await _accessRepository.GetAccess(token);
            var accessRequest = token == null ? null : await _accessRepository.GetAccessRequest(token);

            return _myAccessDtoAdapter.Adapt(access, accessRequest);
        }

        public async IAsyncEnumerable<RecoverAccessDto> GetAccessForRecovery()
        {
            await foreach (var access in _accessRepository.GetAllAccess())
            {
               yield return _recoverAccessDtoAdapter.Adapt(access);
            }

            await foreach (var accessRequest in _accessRepository.GetAllAccessRequests())
            {
               yield return _recoverAccessDtoAdapter.Adapt(accessRequest);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RecoverAccess(RecoverAccessDto recoverAccessDto, string adminPassCode)
        {
            if (_adminPassCode != adminPassCode)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Admin pass code mismatch");
            }

            await foreach (var access in _accessRepository.GetAllAccess())
            {
                var adapted = _recoverAccessDtoAdapter.Adapt(access);
                if (adapted.RecoveryId == recoverAccessDto.RecoveryId)
                {
                    return await RecoverAccess(access);
                }
            }

            await foreach (var accessRequest in _accessRepository.GetAllAccessRequests())
            {
                var adapted = _recoverAccessDtoAdapter.Adapt(accessRequest);
                if (adapted.RecoveryId == recoverAccessDto.RecoveryId)
                {
                    return await RecoverAccess(accessRequest);
                }
            }

            return _serviceHelper.NotFound<AccessDto>("Access not found");
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
        public async Task<AccessDto> GetAccess()
        {
            var token = GetToken();
            if (token == null)
            {
                return null;
            }

            return _accessDtoAdapter.Adapt(await _accessRepository.GetAccess(token));
        }

        public async Task<bool> IsAdmin()
        {
            var access = await GetAccess();
            if (access == null)
            {
                return false;
            }

            return access.Admin && access.Revoked == null;
        }

        public async Task<bool> CanAccessTeam(Guid teamId)
        {
            var access = await GetAccess();
            return (access.Admin || access.TeamId == teamId) && access.Revoked == null;
        }

        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto accessRequestDto)
        {
            var token = GetToken();
            if (token != null)
            {
                // access already requested
                var existingAccessRequest = await _accessRepository.GetAccessRequest(token);
                if (existingAccessRequest != null)
                {
                    return _accessRequestedDtoAdapter.Adapt(existingAccessRequest);
                }
            }

            var accessRequest = _accessRequestDtoAdapter.Adapt(accessRequestDto);
            accessRequest.UserId = Guid.NewGuid();
            accessRequest.Token = Guid.NewGuid().ToString();

            await _accessRepository.AddAccessRequest(accessRequest);

            SetToken(accessRequest.Token);

            return _accessRequestedDtoAdapter.Adapt(accessRequest);
        }

        public async Task<ActionResultDto<AccessDto>> RespondToRequest(AccessResponseDto response)
        {
            if (!await IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<AccessDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(response.UserId);
            if (accessRequest == null)
            {
                return _serviceHelper.NotFound<AccessDto>("Access request not found");
            }

            var existingAccess = await _accessRepository.GetAccess(response.UserId);
            if (existingAccess != null)
            {
                return _serviceHelper.Success("Access already exists", _accessDtoAdapter.Adapt(existingAccess));
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
                    Token = accessRequest.Token,
                };
                await _accessRepository.AddAccess(newAccess);

                // clean up the access request
                await _accessRepository.RemoveAccessRequest(response.UserId);

                return _serviceHelper.Success("Access granted", _accessDtoAdapter.Adapt(newAccess));
            }

            return _serviceHelper.NotSuccess<AccessDto>("Access request ignored");
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

        public async Task<ActionResultDto<AccessDto>> RemoveAccess(Guid userId)
        {
            if (!await IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<AccessDto>();
            }

            var access = await _accessRepository.GetAccess(userId);
            if (access == null)
            {
                return _serviceHelper.NotFound<AccessDto>("Access not found");
            }

            await _accessRepository.RemoveAccess(access.UserId);
            return _serviceHelper.Success("Access removed", _accessDtoAdapter.Adapt(access));
        }

        public async Task<ActionResultDto<AccessRequestDto>> RemoveAccessRequest(Guid userId)
        {
            if (!await IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<AccessRequestDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(userId);
            if (accessRequest == null)
            {
                return _serviceHelper.NotFound<AccessRequestDto>("Access request not found");
            }

            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);
            return _serviceHelper.Success("Access request removed", _accessRequestDtoAdapter.Adapt(accessRequest));
        }

        public async Task<ActionResultDto<AccessDto>> RevokeAccess(AccessResponseDto accessResponseDto)
        {
            if (!await IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<AccessDto>();
            }

            var access = await _accessRepository.GetAccess(accessResponseDto.UserId);
            if (access == null)
            {
                return _serviceHelper.NotFound<AccessDto>("Access not found");
            }

            if (access.Revoked != null)
            {
                return _serviceHelper.Success("Access already revoked", _accessDtoAdapter.Adapt(access));
            }

            access.Revoked = DateTime.UtcNow;
            access.RevokedReason = accessResponseDto.Reason;
            await _accessRepository.UpdateAccess(access);

            return _serviceHelper.Success("Access revoked", _accessDtoAdapter.Adapt(access));
        }

        public async Task<ActionResultDto<AccessDto>> UpdateAccess(AccessDto updated)
        {
             var accessToUpdate = await _accessRepository.GetAccess(GetToken());

             if (accessToUpdate == null)
             {
                 return _serviceHelper.NotLoggedIn<AccessDto>();
             }
             var isAdmin = accessToUpdate.Admin;

             if (accessToUpdate.Revoked != null)
             {
                 return _serviceHelper.NotPermitted<AccessDto>("Unable to update revoked access");
             }

             if (updated.UserId != accessToUpdate.UserId)
             {
                 if (!isAdmin)
                 {
                     return _serviceHelper.NotPermitted<AccessDto>("Only an admin can change another users' details");
                 }

                 accessToUpdate = await _accessRepository.GetAccess(updated.UserId);

                 if (accessToUpdate == null)
                 {
                     return _serviceHelper.NotFound<AccessDto>("Access not found");
                 }
             }

             if (isAdmin)
             {
                 accessToUpdate.Admin = updated.Admin;
                 accessToUpdate.TeamId = updated.TeamId;
             }

             accessToUpdate.Name = updated.Name;

             await _accessRepository.UpdateAccess(accessToUpdate);

             return _serviceHelper.Success("Access updated", _accessDtoAdapter.Adapt(accessToUpdate));
        }

        private string GetToken()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(CookieName, out var token))
            {
                return token;
            }

            return null;
        }

        private void SetToken(string token)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddYears(1),
            };
            response?.Cookies.Append(CookieName, token, options);
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(Access access)
        {
            if (access.Revoked != null)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Unable to recover revoked access");
            }

            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessToken(access.Token, newToken);

            SetToken(newToken);

            return _serviceHelper.Success("Access recovered", _accessDtoAdapter.Adapt(access));
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(AccessRequest accessRequest)
        {
            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessRequestToken(accessRequest.Token, newToken);

            SetToken(newToken);

            var existingAccess = await _accessRepository.GetAccess(accessRequest.UserId);
            if (existingAccess != null)
            {
                return _serviceHelper.Success("Access already exists", _accessDtoAdapter.Adapt(existingAccess));
            }

            var newAccess = new Access
            {
               Admin = true,
               Granted = DateTime.UtcNow,
               Name = accessRequest.Name,
               Revoked = null,
               RevokedReason = null,
               TeamId = accessRequest.TeamId,
               UserId = accessRequest.UserId,
               Token = accessRequest.Token,
            };
            await _accessRepository.AddAccess(newAccess);

            // clean up the access request
            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);

            return _serviceHelper.Success("Access request removed", _accessDtoAdapter.Adapt(newAccess));
        }
    }
}
