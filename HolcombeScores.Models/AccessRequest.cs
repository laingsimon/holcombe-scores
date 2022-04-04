using System;

namespace HolcombeScores.Models
{
    public class AccessRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public Guid TeamId { get; set; }
        public DateTime Requested { get; set; }
    }
}