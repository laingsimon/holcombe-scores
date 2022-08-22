namespace HolcombeScores.Api.Models.Dtos;

public class UpdateAvailabilityDto
{
    public Guid TeamId { get; set; }
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public bool Available { get; set; }
}