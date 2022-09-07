using Azure.Data.Tables;
using HolcombeScores.Api.Services;

namespace HolcombeScores.Api.Repositories;

public class TableClientFactory : ITableClientFactory
{
    private readonly IAzureRepositoryContext _repositoryContext;
    private readonly ITestingContext _testingContext;

    public TableClientFactory(IAzureRepositoryContext repositoryContext, ITestingContext testingContext)
    {
        _repositoryContext = repositoryContext;
        _testingContext = testingContext;
    }

    public TableClient CreateTableClient(string tableName)
    {
        var client = new TableClient(
            _repositoryContext.StorageUri,
            _testingContext.GetTableName(tableName),
            _repositoryContext.GetTableSharedKeyCredential());

        client.CreateIfNotExists();
        return client;
    }
}