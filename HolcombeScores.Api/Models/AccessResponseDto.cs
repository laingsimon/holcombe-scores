using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents a response from an administrator to an access request
    /// </summary>
    public class AccessResponseDto
    {
        public Guid UserId { get; set; }
        public bool Allow { get; set; }
        public string Reason { get; set; }
        public Guid TeamId { get; set; }
    }
}