using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TypedTableClient<Team> _teamTableClient;

        public TeamRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _teamTableClient = new TypedTableClient<Team>(tableServiceClientFactory.CreateTableClient("Team"));
        }

        public IAsyncEnumerable<Team> GetAll()
        {
            return _teamTableClient.QueryAsync();
        }

        public async Task<Team> Get(Guid teamId)
        {
            return await _teamTableClient.SingleOrDefaultAsync(t => t.Id == teamId);
        }

        public async Task CreateTeam(Team team)
        {
            team.ETag = ETag.All;
            team.RowKey = team.Id.ToString();
            team.PartitionKey = team.Id.ToString();
            await _teamTableClient.AddEntityAsync(team);
        }

        public async Task UpdateTeam(Team team)
        {
            await _teamTableClient.UpdateEntityAsync(team, team.ETag);
        }

        public async Task DeleteTeam(Guid id)
        {
            await _teamTableClient.DeleteEntityAsync(id.ToString(), id.ToString());
        }
    }
}
