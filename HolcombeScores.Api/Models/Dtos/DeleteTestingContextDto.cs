namespace HolcombeScores.Api.Models.Dtos;

public class DeleteTestingContextDto
{
    public string ContextId { get; set; }
    public List<string> RemovedTables { get; set; }
}