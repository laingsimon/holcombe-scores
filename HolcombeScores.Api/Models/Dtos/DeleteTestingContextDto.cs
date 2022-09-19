namespace HolcombeScores.Api.Models.Dtos;

public class DeleteTestingContextDto
{
    public Guid ContextId { get; set; }
    public List<string> RemovedTables { get; set; }
}