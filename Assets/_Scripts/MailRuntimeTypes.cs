using System;
using System.Collections.Generic;
using System.Linq;

public enum MailRedFlag
{
    SpoofedAddress = 0,
    SuspiciousLinkUrl = 1,
    UrgencyOrFear = 2,
    BadFileExtension = 3,
    AskingForCredentials = 4
}

public enum MailDecision { Accept, Report }
public enum MailOutcome { AcceptedScam, ReportedScam, AcceptedLegitimate, ReportedLegitimate }
public enum MailSequenceStep
{
    ScamCutscene,
    MandatoryMeeting,
    ThankYouMonologue,
    CloseCallCutscene,
    OrderConfirmation,
    AngryBossCutscene
}

public readonly struct MailReportScore
{
    public int CorrectSelections { get; }
    public int IncorrectSelections { get; }
    public int MissedFlags { get; }
    public int Points { get; }
    public int MaximumPoints => CorrectSelections + MissedFlags;
    public bool IsExactMatch => IncorrectSelections == 0 && MissedFlags == 0;

    public MailReportScore(int correct, int incorrect, int missed, bool penalizeIncorrect)
    {
        CorrectSelections = correct;
        IncorrectSelections = incorrect;
        MissedFlags = missed;
        Points = Math.Max(0, correct - (penalizeIncorrect ? incorrect : 0));
    }
}

// A runtime record. Never stored on the ScriptableObject.
public sealed class MailResolution
{
    public MailData Mail { get; }
    public MailDecision Decision { get; }
    public MailOutcome Outcome { get; }
    public bool ClassificationCorrect =>
        Outcome == MailOutcome.ReportedScam || Outcome == MailOutcome.AcceptedLegitimate;
    public IReadOnlyList<MailRedFlag> SelectedFlags { get; }
    public MailReportScore EvidenceScore { get; }

    internal MailResolution(MailData mail, MailDecision decision,
        IEnumerable<MailRedFlag> selectedFlags, bool penalizeIncorrect)
    {
        Mail = mail;
        Decision = decision;
        var flags = decision == MailDecision.Report
            ? MailReportScoring.UniqueFlags(selectedFlags)
            : new HashSet<MailRedFlag>();
        SelectedFlags = Array.AsReadOnly(flags.OrderBy(flag => (int)flag).ToArray());
        EvidenceScore = MailReportScoring.Evaluate(mail, flags, penalizeIncorrect);
        Outcome = MailOutcomeRules.Classify(mail.IsScam, decision);
    }
}

public static class MailReportScoring
{
    // Reject invalid enum values rather than silently scoring an invalid submission.
    public static HashSet<MailRedFlag> UniqueFlags(IEnumerable<MailRedFlag> flags)
    {
        var result = new HashSet<MailRedFlag>();
        if (flags == null) return result;
        foreach (MailRedFlag flag in flags)
        {
            if (!Enum.IsDefined(typeof(MailRedFlag), flag))
                throw new ArgumentOutOfRangeException(nameof(flags), flag, "Unknown mail red flag.");
            result.Add(flag);
        }
        return result;
    }

    public static MailReportScore Evaluate(MailData mail,
        IEnumerable<MailRedFlag> selectedFlags, bool penalizeIncorrect = true)
    {
        if (mail == null) throw new ArgumentNullException(nameof(mail));
        var expected = mail.IsScam ? UniqueFlags(mail.CorrectRedFlags) : new HashSet<MailRedFlag>();
        var selected = UniqueFlags(selectedFlags);
        int correct = selected.Count(expected.Contains);
        return new MailReportScore(correct, selected.Count - correct,
            expected.Count - correct, penalizeIncorrect);
    }

    public static int MaximumFor(MailData mail) =>
        mail != null && mail.IsScam ? UniqueFlags(mail.CorrectRedFlags).Count : 0;

    public static string Label(MailRedFlag flag)
    {
        switch (flag)
        {
            case MailRedFlag.SpoofedAddress: return "Spoofed Address";
            case MailRedFlag.SuspiciousLinkUrl: return "Suspicious Link URL";
            case MailRedFlag.UrgencyOrFear: return "Urgency/Fear Pressure";
            case MailRedFlag.BadFileExtension: return "Bad File Extension";
            case MailRedFlag.AskingForCredentials: return "Asking for Credentials";
            default: return flag.ToString();
        }
    }
}

public static class MailOutcomeRules
{
    public static MailOutcome Classify(bool isScam, MailDecision decision)
    {
        switch (decision)
        {
            case MailDecision.Accept:
                return isScam ? MailOutcome.AcceptedScam : MailOutcome.AcceptedLegitimate;
            case MailDecision.Report:
                return isScam ? MailOutcome.ReportedScam : MailOutcome.ReportedLegitimate;
            default: throw new ArgumentOutOfRangeException(nameof(decision));
        }
    }

    public static IReadOnlyList<MailSequenceStep> StepsFor(MailOutcome outcome)
    {
        switch (outcome)
        {
            case MailOutcome.AcceptedScam:
                return Array.AsReadOnly(new[] { MailSequenceStep.ScamCutscene, MailSequenceStep.MandatoryMeeting });
            case MailOutcome.ReportedScam:
                return Array.AsReadOnly(new[] { MailSequenceStep.ThankYouMonologue, MailSequenceStep.CloseCallCutscene });
            case MailOutcome.AcceptedLegitimate:
                return Array.AsReadOnly(new[] { MailSequenceStep.OrderConfirmation });
            case MailOutcome.ReportedLegitimate:
                return Array.AsReadOnly(new[] { MailSequenceStep.AngryBossCutscene });
            default: throw new ArgumentOutOfRangeException(nameof(outcome));
        }
    }
}
