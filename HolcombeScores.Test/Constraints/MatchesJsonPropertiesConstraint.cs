using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Constraints;

public sealed class MatchesJsonPropertiesConstraint : Constraint
{
    private readonly Table _properties;
    private readonly bool _all;

    public MatchesJsonPropertiesConstraint(Table properties, bool all)
    {
        _properties = properties;
        _all = all;
        Description = GetDescription(properties);
    }

    private static string GetDescription(Table properties)
    {
        return "a json object where " + string.Join(" and ",
            properties.Rows.Select(row => $"`{row["PropertyPath"]}` is to equal `{row["Value"]}`"));
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var json = JObject.Parse((actual as string)!);

        var mismatchingProperties = new Dictionary<string, string>();
        var stringRepresentation = new Dictionary<string, object>();

        foreach (var property in _properties.Rows)
        {
            var propertyPath = property["PropertyPath"];
            var value = property["Value"];

            var matchingTokens = json.SelectTokens(propertyPath).Select(t =>
            {
                if (t is JArray)
                {
                    return t.ToString();
                }

                if (t.Type == JTokenType.Null)
                {
                    return "null";
                }

                return t.ToString(Formatting.None).Trim('\"') ?? "null";
            }).ToArray();
            var matches = _all;

            foreach (var matchingToken in matchingTokens)
            {
                stringRepresentation.Add(propertyPath, matchingToken);
                matches = _all
                    ? matches && matchingToken.Equals(value)
                    : matches || matchingToken.Equals(value);
            }

            if (!matchingTokens.Any())
            {
                stringRepresentation.Add(propertyPath, "**property could not be found**");
                mismatchingProperties.Add(propertyPath, "No values found for property path");
            } else if (!matches)
            {
                mismatchingProperties.Add(
                    propertyPath,
                    _all
                        ? $"Some values do not equal the expected value: {value}"
                        : $"No values equal the expected value: {value}");
            }
        }

        return new ConstraintResult(this, JsonConvert.SerializeObject(stringRepresentation), !mismatchingProperties.Any());
    }
}