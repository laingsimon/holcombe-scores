using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents the details of a (Holcombe) player
    /// </summary>
    public class PlayerDto
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public Guid TeamId { get; set; }
    }
}