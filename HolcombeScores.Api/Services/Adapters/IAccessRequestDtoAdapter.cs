using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessRequestDtoAdapter
    {
        AccessRequest Adapt(AccessRequestDto accessRequest);
        AccessRequestDto Adapt(AccessRequest accessRequest);
    }
}