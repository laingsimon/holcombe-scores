using NUnit.Framework.Interfaces;
using TechTalk.SpecFlow;

namespace HolcombeScores.Test.Steps;

/// <summary>
/// https://github.com/SpecFlowOSS/SpecFlow/issues/2608
///
/// The background isn't awaited, so scenario steps execute before the background has completed.
/// These steps patch this until the issue is resolved.
/// </summary>
[Binding]
public class BackgroundPatchSteps
{
    private readonly BackgroundAwaitingContext _context;

    public BackgroundPatchSteps(BackgroundAwaitingContext context)
    {
        _context = context;
    }

    [Then("the background is now complete")]
    public void TheBackgroundIsNowComplete()
    {
        _context.WaitHandle.Set();
    }

    [Given("I wait for the background to complete")]
    public void IWaitForTheBackgroundToComplete()
    {
        var resultOutcome = NUnit.Framework.TestContext.CurrentContext.Result.Outcome;

        for (var index = 0; index < 100; index++)
        {
            if (_context.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100)))
            {
                return;
            }

            if (resultOutcome == ResultState.Error || resultOutcome == ResultState.Failure)
            {
                return;
            }
        }
    }

    public class BackgroundAwaitingContext
    {
        public ManualResetEvent WaitHandle { get; }

        public BackgroundAwaitingContext()
        {
            WaitHandle = new ManualResetEvent(false);
        }
    }
}