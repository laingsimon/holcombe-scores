namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// A representation of an existing access record, granted or revoked
    /// </summary>
    public class AccessDto
    {
        public Guid TeamId { get; set; }
        public DateTime Granted { get; set; }
        public DateTime? Revoked { get; set; }
        public Guid UserId { get; set; }
        public bool Admin { get; set; }
        public bool Manager { get; set; }
        public string Name { get; set; }
        public string RevokedReason { get; set; }
    }
}