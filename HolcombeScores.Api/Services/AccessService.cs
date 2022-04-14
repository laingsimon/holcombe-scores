using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;
using HolcombeScores.Models;
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

        public AccessService(
            IAccessRepository accessRepository,
            IAccessRequestedDtoAdapter accessRequestedDtoAdapter,
            IAccessRequestDtoAdapter accessRequestDtoAdapter,
            IAccessDtoAdapter accessDtoAdapter,
            IHttpContextAccessor httpContextAccessor,
            IRecoverAccessDtoAdapter recoverAccessDtoAdapter,
            IMyAccessDtoAdapter myAccessDtoAdapter,
            IConfiguration configuration)
        {
            _accessRepository = accessRepository;
            _accessRequestedDtoAdapter = accessRequestedDtoAdapter;
            _accessRequestDtoAdapter = accessRequestDtoAdapter;
            _accessDtoAdapter = accessDtoAdapter;
            _httpContextAccessor = httpContextAccessor;
            _recoverAccessDtoAdapter = recoverAccessDtoAdapter;
            _adminPassCode = configuration["AdminPassCode"];
            _myAccessDtoAdapter = myAccessDtoAdapter;
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

            await foreach (var accessRequest in _accessRepository.GetAllAccessRequests())
            {
                var adapted = _recoverAccessDtoAdapter.Adapt(accessRequest);
                if (adapted.RecoveryId == recoverAccessDto.RecoveryId)
                {
                    return await RecoverAccess(accessRequest);
                }
            }


            return NotFound<AccessDto>("Access not found");
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
            var token = GetToken();
            if (token == null)
            {
                return null;
            }

            // TODO return a different object rather than the data object, expose IsAdmin and CanAccessTeam() and DefaultTeamId
            return await _accessRepository.GetAccess(token);
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
                return NotPermitted<AccessDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(response.UserId);
            if (accessRequest == null)
            {
                return NotFound<AccessDto>();
            }

            var existingAccess = await _accessRepository.GetAccess(response.UserId);
            if (existingAccess != null)
            {
                return Success<AccessDto>("Access already exists", _accessDtoAdapter.Adapt(existingAccess));
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

                return Success<AccessDto>("Access granted", _accessDtoAdapter.Adapt(newAccess));
            }

            return NotSuccess<AccessDto>("Access not granted");
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
                return new ActionResultDto<AccessDto>
                {
                    Errors =
                    {
                        "Not an admin",
                    }
                };
            }

            var access = await _accessRepository.GetAccess(userId);
            if (access == null)
            {
                return new ActionResultDto<AccessDto>
                {
                    Errors =
                    {
                        "Access not found",
                    }
                };
            }

            await _accessRepository.RemoveAccess(access.UserId);
            return new ActionResultDto<AccessDto>
            {
                Messages =
                {
                    "Access removed",
                },
                Success = true,
                Outcome = _accessDtoAdapter.Adapt(access),
            };
        }

        public async Task<ActionResultDto<AccessRequestDto>> RemoveAccessRequest(Guid userId)
        {
            if (!await IsAdmin())
            {
                return NotPermitted<AccessRequestDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(userId);
            if (accessRequest == null)
            {
                return NotFound<AccessRequestDto>("Access request not found");
            }

            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);
            return Success<AccessRequestDto>("Access request removed", _accessRequestDtoAdapter.Adapt(accessRequest));
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

        public async Task<ActionResultDto<AccessDto>> UpdateAccess(AccessDto updated)
        {
             var accessToUpdate = await GetAccess();

             if (accessToUpdate == null)
             {
                 return NotLoggedIn<AccessDto>();
             }

             if (updated.UserId != accessToUpdate.UserId)
             {
                 if (!access.Admin)
                 {
                     return NotPermitted<AccessDto>("Only an admin can change another users' details");
                 }

                 accessToUpdate = await _accessRepository.GetAccess(updated.UserId);

                 if (accessToUpdate == null)
                 {
                     return NotFound<AccessDto>(updated.UserId);
                 }

                 accessToUpdate.Admin = update.Admin;
                 accessToUpdate.TeamId = updated.TeamId;
             }

             accessToUpdate.Name = updated.Name;

             await _accessRepository.UpdateAccess(accessToUpdate);

             return Success<AccessDto>("Access updated");
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
            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessToken(access.Token, newToken);

            SetToken(newToken);

            return Success<AccessDto>("Access recovered", _accessDtoAdapter.Adapt(access));
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(AccessRequest accessRequest)
        {
            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessRequestToken(accessRequest.Token, newToken);

            var resultDto = new ActionResultDto<AccessDto>();
            SetToken(newToken);
            resultDto.Messages.Add("Cookie set");

            var existingAccess = await _accessRepository.GetAccess(accessRequest.UserId);
            if (existingAccess != null)
            {
                // access already granted/revoked don't overwrite
                resultDto.Warnings.Add("Access already exists");
                resultDto.Success = true; // technically not true, but access does exist
                resultDto.Outcome = _accessDtoAdapter.Adapt(existingAccess);
                return resultDto;
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

            resultDto.Success = true;
            resultDto.Messages.Add("Access recovered");
            resultDto.Outcome = _accessDtoAdapter.Adapt(newAccess);

            // clean up the access request
            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);
            resultDto.Messages.Add("Access request removed");

            return resultDto;
        }

        private static ActionResultDto<T> Success<T>(string message, T outcome = default)
        {
           return new ActionResultDto<T>
           {
               Messages =
               {
                   message,
               },
               Outcome = outcome,
               Success = true,
           };
        }

        private static ActionResultDto<T> NotSuccess<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Messages =
               {
                   message,
               }
           };
        }

        private static ActionResultDto<T> NotFound<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Warnings =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<T> NotPermitted<T>(string message)
        {
           return new ActionResultDto<T>
           {
               Errors =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<T> NotLoggedIn<T>()
        {
           return new ActionResultDto<T>
           {
               Warnings =
               {
                   "Not logged in",
               },
           };
        }
    }
}
