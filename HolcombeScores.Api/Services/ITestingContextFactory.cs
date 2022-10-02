using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services;

public interface ITestingContextFactory
{
    ITestingContext Create();
}