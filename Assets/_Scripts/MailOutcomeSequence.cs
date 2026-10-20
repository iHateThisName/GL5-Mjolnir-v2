using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Keep this component on an always-active controller object, NOT on feedbackPanel.
public class MailOutcomeSequence : MonoBehaviour
{
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TMP_Text feedbackTitle;
    [SerializeField] private TMP_Text feedbackBody;
    [SerializeField] private Button continueButton;
    [Tooltip("Until cutscenes exist: Continue manually completes each labeled placeholder step.")]
    [SerializeField] private bool usePlaceholderContinue = true;

    private readonly List<MailSequenceStep> steps = new List<MailSequenceStep>();
    private int stepIndex;
    private int tokenCounter;
    public int CurrentStepToken { get; private set; }
    public MailResolution CurrentResolution { get; private set; }
    public bool IsPlaying => CurrentResolution != null;
    public bool IsConfigured => feedbackPanel != null && feedbackTitle != null &&
                                feedbackBody != null && continueButton != null;
    public bool CanBegin => IsConfigured && !IsPlaying;

    // Future cutscenes: capture the token, then CompleteStep(token) on real completion.
    public event Action<MailResolution, MailSequenceStep, int> StepRequested;
    public event Action<MailResolution> Completed;

    private void Awake()
    {
        if (continueButton != null) continueButton.onClick.AddListener(ContinuePlaceholder);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(ContinuePlaceholder);
    }

    public void Begin(MailResolution resolution)
    {
        if (!CanBegin || resolution == null)
            throw new InvalidOperationException("Mail outcome sequence is busy or not configured.");
        CurrentResolution = resolution;
        steps.Clear();
        steps.AddRange(MailOutcomeRules.StepsFor(resolution.Outcome));
        stepIndex = 0;
        ShowStep();
    }

    private void ContinuePlaceholder() => CompleteStep(CurrentStepToken);

    public void CompleteStep(int token)
    {
        // Ignore duplicate or stale asynchronous completion callbacks.
        if (!IsPlaying || token != CurrentStepToken) return;
        CurrentStepToken = ++tokenCounter;
        stepIndex++;
        if (stepIndex < steps.Count) { ShowStep(); return; }
        MailResolution finished = CurrentResolution;
        CurrentResolution = null;
        feedbackPanel.SetActive(false);
        Completed?.Invoke(finished);
    }

    private void ShowStep()
    {
        CurrentStepToken = ++tokenCounter;
        feedbackPanel.SetActive(true);
        continueButton.gameObject.SetActive(usePlaceholderContinue);
        MailSequenceStep step = steps[stepIndex];
        feedbackTitle.text = Title(step);
        feedbackBody.richText = true;
        feedbackBody.text = Body(step);
        StepRequested?.Invoke(CurrentResolution, step, CurrentStepToken);
    }

    private string Title(MailSequenceStep step)
    {
        switch (step)
        {
            case MailSequenceStep.ScamCutscene: return "You accepted a scam";
            case MailSequenceStep.MandatoryMeeting: return "Mandatory security meeting";
            case MailSequenceStep.ThankYouMonologue: return "Scam correctly reported";
            case MailSequenceStep.CloseCallCutscene: return "Close call";
            case MailSequenceStep.OrderConfirmation: return "Legitimate work completed";
            case MailSequenceStep.AngryBossCutscene: return "You reported legitimate work";
            default: return "Email outcome";
        }
    }

    private string Body(MailSequenceStep step)
    {
        MailData mail = CurrentResolution.Mail;
        switch (step)
        {
            case MailSequenceStep.ScamCutscene:
                return "This email was a scam. Its link, attachment, or action was unsafe.\n\n" +
                       "[Scam cutscene placeholder — Continue proceeds to the mandatory meeting.]";
            case MailSequenceStep.MandatoryMeeting:
                return $"{mail.AnnotatedEmailAddress}\n\n" +
                       string.Join("\n", mail.AnnotatedEmailBody) + "\n\nLook for:\n" +
                       string.Join("\n", MailReportScoring.UniqueFlags(mail.CorrectRedFlags)
                           .OrderBy(flag => (int)flag).Select(flag => "• " + MailReportScoring.Label(flag))) +
                       "\n\n[Meeting cutscene placeholder — Continue returns to work.]";
            case MailSequenceStep.ThankYouMonologue:
                return SuccessReply("Thank you for catching that scam!") + "\n\n" + EvidenceSummary();
            case MailSequenceStep.CloseCallCutscene:
                return "You prevented the scam.\n\n[Close-call cutscene placeholder — Continue returns to work.]";
            case MailSequenceStep.OrderConfirmation:
                return SuccessReply("Your request has been processed. Keep going!");
            case MailSequenceStep.AngryBossCutscene:
                return "This email was legitimate work, not a scam. Cross-check the sender and actual destination " +
                       "against the Company Directory.\n\n[Angry Boss cutscene placeholder — Continue returns to work.]";
            default: return string.Empty;
        }
    }

    private string SuccessReply(string fallback) =>
        string.IsNullOrWhiteSpace(CurrentResolution.Mail.FeedbackReplyText)
            ? fallback : CurrentResolution.Mail.FeedbackReplyText;

    private string EvidenceSummary()
    {
        MailReportScore score = CurrentResolution.EvidenceScore;
        var expected = MailReportScoring.UniqueFlags(CurrentResolution.Mail.CorrectRedFlags);
        var selected = MailReportScoring.UniqueFlags(CurrentResolution.SelectedFlags);
        var text = new StringBuilder();
        text.AppendLine(score.MaximumPoints > 0
            ? $"Evidence score: {score.Points}/{score.MaximumPoints}"
            : "No checklist evidence is configured for this email.");
        var missed = expected.Except(selected).OrderBy(flag => (int)flag).ToArray();
        var extra = selected.Except(expected).OrderBy(flag => (int)flag).ToArray();
        if (missed.Length > 0)
            text.AppendLine("Missed: " + string.Join(", ", missed.Select(MailReportScoring.Label)));
        if (extra.Length > 0)
            text.AppendLine("Not evidence here: " + string.Join(", ", extra.Select(MailReportScoring.Label)));
        if (score.IsExactMatch && score.MaximumPoints > 0) text.AppendLine("You identified all the evidence.");
        return text.ToString();
    }
}
