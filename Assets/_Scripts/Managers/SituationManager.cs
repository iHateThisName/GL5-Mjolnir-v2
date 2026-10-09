using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// 
/// The situation manager is responsible for managing and tracking the current situations in the game. 
/// Situations represent specific scenarios or events that can occur during gameplay, and they may have associated conditions that need to be met for them to be active or resolved.
/// </summary>

[DefaultExecutionOrder(-9)]
public class SituationManager : Singleton<SituationManager> {
    [field: SerializeField] public List<SituationData> Situations { get; private set; } = new List<SituationData>();
    public event System.Action<SituationData> OnSituationStateChange; // When the situation state has changed based on SituationStateEnum

    public int TotalScamReportScore => this.Situations.OfType<MailData>().Where(mail => mail.IsSuspiciousLinkReported || mail.IsUnknownSenderReported || mail.IsTimePressureReported || mail.IsSpellingErrorsReported).Count() * 4;
    public int currentScamReportsScore = 0;
    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnCoditionChanged;
    }

    private void OnDisable() {
        ConditionTracker.Instance.OnConditionStateChanged -= OnCoditionChanged;
    }

    private async void Start() {

        this.Situations.ForEach(situation => {
            situation.Initialize();
        });

        await Awaitable.WaitForSecondsAsync(1);

        this.Situations.ForEach(situation => {
            CheckSituationState(situation);
        });
    }

    [ContextMenu("Debug Log Active Situations")]
    public void DebugLogActiveSituations() {
        List<SituationData> activeSituations = Situations.Where(s => s.SituationStateEnum == SituationStateEnum.Active).ToList();
        Debug.Log($"Active Situations: {activeSituations.Count}");
        foreach (SituationData situation in activeSituations) {
            Debug.Log($"Active Situation: {situation.SituationName}");
        }
    }

    [ContextMenu("Debug Log All Situations")]
    public void DebugLogAllSituations() {
        StringBuilder sb = new StringBuilder();
        foreach (SituationData situation in Situations) {
            sb.AppendLine($"Situation: {situation.SituationName}, State: {situation.SituationStateEnum}");
        }
        Debug.Log(sb.ToString());
    }
    public void AddSituation(SituationData situationData) {
        if (!Situations.Contains(situationData)) {
            Situations.Add(situationData);

            // If a situation gets added that does not start inactive then notify any listner.
            if (situationData.SituationStateEnum != SituationStateEnum.Inactive) {
                OnSituationStateChange?.Invoke(situationData);
                Debug.Log($"Situation {situationData.SituationName} added. Current state: {situationData.SituationStateEnum}");
            }
        }
    }

    public void OnCoditionChanged(ConditionTracker.ConditionState changed) {
        List<SituationData> situationsRelated = this.Situations.FindAll(s => s.RequiredConditions.Contains(changed));

        situationsRelated.ForEach(situation => {
            if (situation.IsRequiredConditionsMet()) {

                if (situation.SituationStateEnum == SituationStateEnum.Success) {
                    // Raise the resulting conditions associated with that situation.
                    foreach (ConditionTracker.ConditionState raisedCondition in situation.ResultingSuccessConditions) {
                        ConditionTracker.Instance.SetCondition(raisedCondition);
                    }
                } else if (situation.SituationStateEnum == SituationStateEnum.Failed) {
                    // Raise the resulting conditions associated with that situation.
                    foreach (ConditionTracker.ConditionState raisedCondition in situation.ResultingFailedConditions) {
                        ConditionTracker.Instance.SetCondition(raisedCondition);
                    }
                }

                // Update the situation state.
                if (situation.SituationStateEnum == SituationStateEnum.Inactive && situation.IsRequiredSituationsCompleted()) {
                    situation.SituationStateEnum = SituationStateEnum.Active;

                    // Notify that the state has been changed.
                    OnSituationStateChange?.Invoke(situation);
                    Debug.Log($"Situation {situation.SituationName} is now active due to condition change: {changed.Condition}");
                }
            }
        });
    }

    public void CheckAllSituationState() {
        // Find all situations that are not completed (NOT successful/failed).
        List<SituationData> NotCompletedSituations = Situations.FindAll(x => x.SituationStateEnum != SituationStateEnum.Success || x.SituationStateEnum != SituationStateEnum.Failed);

        foreach (SituationData situation in NotCompletedSituations) {
            CheckSituationState(situation);
        }
    }

    public void CheckSituationState(SituationData situation) {
        if (situation.SituationStateEnum != SituationStateEnum.Success || situation.SituationStateEnum != SituationStateEnum.Failed) {
            if (situation.IsRequiredSituationsCompleted() && situation.IsRequiredConditionsMet()) {
                situation.SituationStateEnum = SituationStateEnum.Active;
                OnSituationStateChange?.Invoke(situation);
                Debug.Log("Situation " + situation.SituationName + " is now active.");
            }
        }
    }

    public void SetSituationState(SituationManager.SituationStateEnum newState, SituationData situation) {
        if (situation.SituationStateEnum == SituationManager.SituationStateEnum.Inactive) return;

        // State has to be active to change state to success or failed

        if (newState == SituationManager.SituationStateEnum.Success || newState == SituationManager.SituationStateEnum.Failed) {
            situation.SituationStateEnum = newState;

            if (newState == SituationManager.SituationStateEnum.Success) {
                // Set the resulting conditions when the situation is completed successfully
                foreach (ConditionTracker.ConditionState condition in situation.ResultingSuccessConditions) {
                    ConditionTracker.Instance.SetCondition(condition);
                }
            } else if (newState == SituationManager.SituationStateEnum.Failed) {
                // Set the resulting conditions when the situation fails
                foreach (ConditionTracker.ConditionState condition in situation.ResultingFailedConditions) {
                    ConditionTracker.Instance.SetCondition(condition);
                }
            }

            // Check if any child situations can now be activated based on the new state of this situation.
            List<SituationData> childSituations = this.Situations.FindAll(s => s.ParentTask == situation || s.RequiredSituations.Contains(situation));
            childSituations.ForEach(child => CheckSituationState(child));

            OnSituationStateChange?.Invoke(situation);
            Debug.Log($"Situation {situation.SituationName} has changed state to {newState}");
        }

    }

    internal void ReportScam(MailData currentSelectedMail, bool isSuspiciousLinkReported, bool isUnknownSenderReported,
                             bool isTimePressureReported, bool isSpellingErrorsReported) {
        

        int correctReports = 0;

        if (currentSelectedMail.IsSuspiciousLinkReported == isSuspiciousLinkReported) {
            correctReports++;
        }

        if (currentSelectedMail.IsUnknownSenderReported == isUnknownSenderReported) {
            correctReports++;
        }

        if (currentSelectedMail.IsTimePressureReported == isTimePressureReported) {
            correctReports++;
        }

        if (currentSelectedMail.IsSpellingErrorsReported == isSpellingErrorsReported) {
            correctReports++;
        }

        this.currentScamReportsScore += correctReports;
    }

    [System.Serializable] public enum SituationStateEnum : int { Inactive = 0, Active = 1, Success = 2, Failed = 3 }
}
