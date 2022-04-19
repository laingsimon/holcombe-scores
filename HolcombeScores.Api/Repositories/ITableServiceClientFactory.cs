using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public interface ITableServiceClientFactory
    {
        TableClient CreateTableClient(string tableName);
    }
}