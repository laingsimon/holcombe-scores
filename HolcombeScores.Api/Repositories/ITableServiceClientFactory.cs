using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public interface ITableServiceClientFactory
    {
        TableClient CreateTableClient(string tableName);
    }
}