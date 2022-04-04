using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents a request to create a new game
    /// </summary>
    public class NewGameDto
    {
        public Guid TeamId { get; set; }
        public DateTime? Date { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public string[] Players { get; set; }
    }
}