namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents the data about a game
    /// </summary>
    public class GameDto
    {
        public Guid TeamId { get; set; }
        public Guid Id { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public DateTime Date { get; set; }
        public PlayerDto[] Squad { get; set; }
        public GoalDto[] Goals { get; set; }
        public bool ReadOnly { get; set; }
        public bool Training { get;set; }
        public bool Playable { get; set; }
        public bool Postponed { get; set; }
        public string Address { get; set; }
        public string GoogleMapsApiKey { get; set; }
        public bool HasStarted { get; set; }
        public bool Friendly { get; set; }

        /// <summary>
        /// A token that must be passed to POST /api/Game/Goal to record a goal
        /// </summary>
        public string RecordGoalToken { get; set; }

        public bool Started { get; set; }
    }
}
