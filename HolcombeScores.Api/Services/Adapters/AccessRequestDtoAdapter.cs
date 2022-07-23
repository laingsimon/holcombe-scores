using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class AccessRequestDtoAdapter : IAccessRequestDtoAdapter
    {
        public AccessRequest Adapt(AccessRequestDto accessRequest)
        {
            if (accessRequest == null)
            {
                return null;
            }

            return new AccessRequest
            {
                Name = accessRequest.Name,
                Requested = accessRequest.Requested,
                TeamId = accessRequest.TeamId,
                UserId = accessRequest.UserId,
                Reason = accessRequest.Reason,
                Rejected = accessRequest.Rejected,
            };
        }

        public AccessRequestDto Adapt(AccessRequest accessRequest)
        {
            if (accessRequest == null)
            {
                return null;
            }

            return new AccessRequestDto
            {
                Name = accessRequest.Name,
                Requested = accessRequest.Requested,
                TeamId = accessRequest.TeamId,
                UserId = accessRequest.UserId,
                Reason = accessRequest.Reason,
                Rejected = accessRequest.Rejected,
            };
        }
    }
}
