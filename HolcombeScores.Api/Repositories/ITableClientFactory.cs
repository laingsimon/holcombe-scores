using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories;

public interface ITableClientFactory
{
    TableClient CreateTableClient(string tableName);
}