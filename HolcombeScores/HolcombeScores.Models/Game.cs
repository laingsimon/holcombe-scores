using System;

namespace HolcombeScores.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public DateTime Date { get; set; }
        public Player[] Squad { get; set; }

        public Goal[] Goals { get; set; }
    }
}