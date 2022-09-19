namespace HolcombeScores.Test.Contexts;

public class ApiSuiteContext
{
    public Guid? TestContextId { get; set; }
    public HashSet<string> TestContextUsages { get; } = new HashSet<string>();
}