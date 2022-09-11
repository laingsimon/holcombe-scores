namespace HolcombeScores.Api.Models.Dtos;

public class CreateTestingContextRequestDto
{
    public bool? CopyExistingTables { get; set; }

    public bool SetContextRequiredCookie { get; set; }

    public Dictionary<string, CreateTestingContextTableRequestDto> Tables { get; set; }
}