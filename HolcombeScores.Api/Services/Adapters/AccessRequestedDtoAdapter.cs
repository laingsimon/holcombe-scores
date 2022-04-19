using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class AccessRequestedDtoAdapter : IAccessRequestedDtoAdapter
    {
        private readonly IAccessRequestDtoAdapter _accessRequestDtoAdapter;

        public AccessRequestedDtoAdapter(IAccessRequestDtoAdapter accessRequestDtoAdapter)
        {
            _accessRequestDtoAdapter = accessRequestDtoAdapter;
        }

        public AccessRequestedDto Adapt(AccessRequest accessRequest)
        {
            if (accessRequest == null)
            {
                return null;
            }

            return new AccessRequestedDto
            {
                UserId = accessRequest.UserId,
                Request = _accessRequestDtoAdapter.Adapt(accessRequest),
            };
        }
    }
}
