using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public interface IAccessRepository
    {
        IAsyncEnumerable<Access> GetAllAccess();
        IAsyncEnumerable<AccessRequest> GetAllAccessRequests();
        Task<AccessRequest> GetAccessRequest(Guid userId);
        Task<Access> GetAccess(Guid userId);
        Task AddAccessRequest(AccessRequest accessRequest);
        Task AddAccess(Access access);
        Task RemoveAccessRequest(Guid userId);
        Task UpdateAccess(Access access);
        Task RemoveAccess(Guid userId);
    }
}
