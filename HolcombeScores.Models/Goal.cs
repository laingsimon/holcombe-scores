using System;

namespace HolcombeScores.Models
{
    public class Goal
    {
        public DateTime Time { get; set; }
        public bool HolcombeGoal { get; set; }
        public int PlayerNumber { get; set; }
        public Guid TeamId { get; set; }
        public Guid GameId { get; set; }
    }
}
