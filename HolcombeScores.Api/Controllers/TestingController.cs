using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers;

[ApiController]
public class TestingController : Controller
{
    private readonly ITestingService _testingService;

    public TestingController(ITestingService testingService)
    {
        _testingService = testingService;
    }

    [HttpGet("/api/Testing")]
    public TestingContextDto GetTestingContext()
    {
        return new TestingContextDto
        {
            ContextId = _testingService.GetTestingContextId(),
        };
    }

    [HttpGet("/api/Testing/All")]
    public IAsyncEnumerable<TestingContextDetail> GetTestingContexts()
    {
        return _testingService.GetAllTestingContexts();
    }

    [HttpPost("/api/Testing")]
    public async Task<ActionResultDto<TestingContextCreatedDto>> CreateTestingContext(CreateTestingContextRequestDto request)
    {
        return await _testingService.CreateTestingContext(request);
    }

    [HttpDelete("/api/Testing")]
    public async Task<ActionResultDto<DeleteTestingContextDto>> EndTestingContext()
    {
        return await _testingService.EndTestingContext();
    }

    [HttpDelete("/api/Testing/All")]
    public async Task<ActionResultDto<DeleteTestingContextDto>> EndAllTestingContexts()
    {
        return await _testingService.EndAllTestingContexts();
    }
}