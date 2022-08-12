namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// A representation of an impersonation of another user
    /// </summary>
    public class ImpersonationDto
    {
        public Guid UserId { get; set; }
        public string AdminPassCode { get; set; }
    }
}
