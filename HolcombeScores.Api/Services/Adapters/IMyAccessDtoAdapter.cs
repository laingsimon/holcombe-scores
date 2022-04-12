using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IMyAccessDtoAdapter
    {
        MyAccessDto Adapt(Access access, AccessRequest accessRequest);
    }
}
