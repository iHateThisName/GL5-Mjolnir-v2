using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // Added for the Button

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class SummaryUI : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Canvas summaryCanvas;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private RectTransform contentContainer; // Reference to the container for content elements
    [SerializeField] private GameObject contentElementPrefab; // Reference to the prefab for content elements

    [Header("Navigation")]
    [SerializeField] private Button mainMenuButton; // Drag your UI Button here in the Inspector


    private ConditionTracker conditionTracker;

    private void OnEnable() {
        conditionTracker = ConditionTracker.Instance;

        if (conditionTracker != null) {
            conditionTracker.OnConditionStateChanged += OnConditionStateChanged;
        } else {
            Debug.LogError(
                "SummaryUI requires an initialized ConditionTracker.", this);
        }

        // Subscribe to the button click
        if (mainMenuButton != null) {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void OnDisable() {
        if (conditionTracker != null) {
            conditionTracker.OnConditionStateChanged -= OnConditionStateChanged;
        }

        conditionTracker = null;

        // Unsubscribe to prevent memory leaks
        if (mainMenuButton != null) {
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);
        }
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == ConditionTracker.ConditionEnum.ShowSummary &&
            state.ExpectedState) {
            DisplayEndOfTheDaySummary();
        }
    }

    [ContextMenu("Display End of the Day Summary")]
    public void DisplayEndOfTheDaySummary() {
        GameManager.Instance.CurrentPlayerState = EnumPlayerState.Summary;
        List<SituationData> allSituations = SituationManager.Instance.Situations;
        //List<MailData> mails = SituationManager.Instance.Situations.OfType<MailData>().Where(mail => mail != null).Distinct().ToList();

        StringBuilder summary = new StringBuilder();
        int totalEmails = 0;
        int completedEmails = 0;
        int ReportedScams = 0;
        int AcceptedScams = 0;
        int AcceptedLegitimate = 0;
        int ReportedLegitimate = 0;
        int CorrectlyScamReportedOptions = 0;
        int totalScamEmail = ReportedScams + AcceptedScams + AcceptedLegitimate + ReportedLegitimate;

        foreach (SituationData situationData in allSituations) {


            if (situationData is MailData) {
                totalEmails++;
                if (situationData.IsCompleted) completedEmails++;
                // Mail

                MailData mailData = situationData as MailData;
                SituationManager.Instance.TryGetMailResolution(mailData, out MailResolution resolution);

                //if (mailData.MailType == CommunicationTypeEnum.Scam) {
                switch (resolution.Outcome) {
                    case MailOutcome.AcceptedScam: // This is a scam that the player accepted, which is bad.
                        AcceptedScams++;
                        break;

                    case MailOutcome.ReportedScam: // This is a scam that the player reported, which is good.
                        ReportedScams++;
                        CorrectlyScamReportedOptions += resolution.EvidenceScore.CorrectSelections;
                        break;

                    case MailOutcome.AcceptedLegitimate: // This is a legitimate email that the player accepted, which is good.
                        AcceptedLegitimate++;
                        break;

                    case MailOutcome.ReportedLegitimate: // This is a legitimate email that the player reported, which is bad.
                        ReportedLegitimate++;
                        break;
                    default:
                        Debug.LogWarning($"Unhandled mail outcome: {resolution.Outcome}");
                        break;

                }
            }

        }

        summary.AppendLine($"Emails Handled: {completedEmails}/{totalEmails}");
        summary.AppendLine($"Scams Reported: {ReportedScams}/{totalScamEmail}");
        //summary.AppendLine($"Scams Accepted: {AcceptedScams}/{totalScamEmail}");
        summary.AppendLine($"Legitimate Emails Accepted: {AcceptedLegitimate}/{totalEmails}");
        //summary.AppendLine($"Legitimate Emails Reported: {ReportedLegitimate}/{totalEmails}");
        summary.AppendLine($"Correctly Reported Scams: {CorrectlyScamReportedOptions}/{totalScamEmail * 5}");

        summary.AppendLine($"Grade: {CalculateGrade(totalEmails, totalScamEmail, completedEmails, CorrectlyScamReportedOptions)}");

        summaryText.text = summary.ToString();

        this.summaryCanvas.gameObject.SetActive(true);
    }

    public GradeEnum CalculateGrade(int totalEmails, int totalScamEmail, int completedEmails, int correctlyScamReportedOptions) {
        float completionRate = (float)completedEmails / totalEmails;
        float scamReportOptionRate = (float)correctlyScamReportedOptions / (totalScamEmail * 5);

        if (completionRate < 0.5f || scamReportOptionRate < 0.5f) {
            return GradeEnum.F;
        } else if (completionRate < 0.7f || scamReportOptionRate < 0.7f) {
            return GradeEnum.C;
        } else if (completionRate < 0.9f || scamReportOptionRate < 0.9f) {
            return GradeEnum.B;
        } else {
            return GradeEnum.A;
        }
    }

    public void AddElement(string leftText, string rightText) {
        GameObject newElement = Instantiate(contentElementPrefab, contentContainer);
    }

    //int completedEmails = mails.Count(mail => mail.IsCompleted);
    //int correctlyReportedScams = 0;

    //foreach (var mail in mails)
    //{
    //    if (manager.TryGetMailResolution(mail, out var resolution) &&
    //        resolution.Outcome == MailOutcome.ReportedScam)
    //    {
    //        correctlyReportedScams++;
    //    }
    //}

    //var summary = new StringBuilder();
    //summary.AppendLine($"Emails Handled: {completedEmails}/{mails.Count}");
    //summary.AppendLine(
    //    $"Scams Reported Correctly: {correctlyReportedScams}");
    //summary.AppendLine(
    //    $"Evidence Score: {manager.CurrentScamReportScore}/" +
    //    $"{manager.TotalScamReportScore}");

    //summaryText.text = summary.ToString();
    //summaryContainer.SetActive(true);



    public void GoToMainMenu() {
        if (GameSceneManager.Instance != null) {
            GameSceneManager.Instance.LoadScene(GameSceneManager.Instance.mainMenuScene);
        } else {
            Debug.LogError("GameSceneManager is missing!", this);
        }
    }

    public enum GradeEnum : int { F = 0, C = 1, B = 2, A = 3 }
}