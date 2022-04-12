using HolcombeScores.Api.Models;
using HolcombeScores.Models;

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

        public MyAccessDto Adapt(Access access, AccessRequest accessRequest)
        {
            return new MyAccessDto
            {
                Access = _accessDtoAdapter.Adapt(access),
                AccessRequest = _accessRequestDtoAdapter.Adapt(accessRequest),
            };
        }
    }
}
