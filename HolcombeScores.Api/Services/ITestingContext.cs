namespace HolcombeScores.Api.Services;

public interface ITestingContext
{
    Guid? ContextId { get; }
    bool TestingContextRequired { get; }
    string GetTableName(string tableName);
    bool IsTestingTable(string tableName);
    bool IsRealTable(string tableName);
}