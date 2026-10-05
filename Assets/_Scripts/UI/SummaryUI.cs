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
    private int totalNumberOfNessecarySituations = 0;
    private int completedSituationsCount = 0;
    private int completedNecessarySituationsCount = 0;

    private void OnEnable() {
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChanged;
    }

    private void OnDisable() {
        SituationManager.Instance.OnSituationStateChange -= OnSituationStateChanged;
    }
    private void Start() {
        this.totalNumberOfNessecarySituations = SituationManager.Instance.Situations.Where(x => x.TypeEnum == SituationData.situationTypeEnum.PrimaryTask).Count();
    }

    private void OnSituationStateChanged(SituationData data) {
        // Check if the situation is completed;
        if (data.IsCompleted) {
            this.completedSituationsCount++;

            if (data.TypeEnum == SituationData.situationTypeEnum.PrimaryTask) {
                this.completedNecessarySituationsCount++;
                if (this.totalNumberOfNessecarySituations == this.completedNecessarySituationsCount) {
                    DisplayEndOfTheDaySummary();
                }
            }
        }
    }

    [ContextMenu("Display End of the Day Summary")]
    public void DisplayEndOfTheDaySummary() {
        StringBuilder summaryBuilder = new StringBuilder();

        List<SituationData> failedScam = new List<SituationData>();
        List<SituationData> successfulScam = new List<SituationData>();
        int totalCompletedSituations = 0;
        int totalEmails = 0;

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
                totalEmails++;
            }
        });

        summaryBuilder.AppendLine($"Completed Tasks: {totalCompletedSituations}/{this.totalNumberOfNessecarySituations}");
        summaryBuilder.AppendLine($"Emails: {totalEmails}");
        summaryBuilder.AppendLine($"Avoided Scams: {successfulScam.Count}");
        summaryBuilder.AppendLine($"Scams: {failedScam.Count}");

        this.summaryText.text = summaryBuilder.ToString();
        this.summaryContainer.SetActive(true);
    }
}
