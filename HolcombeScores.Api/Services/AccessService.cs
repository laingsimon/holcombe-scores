using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class AccessService : IAccessService
    {
        private const string TokenCookieName = "HS_Token";
        private const string UserIdCookieName = "HS_User";
        private const string ImpersonatedByTokenCookieName = "HS_ImpersonatedByToken";
        private const string ImpersonatedByUserIdCookieName = "HS_ImpersonatedByUserId";

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
            var access = await GetAccessInternal(permitRevoked: true);
            var accessRequest = await GetAccessRequestInternal();

            return _myAccessDtoAdapter.Adapt(access, accessRequest, await GetImpersonatedByAccess());
        }

        public async Task<ActionResultDto<MyAccessDto>> Impersonate(ImpersonationDto impersonation)
        {
            if (_adminPassCode != impersonation.AdminPassCode)
            {
                return _serviceHelper.NotPermitted<MyAccessDto>("Admin passcode mismatch");
            }

            if (!(await IsAdmin()))
            {
                return _serviceHelper.NotAnAdmin<MyAccessDto>();
            }

            var existingAccess = await _accessRepository.GetAccess(impersonation.ImpersonateUserId);
            if (existingAccess == null)
            {
                return _serviceHelper.NotSuccess("Access not found");
            }
            
            SetImpersonatedByCookies(existingAccess.Token, existingAccess.UserId);

            var access = await GetAccessInternal(permitRevoked: true);
            var accessRequest = await GetAccessRequestInternal();
            var myAccess = _myAccessDtoAdapter.Adapt(access, accessRequest);
            return _serviceHelper.Success<MyAccessDto>("Impersonation complete", myAccess);
        }

        public async Task<MyAccessDto> Unimpersonate()
        {
            RemoveImpersonationCookies();

            var access = await GetAccessInternal(permitRevoked: true);
            var accessRequest = await GetAccessRequestInternal();
            var myAccess = _myAccessDtoAdapter.Adapt(access, accessRequest, existingAccess);
            return _serviceHelper.Success<MyAccessDto>("Impersonation complete", myAccess);
        }

        public async IAsyncEnumerable<RecoverAccessDto> GetAccessForRecovery()
        {
            var userId = GetRequestUserId();

            await foreach (var access in _accessRepository.GetAllAccess())
            {
                if (userId != null && access.UserId != userId)
                {
                    // if the userId cookie was set, then only show the users access requests
                    continue;
                }

                yield return _recoverAccessDtoAdapter.Adapt(access);
            }

            await foreach (var accessRequest in _accessRepository.GetAllAccessRequests())
            {
                if (userId != null && accessRequest.UserId != userId)
                {
                    // if the userId cookie was set, then only show the users access
                    continue;
                }

                yield return _recoverAccessDtoAdapter.Adapt(accessRequest);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RecoverAccess(RecoverAccessDto recoverAccessDto)
        {
            if (_adminPassCode != recoverAccessDto.AdminPassCode)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Admin passcode mismatch");
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
            var myAccess = await GetAccess();
            if (!myAccess.Admin && !myAccess.Manager)
            {
                yield break;
            }

            await foreach (var access in _accessRepository.GetAllAccess(myAccess.Admin ? null : myAccess.TeamId))
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
            return _accessDtoAdapter.Adapt(await GetAccessInternal());
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

        public async Task<bool> IsManagerOrAdmin()
        {
            var access = await GetAccess();
            if (access == null)
            {
                return false;
            }

            return (access.Manager || access.Admin) && access.Revoked == null;
        }

        public async Task<bool> CanAccessTeam(Guid teamId)
        {
            var access = await GetAccess();
            return (access.Admin || access.TeamId == teamId) && access.Revoked == null;
        }

        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto accessRequestDto)
        {
            var existingAccessRequest = await GetAccessRequestInternal();
            if (existingAccessRequest != null)
            {
                // access already requested
                return _accessRequestedDtoAdapter.Adapt(existingAccessRequest);
            }

            var accessRequest = _accessRequestDtoAdapter.Adapt(accessRequestDto);
            accessRequest.UserId = Guid.NewGuid();
            accessRequest.Token = Guid.NewGuid().ToString();

            await _accessRepository.AddAccessRequest(accessRequest);

            SetCookies(accessRequest.Token, accessRequest.UserId);

            return _accessRequestedDtoAdapter.Adapt(accessRequest);
        }

        public async Task<ActionResultDto<AccessDto>> RespondToRequest(AccessResponseDto response)
        {
            if (!await IsManagerOrAdmin())
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
                    Admin = false,
                    Manager = false,
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

            accessRequest.Reason = response.Reason;
            accessRequest.Rejected = true;
            await _accessRepository.UpdateAccessRequest(accessRequest);
            return _serviceHelper.Success<AccessDto>("Access request ignored");
        }

        public async IAsyncEnumerable<AccessRequestDto> GetAccessRequests()
        {
            var myAccess = await GetAccess();
            if (!myAccess.Admin && !myAccess.Manager)
            {
                yield break;
            }

            await foreach (var item in _accessRepository.GetAllAccessRequests(myAccess.Admin ? null : myAccess.TeamId))
            {
                yield return _accessRequestDtoAdapter.Adapt(item);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RemoveAccess(Guid userId)
        {
            var myAccess = await GetAccessInternal();
            if (!myAccess.Admin && !myAccess.Manager && myAccess.UserId != userId)
            {
                return _serviceHelper.NotAnAdmin<AccessDto>();
            }

            var access = await _accessRepository.GetAccess(userId);
            if (access == null)
            {
                return _serviceHelper.NotFound<AccessDto>("Access not found");
            }

            if (myAccess.Manager && access.TeamId != myAccess.TeamId)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Only admins can delete access for another team");
            }

            await _accessRepository.RemoveAccess(access.UserId);
            return _serviceHelper.Success("Access removed", _accessDtoAdapter.Adapt(access));
        }

        public async Task<ActionResultDto<AccessRequestDto>> RemoveAccessRequest(Guid userId)
        {
            var myAccess = await GetAccessInternal();
            if (!myAccess.Admin && !myAccess.Manager)
            {
                return _serviceHelper.NotAnAdmin<AccessRequestDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(userId);
            if (accessRequest == null)
            {
                return _serviceHelper.NotFound<AccessRequestDto>("Access request not found");
            }

            if (myAccess.Manager && accessRequest.TeamId != myAccess.TeamId)
            {
                return _serviceHelper.NotPermitted<AccessRequestDto>("Only admins can delete requests for another team");
            }

            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);
            return _serviceHelper.Success("Access request removed", _accessRequestDtoAdapter.Adapt(accessRequest));
        }

        public async Task<ActionResultDto<AccessDto>> RevokeAccess(AccessResponseDto accessResponseDto)
        {
            var myAccess = await GetAccessInternal();
            if (!myAccess.Admin && !myAccess.Manager)
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

            if (myAccess.Manager && access.TeamId != myAccess.TeamId)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Only admins can revoke requests for another team");
            }

            access.Revoked = DateTime.UtcNow;
            access.RevokedReason = accessResponseDto.Reason;
            await _accessRepository.UpdateAccess(access);

            return _serviceHelper.Success("Access revoked", _accessDtoAdapter.Adapt(access));
        }

        public async Task<ActionResultDto<AccessDto>> UpdateAccess(AccessDto updated)
        {
            var myAccess = await GetAccessInternal();

            if (myAccess == null)
            {
                return _serviceHelper.NotLoggedIn<AccessDto>();
            }

            var isAdmin = myAccess.Admin;
            var isManager = myAccess.Manager;
            var accessToUpdate = myAccess;

            if (updated.UserId != myAccess.UserId)
            {
                if (!isAdmin && !isManager)
                {
                    return _serviceHelper.NotPermitted<AccessDto>("Only a managers and admins can change another users' details");
                }

                accessToUpdate = await _accessRepository.GetAccess(updated.UserId);

                if (accessToUpdate == null)
                {
                    return _serviceHelper.NotFound<AccessDto>($"Access not found for user ${updated.UserId}");
                }

                if (!isAdmin && accessToUpdate.TeamId != myAccess.TeamId)
                {
                    return _serviceHelper.NotPermitted<AccessDto>("Only a admins can change access details for another team");
                }
            }

            if (accessToUpdate.Revoked != null)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Unable to update revoked access");
            }

            if (isManager || isAdmin)
            {
                accessToUpdate.Manager = updated.Manager;
                accessToUpdate.TeamId = updated.TeamId;
            }

            if (isAdmin)
            {
                accessToUpdate.Admin = updated.Admin;
            }

            accessToUpdate.Name = updated.Name;

            await _accessRepository.UpdateAccess(accessToUpdate);

            return _serviceHelper.Success("Access updated", _accessDtoAdapter.Adapt(accessToUpdate));
        }

        public Task<ActionResultDto<string>> Logout()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response.Cookies.Delete(TokenCookieName);
            response.Cookies.Delete(UserIdCookieName);

            return Task.FromResult(_serviceHelper.Success("Logged out", "Cookies removed, access can be recovered"));
        }

        private string GetRequestToken()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(TokenCookieName, out var token))
            {
                return token;
            }

            return null;
        }

        private string GetImpersonatedByToken()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(ImpersonatedByTokenCookieName, out var token))
            {
                return token;
            }

            return null;
        }

        private Guid? GetRequestUserId()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(UserIdCookieName, out var userIdString))
            {
                return Guid.TryParse(userIdString, out var userId)
                    ? userId
                    : null;
            }

            return null;
        }

        private Guid? GetImpersonatedByUserId()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(ImpersonatedByUserIdCookieName, out var userIdString))
            {
                return Guid.TryParse(userIdString, out var userId)
                    ? userId
                    : null;
            }

            return null;
        }

        private void SetCookies(string token, Guid userId)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            var options = new CookieOptions
            {
                Secure = true,
                Expires = DateTime.UtcNow.AddYears(1),
                SameSite = SameSiteMode.None,
            };
            response?.Cookies.Append(TokenCookieName, token, options);
            response?.Cookies.Append(UserIdCookieName, userId.ToString(), options);
        }

        private void SetImpersonatedByCookies(string token, Guid userId)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            var options = new CookieOptions
            {
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7),
                SameSite = SameSiteMode.None,
            };
            response?.Cookies.Append(ImpersonatedByTokenCookieName, token, options);
            response?.Cookies.Append(ImpersonatedByUserIdCookieName, userId.ToString(), options);
        }

        private void RemoveImpersonationCookies()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Delete(ImpersonatedByTokenCookieName);
            response?.Cookies.Delete(ImpersonatedByUserIdCookieName);
        }

        private async Task<Access> GetAccessInternal(bool permitRevoked = false)
        {
            var token = GetRequestToken();
            var userId = GetRequestUserId();
            if (token == null || userId == null)
            {
                return null;
            }

            var access = await _accessRepository.GetAccess(token, userId.Value);
            if (permitRevoked)
            {
                return access;
            }

            if (access?.Revoked != null)
            {
                // the access has been revoked
                return null;
            }

            return access;
        }

        private async Task<Access> GetImpersonatedByAccess()
        {
            var token = GetImpersonatedByToken();
            var userId = GetImpersonatedByUserId();
            if (token == null || userId == null)
            {
                return null;
            }

            var access = await _accessRepository.GetAccess(token, userId.Value);
            if (access?.Revoked != null)
            {
                // the access has been revoked
                return null;
            }

            return access;
        }

        private async Task<AccessRequest> GetAccessRequestInternal()
        {
            var token = GetRequestToken();
            var userId = GetRequestUserId();
            if (token == null || userId == null)
            {
                return null;
            }

            return await _accessRepository.GetAccessRequest(token, userId.Value);
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(Access access)
        {
            if (access.Revoked != null)
            {
                return _serviceHelper.NotPermitted<AccessDto>("Unable to recover revoked access");
            }

            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessToken(access.Token, newToken);

            SetCookies(newToken, access.UserId);

            return _serviceHelper.Success("Access recovered", _accessDtoAdapter.Adapt(access));
        }

        private async Task<ActionResultDto<AccessDto>> RecoverAccess(AccessRequest accessRequest)
        {
            var newToken = Guid.NewGuid().ToString();
            await _accessRepository.UpdateAccessRequestToken(accessRequest.Token, newToken);

            SetCookies(newToken, accessRequest.UserId);

            var existingAccess = await _accessRepository.GetAccess(accessRequest.UserId);
            if (existingAccess != null)
            {
                return _serviceHelper.Success("Access already exists", _accessDtoAdapter.Adapt(existingAccess));
            }

            var newAccess = new Access
            {
               Admin = false,
               Manager = false,
               Granted = DateTime.UtcNow,
               Name = accessRequest.Name,
               Revoked = null,
               RevokedReason = null,
               TeamId = accessRequest.TeamId,
               UserId = accessRequest.UserId,
               Token = newToken,
            };
            await _accessRepository.AddAccess(newAccess);

            // clean up the access request
            await _accessRepository.RemoveAccessRequest(accessRequest.UserId);

            return _serviceHelper.Success("Access request recovered", _accessDtoAdapter.Adapt(newAccess));
        }
    }
}
