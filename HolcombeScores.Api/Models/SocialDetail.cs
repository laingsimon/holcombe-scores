namespace HolcombeScores.Api.Models;

public class SocialDetail
{
    public string Url { get; set; }

    /// <summary>
    /// Advice: Should be between 55 and 500 characters, limit 300
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Advice: Should be between 30 and 60 characters, limit 90
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Advice: Should have a ratio of 1.19:1 (1200 x 630)
    /// </summary>
    public string Image { get; set; }
    public string Type { get; set; }

    public Dictionary<string, string> ToVariables()
    {
        return new Dictionary<string, string>
        {
            {"og:url", Url},
            {"og:description", Description},
            {"og:title", Title},
            {"og:image", Image},
            {"og:type", Type},
        };
    }
}