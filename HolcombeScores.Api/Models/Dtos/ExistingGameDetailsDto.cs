using System;

namespace HolcombeScores.Api.Models.Dtos
{
    public class ExistingGameDetailsDto : GameDetailsDto
    {
        public Guid Id { get; set; }
    }
}