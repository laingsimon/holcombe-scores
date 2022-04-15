using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents the request to transfer a player from one team to another
    /// </summary>
    public class TransferPlayerDto
    {
        public int CurrentNumber { get; set; }
        public Guid CurrentTeamId { get; set; }
        public int? NewNumber { get; set; }
        public Guid NewTeamId { get; set; }
    }
}
