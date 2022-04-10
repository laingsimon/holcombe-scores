using System;

namespace HolcombeScores.Api.Models
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
    }
}