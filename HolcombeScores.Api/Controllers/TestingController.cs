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
    public Guid? GetTestingContext()
    {
        return _testingService.GetTestingContextId();
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
}