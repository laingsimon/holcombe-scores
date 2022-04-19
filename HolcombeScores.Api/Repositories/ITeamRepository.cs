using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public interface ITeamRepository
    {
        IAsyncEnumerable<Team> GetAll();
        Task<Team> Get(Guid teamId);
        Task CreateTeam(Team team);
        Task UpdateTeam(Team team);
        Task DeleteTeam(Guid id);
    }
}
