using System;

namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents a request from someone to have access to the data
    /// </summary>
    public class AccessRequestDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public Guid TeamId { get; set; }
        public DateTime Requested { get; set; }
    }
}