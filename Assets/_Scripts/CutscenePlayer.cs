using System;
using UnityEngine;
using UnityEngine.Playables;
using static ConditionTracker;

public class CutscenePlayer : MonoBehaviour {
    [SerializeField] private PlayableDirector director;
    [SerializeField] private ConditionTracker.ConditionState[] requiredConditions = Array.Empty<ConditionTracker.ConditionState>();
    [SerializeField] private ConditionTracker.ConditionState[] raisedFinishConditions = Array.Empty<ConditionTracker.ConditionState>();

    private void OnEnable() {
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
        }
        this.director.stopped += OnCutsceneStopped;
    }

    private void OnDisable() {
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
        }

        this.director.stopped -= OnCutsceneStopped;
    }
    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        foreach (ConditionState conditionState in this.requiredConditions) {

            if (conditionState.ExpectedState) {
                if (!ConditionTracker.Instance.HasCondition(conditionState.Condition)) {
                    return; // A condtion is not true
                }
            } else {
                if (ConditionTracker.Instance.HasCondition(conditionState.Condition)) {
                    return; // A condtion that should be false is true.
                }
            }
        }

        // This point is reache when all requiredConditions are met.
        PlayCutscene();
    }

    private void PlayCutscene() {
        director.Stop();
        director.time = 0;
        director.Play();
    }
    private void OnCutsceneStopped(PlayableDirector director) {

        Debug.Log("Director Stopped");

        foreach (ConditionState condition in raisedFinishConditions) {
            ConditionTracker.Instance.SetCondition(condition);
        }
    }

}
