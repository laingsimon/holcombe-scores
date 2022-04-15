using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly TableClient _teamTableClient;

        public TeamRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _teamTableClient = tableServiceClientFactory.CreateTableClient("Team");
        }

        public IAsyncEnumerable<Team> GetAll()
        {
            return _teamTableClient.QueryAsync<Team>();
        }

        public async Task<Team> Get(Guid teamId)
        {
            return await _teamTableClient.QuerySingleOrDefaultAsync<Team>(teamId.ToString(), teamId.ToString());
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
