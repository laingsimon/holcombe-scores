namespace HolcombeScores.Api.Services;

public interface ITestingContext
{
    Guid? ContextId { get; }
    string GetTableName(string tableName);
    bool IsTestingTable(string tableName);
    bool IsRealTable(string tableName);
    Guid? GetContextIdFromTableName(string tableName);
}