using HolcombeScores.Api.Models;
using HolcombeScores.Models;

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
            return new AccessRequestedDto
            {
                UserId = accessRequest.UserId,
                Request = _accessRequestDtoAdapter.Adapt(accessRequest),
            };
        }
    }
}