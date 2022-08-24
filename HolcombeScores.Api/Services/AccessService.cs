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
        private const string ImpersonatingTokenCookieName = "HS_ImpersonatingToken";
        private const string ImpersonatingUserIdCookieName = "HS_ImpersonatingUserId";

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
            var impersonatedByAccess = await GetImpersonatedByAccess();
            var accessRequests = GetAccessRequestsInternal();

            var myAccess = await _myAccessDtoAdapter.Adapt(access, accessRequests, impersonatedByAccess);
            if ((myAccess.Requests == null || myAccess.Requests.Length == 0) && myAccess.Access == null)
            {
                // not logged in, ensure any cookies are removed
                await Logout();
            }

            return myAccess;
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

            var impersonatingAccess = await _accessRepository.GetAccess(impersonation.UserId);
            if (impersonatingAccess == null)
            {
                return _serviceHelper.NotFound<MyAccessDto>("Access not found");
            }

            if (impersonatingAccess.Revoked != null)
            {
                return _serviceHelper.NotPermitted<MyAccessDto>("Access to this user has been revoked");
            }

            var impersonatedByAccess = await GetAccessInternal(permitRevoked: true);
            SetImpersonatingCookies(impersonatingAccess.Token, impersonatingAccess.UserId);

            var myAccess = await _myAccessDtoAdapter.Adapt(impersonatingAccess, null, impersonatedByAccess);
            return _serviceHelper.Success("Impersonation complete", myAccess);
        }

        public async Task<MyAccessDto> Unimpersonate()
        {
            RemoveImpersonationCookies();

            var originalAccess = await GetAccessInternal();
            return await _myAccessDtoAdapter.Adapt(originalAccess, null);
        }

        public async IAsyncEnumerable<RecoverAccessDto> GetAccessForRecovery()
        {
            var userId = GetRequestUserId();
            var identifiedUsers = new HashSet<Guid>();

            await foreach (var access in _accessRepository.GetAllAccess())
            {
                if ((userId != null && access.UserId != userId) || identifiedUsers.Contains(access.UserId))
                {
                    // if the userId cookie was set, then only show the users access requests
                    continue;
                }

                identifiedUsers.Add(access.UserId);
                yield return _recoverAccessDtoAdapter.Adapt(access);
            }

            await foreach (var accessRequest in _accessRepository.GetAllAccessRequests())
            {
                if ((userId != null && accessRequest.UserId != userId) || identifiedUsers.Contains(accessRequest.UserId))
                {
                    // if the userId cookie was set, then only show the users access
                    continue;
                }

                identifiedUsers.Add(accessRequest.UserId);
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

            await foreach (var access in _accessRepository.GetAllAccess(myAccess.Admin ? null : myAccess.Teams))
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
            return (access.Admin || (access.Teams != null && access.Teams.Contains(teamId))) && access.Revoked == null;
        }

        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto accessRequestDto)
        {
            var existingAccessRequests = GetAccessRequestsInternal();
            var existingAccessRequest = existingAccessRequests != null
                ? await existingAccessRequests.SingleOrDefaultAsync(r => r.TeamId == accessRequestDto.TeamId)
                : null;
            if (existingAccessRequest != null)
            {
                // access already requested
                return _accessRequestedDtoAdapter.Adapt(existingAccessRequest);
            }

            var accessRequest = _accessRequestDtoAdapter.Adapt(accessRequestDto);
            accessRequest.UserId = GetImpersonatingUserId() ?? GetRequestUserId() ?? Guid.NewGuid();
            accessRequest.Token = GetImpersonatingToken() ?? GetRequestToken() ?? Guid.NewGuid().ToString();

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

            var accessRequest = await _accessRepository.GetAccessRequest(response.UserId, response.TeamId);
            if (accessRequest == null)
            {
                return _serviceHelper.NotFound<AccessDto>("Access request not found");
            }

            var existingAccess = await _accessRepository.GetAccess(response.UserId);
            if (existingAccess != null)
            {
                if (existingAccess.Teams.Contains(accessRequest.TeamId))
                {
                    await _accessRepository.RemoveAccessRequest(accessRequest.UserId, accessRequest.TeamId);
                    return _serviceHelper.Success("Access already exists", _accessDtoAdapter.Adapt(existingAccess));
                }
            }

            if (response.Allow)
            {
                if (existingAccess == null)
                {
                    var newAccess = new Access
                    {
                        Admin = false,
                        Manager = false,
                        Granted = DateTime.UtcNow,
                        Name = accessRequest.Name,
                        Revoked = null,
                        RevokedReason = null,
                        Teams = new[] { response.TeamId },
                        UserId = response.UserId,
                        Token = accessRequest.Token,
                    };
                    await _accessRepository.AddAccess(newAccess);
                    existingAccess = newAccess;
                }
                else
                {
                    // add team to existing access
                    existingAccess.Teams = existingAccess.Teams.Concat(new[] { accessRequest.TeamId }).ToArray();
                    await _accessRepository.UpdateAccess(existingAccess);
                }

                // clean up the access request
                await _accessRepository.RemoveAccessRequest(response.UserId, response.TeamId);

                return _serviceHelper.Success("Access granted", _accessDtoAdapter.Adapt(existingAccess));
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

            await foreach (var item in _accessRepository.GetAllAccessRequests(myAccess.Admin ? null : myAccess.Teams))
            {
                yield return _accessRequestDtoAdapter.Adapt(item);
            }
        }

        public async Task<ActionResultDto<AccessDto>> RemoveAccess(Guid userId, Guid? teamId)
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

            if (myAccess.Manager && !access.Teams.Any(id => myAccess.Teams.Contains(id)))
            {
                return _serviceHelper.NotPermitted<AccessDto>("Only admins can delete access for another team");
            }

            if (teamId == null)
            {
                await _accessRepository.RemoveAccess(access.UserId);
            }
            else
            {
                await _accessRepository.RemoveAccess(access.UserId, teamId.Value);
            }

            return _serviceHelper.Success("Access removed", _accessDtoAdapter.Adapt(await _accessRepository.GetAccess(userId)));
        }

        public async Task<ActionResultDto<AccessRequestDto>> RemoveAccessRequest(Guid teamId, Guid? userId)
        {
            var myAccess = await GetAccessInternal();
            if (myAccess != null && !myAccess.Admin && !myAccess.Manager && myAccess.UserId != userId)
            {
                return _serviceHelper.NotAnAdmin<AccessRequestDto>();
            }

            var accessRequest = await _accessRepository.GetAccessRequest(userId ?? GetImpersonatingUserId() ?? GetRequestUserId() ?? Guid.Empty, teamId);
            if (accessRequest == null)
            {
                return _serviceHelper.NotFound<AccessRequestDto>("Access request not found");
            }

            if (myAccess?.Manager == true && !myAccess.Teams.Contains(teamId))
            {
                return _serviceHelper.NotPermitted<AccessRequestDto>("Only admins can delete requests for another team");
            }

            await _accessRepository.RemoveAccessRequest(accessRequest.UserId, teamId);
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

            if (myAccess.Manager && !access.Teams.Any(id => myAccess.Teams.Contains(id)))
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

                if (!isAdmin && !accessToUpdate.Teams.Any(id => myAccess.Teams.Contains(id)))
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
                accessToUpdate.Teams = updated.Teams ?? accessToUpdate.Teams;
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
            response?.Cookies.Delete(TokenCookieName);
            response?.Cookies.Delete(UserIdCookieName);

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

        private string GetImpersonatingToken()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(ImpersonatingTokenCookieName, out var token))
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

        private Guid? GetImpersonatingUserId()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var cookies = request?.Cookies.ToDictionary(c => c.Key, c => c.Value) ?? new Dictionary<string, string>();
            if (cookies.TryGetValue(ImpersonatingUserIdCookieName, out var userIdString))
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

        private void SetImpersonatingCookies(string token, Guid userId)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            var options = new CookieOptions
            {
                Secure = true,
                Expires = DateTime.UtcNow.AddHours(1),
                SameSite = SameSiteMode.None,
            };
            response?.Cookies.Append(ImpersonatingTokenCookieName, token, options);
            response?.Cookies.Append(ImpersonatingUserIdCookieName, userId.ToString(), options);
        }

        private void RemoveImpersonationCookies()
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            response?.Cookies.Delete(ImpersonatingTokenCookieName);
            response?.Cookies.Delete(ImpersonatingUserIdCookieName);
        }

        private async Task<Access> GetAccessInternal(bool permitRevoked = false)
        {
            var token = GetImpersonatingToken() ?? GetRequestToken();
            var userId = GetImpersonatingUserId() ?? GetRequestUserId();
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
            if (GetImpersonatingToken() == null || GetImpersonatingUserId() == null)
            {
                return null;
            }

            var token = GetRequestToken();
            var userId = GetRequestUserId();
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

        private IAsyncEnumerable<AccessRequest> GetAccessRequestsInternal()
        {
            var token = GetImpersonatingToken() ?? GetRequestToken();
            var userId = GetImpersonatingUserId() ?? GetRequestUserId();
            if (token == null || userId == null)
            {
                return null;
            }

            return _accessRepository.GetAccessRequests(userId.Value);
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
               Teams = new[] { accessRequest.TeamId },
               UserId = accessRequest.UserId,
               Token = newToken,
            };
            await _accessRepository.AddAccess(newAccess);

            // clean up the access request
            await _accessRepository.RemoveAccessRequest(accessRequest.UserId, accessRequest.TeamId);

            return _serviceHelper.Success("Access request recovered", _accessDtoAdapter.Adapt(newAccess));
        }
    }
}
