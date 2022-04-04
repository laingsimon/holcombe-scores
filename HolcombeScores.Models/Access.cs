using System;

namespace HolcombeScores.Models
{
    public class Access
    {
        public Guid TeamId { get; set; }
        public DateTime Granted { get; set; }
        public DateTime? Revoked { get; set; }
        public Guid UserId { get; set; }
        public bool Admin { get; set; }
        public string Name { get; set; }
        public string RevokedReason { get; set; }
    }
}