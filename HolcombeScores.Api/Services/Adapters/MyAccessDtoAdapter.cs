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

        public async Task<MyAccessDto> Adapt(Access access, IAsyncEnumerable<AccessRequest> accessRequests, Access impersonatedBy = null)
        {
            return new MyAccessDto
            {
                Access = _accessDtoAdapter.Adapt(access, impersonatedBy),
                Requests = accessRequests != null
                    ? await accessRequests.SelectAsync(_accessRequestDtoAdapter.Adapt).ToArrayAsync()
                    : null,
            };
        }
    }
}
