namespace HolcombeScores.Api.Services;

public interface ITestingContext
{
    Guid? ContextId { get; }
    string GetTableName(string tableName);
    string GetTableName(string tableName, Guid alternativeContextId);
    bool IsTestingTable(string tableName);
    bool IsRealTable(string tableName);
}