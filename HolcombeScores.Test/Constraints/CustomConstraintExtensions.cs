using NUnit.Framework.Constraints;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Constraints;

public static class CustomConstraintExtensions
{
    public static MatchesJsonPropertiesConstraint MatchingJson(this ConstraintExpression expression, Table properties,
        bool all)
    {
        var constraint = new MatchesJsonPropertiesConstraint(properties, all);
        expression.Append(constraint);
        return constraint;
    }
}