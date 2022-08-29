using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Repositories;

public interface ISocialDetailRepository
{
    Task<SocialDetail> GetSocialDetail(string urlPrefix);
}