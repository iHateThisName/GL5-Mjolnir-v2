using UnityEngine;

public abstract class SituationData : ScriptableObject {
    [Header("Situation Data")]
    [SerializeField, Tooltip("Name of the situation, used for identification and task name.")] 
    private string situationName = string.Empty;

    [SerializeField, Tooltip("Optional, The parent situation that this situation is a part of. If null, this situation is a root task.")] 
    private SituationData parentSituation = null;

    [SerializeField, Tooltip("Optional, Array of situations that must be completed before this situation can be activated.")] 
    private SituationData[] requiredSituations  = new SituationData[0];

    [Tooltip("Optional, Array of conditions that must be met for this situation to be activated.")]
    [SerializeField] private ConditionTracker.ConditionState[] requiredConditions = new ConditionTracker.ConditionState[0];

    [Tooltip("Optional, Array of conditions that will be set when this situation is completed.")]
    [SerializeField] private ConditionTracker.ConditionState[] resultingConditions = new ConditionTracker.ConditionState[0];

    // Public properties to access the private fields
    public string SituationName => situationName; // Name of the situation, used for identification and task name.
    public SituationData[] RequiredSituations => requiredSituations; // Optional, Array of situations that must be completed before this situation can be activated.
    public SituationData ParentTask => parentSituation; // Optional, The parent task that this situation is a part of. If null, this situation is a root task.
    public ConditionTracker.ConditionState[] RequiredConditions => requiredConditions; // Optional, Array of conditions that must be met for this situation to be activated.
    public ConditionTracker.ConditionState[] ResultingConditions => resultingConditions; // Optional, Array of conditions that will be set when this situation is completed.

    public bool IsRequiredSituationsCompleted() {
        int numberOfSituationsCompleted = 0;

        foreach (SituationData situation in requiredSituations) {
            if (situation.IsRequiredConditionsMet()) {
                numberOfSituationsCompleted++;
            } else {
                return false; // If any required situation is not completed, return false immediately
            }
        }
        return numberOfSituationsCompleted == requiredSituations.Length;
    }

    public bool IsRequiredConditionsMet() {
        int numberOfConditionsMet = 0;

        foreach (ConditionTracker.ConditionState condition in requiredConditions) {
            if (condition.IsMet()) {
                numberOfConditionsMet++;
            }
        }

        return numberOfConditionsMet == requiredConditions.Length;
    }
}
