using System;

namespace HolcombeScores.Models
{
    public class Player
    {
        public Guid TeamId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
    }
}