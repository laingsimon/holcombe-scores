using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Bindings.Reflection;
using TechTalk.SpecFlow.Tracing;

namespace HolcombeScores.Test;

public class NullStepFormatter : IStepFormatter
{
    public string GetStepDescription(StepInstance stepInstance)
    {
        return "";
    }

    public string GetMatchText(BindingMatch match, object[] arguments)
    {
        return "";
    }

    public string GetMatchText(IBindingMethod method, object[] arguments)
    {
        return "";
    }

    public string GetStepText(StepInstance stepInstance)
    {
        return "";
    }
}