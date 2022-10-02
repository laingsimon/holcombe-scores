namespace HolcombeScores.Api.Models;

public interface ITestingContext
{
    string ContextId { get; }
    string GetTableName(string tableName);
    bool IsRealTable(string tableName);
    string GetContextIdFromTableName(string tableName);
}