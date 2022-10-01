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
        public bool Training { get; set; }
        public bool Postponed { get; set; }
        public string Address { get; set; }
        public bool Friendly { get; set; }
        /// <summary>
        /// Manager awarded player of the session
        /// </summary>
        public Guid? ManagerPots { get; set; }
        /// <summary>
        /// Supporter awarded player of the session
        /// </summary>
        public Guid? SupporterPots { get; set; }
        /// <summary>
        /// Player awarded player of the session
        /// </summary>
        public Guid? PlayerPots { get; set; }
    }
}
