namespace HolcombeScores.Api.Models.Dtos;

public class CreateTestingContextTableRequestDto
{
    public bool? CopyExistingTable { get; set; }

    public Dictionary<string, object>[] Rows { get; set; }
}