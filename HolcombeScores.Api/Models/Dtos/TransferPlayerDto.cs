namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents the request to transfer a player from one team to another
    /// </summary>
    public class TransferPlayerDto
    {
        public Guid PlayerId { get; set; }
        public int? NewNumber { get; set; }
        public Guid NewTeamId { get; set; }
    }
}
