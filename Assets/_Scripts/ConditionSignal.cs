using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class ConditionSignal : MonoBehaviour {
    [field: SerializeField] public ConditionTracker.ConditionState conditionState { get; private set; }
    [SerializeField] private UnityEvent OnConditionMet;
    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
    }
    private void OnDisable() {
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
        }
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == conditionState.Condition && state.ExpectedState == conditionState.ExpectedState) {
            OnConditionMet?.Invoke();
        }
    }
}
