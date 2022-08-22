namespace HolcombeScores.Api.Models.Dtos;

public class AvailabilityDto
{
    public Guid? Id { get; set; }
    public TeamDto Team { get; set; }
    public Guid GameId { get; set; }
    public PlayerDto Player { get; set; }
    public bool? Available { get; set; }
    public AccessDto ReportedBy { get; set; }
}