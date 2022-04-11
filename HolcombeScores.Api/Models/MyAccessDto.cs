using System;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// A representation of a user's current access
    /// </summary>
    public class MyAccessDto
    {
        public Guid? UserId { get; set; }
        public AccessDto Access { get; set; }
        public AccessRequestDto Request { get; set; }
    }
}
