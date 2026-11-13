using System;
using UnityEngine;
using UnityEngine.Playables;
using static ConditionTracker;

[RequireComponent(typeof(PlayableDirector))]
public class CutscenePlayer : MonoBehaviour {
    [field: SerializeField] public PlayableDirector Director { get; private set; }
    [SerializeField] private ConditionTracker.ConditionState[] requiredConditions = Array.Empty<ConditionTracker.ConditionState>();
    [SerializeField] private ConditionTracker.ConditionState[] raisedFinishConditions = Array.Empty<ConditionTracker.ConditionState>();

    private void Awake() {
        this.Director = GetComponent<PlayableDirector>();
    }
    private void OnEnable() {
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
        }
        this.Director.stopped += OnCutsceneStopped;
    }

    private void OnDisable() {
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
        }

        this.Director.stopped -= OnCutsceneStopped;
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
        Director.Stop();
        Director.time = 0;
        Director.Play();
    }
    private void OnCutsceneStopped(PlayableDirector director) {

        Debug.Log("Director Stopped");

        foreach (ConditionState condition in raisedFinishConditions) {
            ConditionTracker.Instance.SetCondition(condition);
        }
    }

}
