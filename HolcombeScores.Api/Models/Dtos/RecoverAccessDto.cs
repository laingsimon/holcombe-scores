using System.Text.Json.Serialization;

namespace HolcombeScores.Api.Models.Dtos
{
    /// <summary>
    /// A representation of an existing access record that can be recovered
    /// </summary>
    public class RecoverAccessDto
    {
        public string RecoveryId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AdminPassCode { get; set; }

        public bool Admin { get; set; }
    }
}
