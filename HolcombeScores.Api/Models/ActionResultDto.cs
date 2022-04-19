using System.Collections.Generic;

namespace HolcombeScores.Api.Models
{
    /// <summary>
    /// Represents the outcome of an action with its data and any pertinent messages
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionResultDto<T>
    {
        public T Outcome { get; set; }
        public bool Success { get; set; }
        public List<string> Messages { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();
    }
}