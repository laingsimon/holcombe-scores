namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents the data about a goal
    /// </summary>
    public class GoalDto
    {
        public DateTime Time { get; set; }
        public bool HolcombeGoal { get; set; }
        public PlayerDto Player { get; set; }
        public Guid GameId { get; set; }
        public Guid GoalId { get; set; }
    }
}
