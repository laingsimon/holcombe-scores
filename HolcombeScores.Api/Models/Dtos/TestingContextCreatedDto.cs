namespace HolcombeScores.Api.Models.Dtos;

public class TestingContextCreatedDto
{
    public Guid ContextId { get; set; }
    public ActionResultDto<DeleteTestingContextDto> DeletionResult { get; set; }
}