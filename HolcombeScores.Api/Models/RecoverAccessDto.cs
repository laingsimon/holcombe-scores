using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// A representation of an existing access record that can be recovered
    /// </summary>
    public class RecoverAccessDto
    {
        public string RecoveryId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
