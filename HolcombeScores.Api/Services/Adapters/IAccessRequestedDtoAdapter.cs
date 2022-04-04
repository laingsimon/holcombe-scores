using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessRequestedDtoAdapter
    {
        AccessRequestedDto Adapt(AccessRequest accessRequest);
    }
}