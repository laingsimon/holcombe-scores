namespace HolcombeScores.Test.Configuration;

public class TestingConfiguration : ITestingConfiguration
{
    public string ApiAddress { get; set; } = null!;
    public string UiAddress { get; set; } = null!;
    public string AdminPassCode { get; set; } = null!;
}