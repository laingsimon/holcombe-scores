using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents a received request for access to the data
    /// </summary>
    public class AccessRequestedDto
    {
        public Guid UserId { get; set; }
        public AccessRequestDto Request { get; set; }
    }
}