using System.Text.RegularExpressions;
using Azure.Data.Tables;
using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Repositories;

public class TableClientFactory : ITableClientFactory
{
    public const int MaxTableLength = 63;
    public const int MaxHolcombeScoresTableName = 13; // the longest table name - AccessRequest
    private const string TableNameRegex = "^[A-Za-z][A-Za-z0-9]{2,62}$";

    private readonly IAzureRepositoryContext _repositoryContext;
    private readonly ITestingContext _testingContext;

    public TableClientFactory(IAzureRepositoryContext repositoryContext, ITestingContext testingContext)
    {
        _repositoryContext = repositoryContext;
        _testingContext = testingContext;
    }

    public TableClient CreateTableClient(string tableName)
    {
        return CreateTableClient(tableName, _testingContext);
    }

    public TableClient CreateTableClient(string tableName, ITestingContext testingContext)
    {
        if (!Regex.IsMatch(tableName, TableNameRegex))
        {
            throw new ArgumentOutOfRangeException(nameof(tableName), "Table name does not match required format");
        }

        var client = new TableClient(
            _repositoryContext.StorageUri,
            (testingContext ?? _testingContext).GetTableName(tableName),
            _repositoryContext.GetTableSharedKeyCredential());

        client.CreateIfNotExists();
        return client;
    }
}