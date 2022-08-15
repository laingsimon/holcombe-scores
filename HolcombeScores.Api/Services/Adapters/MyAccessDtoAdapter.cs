using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class MyAccessDtoAdapter : IMyAccessDtoAdapter
    {
        private readonly IAccessDtoAdapter _accessDtoAdapter;
        private readonly IAccessRequestDtoAdapter _accessRequestDtoAdapter;

        public MyAccessDtoAdapter(IAccessDtoAdapter accessDtoAdapter, IAccessRequestDtoAdapter accessRequestDtoAdapter)
        {
            _accessDtoAdapter = accessDtoAdapter;
            _accessRequestDtoAdapter = accessRequestDtoAdapter;
        }

        public MyAccessDto Adapt(Access access, IEnumerable<AccessRequest> accessRequests, Access impersonatedBy = null)
        {
            return new MyAccessDto
            {
                Access = _accessDtoAdapter.Adapt(access, impersonatedBy),
                Requests = accessRequests.Select(_accessRequestDtoAdapter.Adapt).ToArray(),
            };
        }
    }
}
