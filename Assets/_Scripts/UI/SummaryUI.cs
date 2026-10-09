using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class SummaryUI : MonoBehaviour {
    [SerializeField] private GameObject summaryContainer;
    [SerializeField] private TMP_Text summaryText;
    private int totalNumberOfPrimaryTasks = 0;
    private int totalNumberOfMailTasks = 0;
    private int completedSituationsCount = 0;
    private int completedNecessarySituationsCount = 0;
    private int completedMailCount = 0;

    private void OnEnable() {
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChanged;
        ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
    }

    private void OnDisable() {
        SituationManager.Instance.OnSituationStateChange -= OnSituationStateChanged;
        ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
    }


    private void Start() {
        List<TaskData> allTasks = SituationManager.Instance.Situations.OfType<TaskData>().ToList();
        List<MailData> allEmails = SituationManager.Instance.Situations.OfType<MailData>().ToList();

        this.totalNumberOfPrimaryTasks = allTasks.Count(task => task.TaskType == TaskData.TaskTypeEnum.Main);
        this.totalNumberOfMailTasks = allEmails.Count;
    }

    private void OnSituationStateChanged(SituationData data) {
        // Check if the situation is completed;
        if (data.IsCompleted) {
            this.completedSituationsCount++;

            // Check if the situation is a of type MailData
            if (data is MailData mailData) {
                this.completedMailCount++;
            }
        }
    }
    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == ConditionTracker.ConditionEnum.IsDayComplete && state.ExpectedState) {
            DisplayEndOfTheDaySummary();
        }
    }

    [ContextMenu("Display End of the Day Summary")]
    public void DisplayEndOfTheDaySummary() {
        StringBuilder summaryBuilder = new StringBuilder();

        List<SituationData> failedScam = new List<SituationData>();
        List<SituationData> successfulScam = new List<SituationData>();
        int totalCompletedSituations = 0;
        int totalMails = 0;

        SituationManager.Instance.Situations.ForEach(situation => {

            if (situation.IsCompleted) {
                totalCompletedSituations++;
            }

            // Check if the situation is a email
            if (situation is MailData emailSituation) {

                // Check if the email is a scam
                if (emailSituation.MailType == MailData.MailTypeEnum.Scam) {

                    if (situation.SituationStateEnum == SituationManager.SituationStateEnum.Success) {
                        successfulScam.Add(situation);
                    } else if (situation.SituationStateEnum == SituationManager.SituationStateEnum.Failed) {
                        failedScam.Add(situation);
                    }

                }
                totalMails++;
            }
        });

        summaryBuilder.AppendLine($"Completed Tasks: {this.completedSituationsCount}");
        summaryBuilder.AppendLine($"Emails: {totalMails}/{this.totalNumberOfMailTasks}");
        summaryBuilder.AppendLine($"Avoided Scams: {successfulScam.Count}");
        summaryBuilder.AppendLine($"Scams Reported Correctly: {SituationManager.Instance.currentScamReportsScore}/{SituationManager.Instance.TotalScamReportScore}");
        //summaryBuilder.AppendLine($"Scams: {failedScam.Count}");

        this.summaryText.text = summaryBuilder.ToString();
        this.summaryContainer.SetActive(true);
    }
}
