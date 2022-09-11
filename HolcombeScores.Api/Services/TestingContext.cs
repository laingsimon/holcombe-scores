using System.Text.RegularExpressions;

namespace HolcombeScores.Api.Services;

public class TestingContext : ITestingContext
{
    internal const string ContextIdCookieName = "HS_TestingContext";

    private const string TestTableDelimiter = "TEST";

    public Guid? ContextId { get; set; }

    public bool TestingContextRequired { get; set; }

    public string GetTableName(string tableName)
    {
        if (ContextId == null && TestingContextRequired)
        {
            throw new InvalidOperationException("TestingContext is required but not supplied");
        }

        return ContextId == null
            ? tableName
            : GetTableName(tableName, ContextId.Value);
    }

    public bool IsTestingTable(string tableName)
    {
        return ContextId != null
            ? tableName.EndsWith($"{TestTableDelimiter}{EscapeContextId(ContextId.Value)}")
            : tableName.Contains(TestTableDelimiter);
    }

    public bool IsRealTable(string tableName)
    {
        return !tableName.Contains(TestTableDelimiter);
    }

    public Guid? GetContextIdFromTableName(string tableName)
    {
        if (IsRealTable(tableName))
        {
            return null;
        }

        var match = Regex.Match(tableName, $@".+{TestTableDelimiter}(?<escapedContextId>.+)$");
        if (!match.Success)
        {
            return null;
        }

        var escapedContextId = match.Groups["escapedContextId"].Value;
        return UnescapeContextId(escapedContextId);
    }


    private static string GetTableName(string tableName, Guid alternativeContextId)
    {
        return $"{tableName}{TestTableDelimiter}{EscapeContextId(alternativeContextId)}";
    }

    private static string EscapeContextId(Guid? contextId)
    {
        return contextId == null
            ? null
            : contextId.ToString().Replace("-", "");
    }

    private static Guid UnescapeContextId(string escaped)
    {
        return Guid.Parse($"{escaped[..8]}-{escaped[8..12]}-{escaped[12..16]}-{escaped[16..20]}-{escaped[20..]}");
    }
}