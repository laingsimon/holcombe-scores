namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents a request to create a new game
    /// </summary>
    public class GameDetailsDto
    {
        public Guid TeamId { get; set; }
        public DateTime? Date { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public Guid[] PlayerIds { get; set; }
    }
}