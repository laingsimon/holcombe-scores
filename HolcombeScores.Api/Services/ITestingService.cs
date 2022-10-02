using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services;

public interface ITestingService
{
    Task<ActionResultDto<TestingContextCreatedDto>> CreateTestingContext(CreateTestingContextRequestDto request);
    Task<ActionResultDto<DeleteTestingContextDto[]>> EndTestingContext(EndTestingContextDto request);
    string GetTestingContextId();
    IAsyncEnumerable<TestingContextDetail> GetAllTestingContexts();
    Task<ActionResultDto<DeleteTestingContextDto>> EndAllTestingContexts();
}