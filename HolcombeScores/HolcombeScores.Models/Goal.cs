using System;

namespace HolcombeScores.Models
{
    public class Goal
    {
        public DateTime Time { get; set; }
        public bool HolcombeGoal { get; set; }
        public Player Player { get; set; }
    }
}