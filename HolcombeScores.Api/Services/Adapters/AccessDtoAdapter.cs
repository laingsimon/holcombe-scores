using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class AccessDtoAdapter : IAccessDtoAdapter
    {
        public Access Adapt(AccessDto access)
        {
            if (access == null)
            {
                return null;
            }

            return new Access
            {
                Admin = access.Admin,
                Granted = access.Granted,
                Name = access.Name,
                Revoked = access.Revoked,
                TeamId = access.TeamId,
                UserId = access.UserId,
                RevokedReason = access.RevokedReason,
            };
        }

        public AccessDto Adapt(Access access)
        {
            if (access == null)
            {
                return null;
            }

            return new AccessDto
            {
                Admin = access.Admin,
                Granted = access.Granted,
                Name = access.Name,
                Revoked = access.Revoked,
                TeamId = access.TeamId,
                UserId = access.UserId,
                RevokedReason = access.RevokedReason,
            };
        }
    }
}
