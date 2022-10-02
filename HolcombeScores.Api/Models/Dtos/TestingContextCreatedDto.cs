namespace HolcombeScores.Api.Models.Dtos;

public class TestingContextCreatedDto
{
    public string ContextId { get; set; }
    public ActionResultDto<DeleteTestingContextDto> DeletionResult { get; set; }
}