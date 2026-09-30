using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// 
/// The situation manager is responsible for managing and tracking the current situations in the game. 
/// Situations represent specific scenarios or events that can occur during gameplay, and they may have associated conditions that need to be met for them to be active or resolved.
/// </summary>

public class SituationManager : Singleton<SituationManager> {

    public List<SituationData> situationDatas = new List<SituationData>();
    public event System.Action<SituationData> OnSituationCompleted; // Can be both successful or failed

    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnCoditionChanged;
    }

    private void OnDisable() {
        ConditionTracker.Instance.OnConditionStateChanged -= OnCoditionChanged;
    }
    public void AddSituation(SituationData situationData) {
        if (!situationDatas.Contains(situationData)) {
            situationDatas.Add(situationData);
        }
    }

    public void OnCoditionChanged(ConditionTracker.ConditionState changed) {
        List<SituationData> situationsRelated = this.situationDatas.FindAll(s => s.RequiredConditions.Contains(changed));
        
        situationsRelated.ForEach(situation => {
            if (situation.IsRequiredConditionsMet()) {

                // Raise the resulting conditions associated with that situation.
                foreach (ConditionTracker.ConditionState raisedCondition in situation.ResultingConditions) {
                    ConditionTracker.Instance.SetCondition(raisedCondition);
                }

                // Trigger the event to notify that the situation's required conditions are met, allowing it to be activated.
                OnSituationCompleted?.Invoke(situation);
            }
        });
    }
}
