namespace HolcombeScores.Test.Configuration;

public interface ITestingConfiguration
{
    string ApiAddress { get; }
    string UiAddress { get; }
    string AdminPassCode { get; }
}