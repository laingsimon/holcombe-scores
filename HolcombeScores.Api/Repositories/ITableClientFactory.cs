using Azure.Data.Tables;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Services;

namespace HolcombeScores.Api.Repositories;

public interface ITableClientFactory
{
    TableClient CreateTableClient(string tableName);
    TableClient CreateTableClient(string tableName, ITestingContext testingContext);
}