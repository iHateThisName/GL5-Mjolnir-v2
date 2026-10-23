using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class SummaryUI : MonoBehaviour
{
    [SerializeField] private GameObject summaryContainer;
    [SerializeField] private TMP_Text summaryText;

    private ConditionTracker conditionTracker;

    private void OnEnable()
    {
        conditionTracker = ConditionTracker.Instance;

        if (conditionTracker != null)
        {
            conditionTracker.OnConditionStateChanged += OnConditionStateChanged;
        }
        else
        {
            Debug.LogError(
                "SummaryUI requires an initialized ConditionTracker.", this);
        }
    }

    private void OnDisable()
    {
        if (conditionTracker != null)
        {
            conditionTracker.OnConditionStateChanged -= OnConditionStateChanged;
        }

        conditionTracker = null;
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state)
    {
        if (state.Condition == ConditionTracker.ConditionEnum.IsDayComplete &&
            state.ExpectedState)
        {
            DisplayEndOfTheDaySummary();
        }
    }

    [ContextMenu("Display End of the Day Summary")]
    public void DisplayEndOfTheDaySummary()
    {
        var manager = SituationManager.Instance;

        if (manager == null || summaryText == null || summaryContainer == null)
        {
            Debug.LogError("SummaryUI is missing a required reference.", this);
            return;
        }

        var mails = manager.Situations
            .OfType<MailData>()
            .Where(mail => mail != null)
            .Distinct()
            .ToList();

        int completedEmails = mails.Count(mail => mail.IsCompleted);
        int correctlyReportedScams = 0;

        foreach (var mail in mails)
        {
            if (manager.TryGetMailResolution(mail, out var resolution) &&
                resolution.Outcome == MailOutcome.ReportedScam)
            {
                correctlyReportedScams++;
            }
        }

        var summary = new StringBuilder();
        summary.AppendLine($"Emails Handled: {completedEmails}/{mails.Count}");
        summary.AppendLine(
            $"Scams Reported Correctly: {correctlyReportedScams}");
        summary.AppendLine(
            $"Evidence Score: {manager.CurrentScamReportScore}/" +
            $"{manager.TotalScamReportScore}");

        summaryText.text = summary.ToString();
        summaryContainer.SetActive(true);
    }
}