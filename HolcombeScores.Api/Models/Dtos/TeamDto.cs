using System;

namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// Represents the details of a team
    /// </summary>
    public class TeamDto
    {
        public string Name { get; set; }
        public string Coach { get; set; }
        public Guid Id { get; set; }
    }
}
