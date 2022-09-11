namespace HolcombeScores.Test;

public class TestingContext : ITestingContext
{
    public string ApiAddress { get; set; } = null!;
    public string UiAddress { get; set; } = null!;
    public string AdminPassCode { get; set; } = null!;
}