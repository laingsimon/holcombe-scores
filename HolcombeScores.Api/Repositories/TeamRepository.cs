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
    }
}
