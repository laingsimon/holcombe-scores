namespace HolcombeScores.Test;

public interface ITestingContext
{
    string ApiAddress { get; }
    string UiAddress { get; }
    string AdminPassCode { get; }
}