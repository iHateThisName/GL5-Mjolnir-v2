using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// 
/// The situation manager is responsible for managing and tracking the current situations in the game. 
/// Situations represent specific scenarios or events that can occur during gameplay, and they may have associated conditions that need to be met for them to be active or resolved.
/// </summary>

public class SituationManager : Singleton<SituationManager> {
    [field: SerializeField] public List<SituationData> Situations { get; private set; } = new List<SituationData>();
    public event System.Action<SituationData> OnSituationStateChange; // When the situation state has changed based on SituationStateEnum

    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnCoditionChanged;
    }

    private void OnDisable() {
        ConditionTracker.Instance.OnConditionStateChanged -= OnCoditionChanged;
    }

    private async void Start() {

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
            }
        }
    }

    public void OnCoditionChanged(ConditionTracker.ConditionState changed) {
        List<SituationData> situationsRelated = this.Situations.FindAll(s => s.RequiredConditions.Contains(changed));

        situationsRelated.ForEach(situation => {
            if (situation.IsRequiredConditionsMet()) {

                // Raise the resulting conditions associated with that situation.
                foreach (ConditionTracker.ConditionState raisedCondition in situation.ResultingConditions) {
                    ConditionTracker.Instance.SetCondition(raisedCondition);
                }
                
                // Update the situation state.
                if (situation.SituationStateEnum == SituationStateEnum.Inactive && situation.IsRequiredSituationsCompleted()) {
                    situation.SituationStateEnum = SituationStateEnum.Active;

                    // Notify that the state has been changed.
                    OnSituationStateChange?.Invoke(situation);
                }
            }
        });
    }

    public void CheckAllSituationState() {
        // Find all situations that are not completed (NOT successful/failed).
        List<SituationData> NotCompletedSituations = Situations.FindAll(x => x.SituationStateEnum != SituationStateEnum.Success || x.SituationStateEnum != SituationStateEnum.Failed);

        foreach (SituationData situation in NotCompletedSituations) {
            if (situation.IsRequiredSituationsCompleted() && situation.IsRequiredConditionsMet()) {
                situation.SituationStateEnum = SituationStateEnum.Active;
                OnSituationStateChange?.Invoke(situation);
            }
        }
    }

    public void CheckSituationState(SituationData situation) {
        if (situation.SituationStateEnum != SituationStateEnum.Success || situation.SituationStateEnum != SituationStateEnum.Failed) {
            if (situation.IsRequiredSituationsCompleted() && situation.IsRequiredConditionsMet()) {
                situation.SituationStateEnum = SituationStateEnum.Active;
                OnSituationStateChange?.Invoke(situation);
            }
        }
    }

    public void SetSituationState(SituationManager.SituationStateEnum newState, SituationData situation) {
        if (situation.SituationStateEnum == SituationManager.SituationStateEnum.Inactive) return;
        
        // State has to be active to change state to success or failed

        if (newState == SituationManager.SituationStateEnum.Success || newState == SituationManager.SituationStateEnum.Failed) {
            situation.SituationStateEnum = newState;

            // Set the resulting conditions when the situation is completed
            foreach (ConditionTracker.ConditionState condition in situation.ResultingConditions) {
                ConditionTracker.Instance.SetCondition(condition);
            }

            // Check if any child situations can now be activated based on the new state of this situation.
            List<SituationData> childSituations = this.Situations.FindAll(s => s.ParentTask == situation || s.RequiredSituations.Contains(situation));
            childSituations.ForEach(child => CheckSituationState(child));
        }

    }
    [System.Serializable] public enum SituationStateEnum : int { Inactive = 0, Active = 1, Success = 2, Failed = 3 }
}
