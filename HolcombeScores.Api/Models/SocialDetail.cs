namespace HolcombeScores.Api.Models;

public class SocialDetail
{
    public string Url { get; set; }
    public string Description { get; set; }
    public string Title { get; set; }
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