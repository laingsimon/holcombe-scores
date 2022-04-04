using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessDtoAdapter
    {
        Access Adapt(AccessDto access);
        AccessDto Adapt(Access access);
    }
}