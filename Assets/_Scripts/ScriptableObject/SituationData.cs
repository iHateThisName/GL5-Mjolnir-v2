using UnityEditor;
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
    [SerializeField] private ConditionTracker.ConditionState[] resultingSuccessConditions = new ConditionTracker.ConditionState[0];
    [SerializeField] private ConditionTracker.ConditionState[] resultingFailedConditions = new ConditionTracker.ConditionState[0];

    [SerializeField] private SituationManager.SituationStateEnum initialState = SituationManager.SituationStateEnum.Inactive; // Situation state enum to represent the starte state of the situation

    [SerializeField] private situationTypeEnum typeEnum = situationTypeEnum.None; // Situation type enum to represent the type of the situation
    // Public properties to access the private fields
    public string SituationName => situationName; // Name of the situation, used for identification and task name.
    public SituationData[] RequiredSituations => requiredSituations; // Optional, Array of situations that must be completed before this situation can be activated.
    public SituationData ParentTask => parentSituation; // Optional, The parent task that this situation is a part of. If null, this situation is a root task.
    public ConditionTracker.ConditionState[] RequiredConditions => requiredConditions; // Optional, Array of conditions that must be met for this situation to be activated.
    public ConditionTracker.ConditionState[] ResultingSuccessConditions => resultingSuccessConditions; // Optional, Array of conditions that will be set when this situation is completed.
    public ConditionTracker.ConditionState[] ResultingFailedConditions => resultingFailedConditions; // Optional, Array of conditions that will be set when this situation fails.
    // Public field
    public SituationManager.SituationStateEnum SituationStateEnum = SituationManager.SituationStateEnum.Inactive; // Situation state enum to represent the current state of the situation
    public situationTypeEnum TypeEnum => typeEnum; // Situation type enum to represent the type of the situation
    public bool IsCompleted => this.SituationStateEnum == SituationManager.SituationStateEnum.Success || this.SituationStateEnum == SituationManager.SituationStateEnum.Failed;
    public bool IsRequiredSituationsCompleted() {
        int numberOfSituationsCompleted = 0;

        foreach (SituationData situation in requiredSituations) {
            if (situation.IsRequiredConditionsMet() && situation.IsCompleted) {
                numberOfSituationsCompleted++;
            } else {
                return false; // If any required situation is not completed, return false immediately
            }
        }
        return numberOfSituationsCompleted == requiredSituations.Length;
    }

    /// <summary>
    /// Checks if all the required conditions have been met for this situation to be active.
    /// </summary>
    /// <returns></returns>
    public bool IsRequiredConditionsMet() {
        int numberOfConditionsMet = 0;

        foreach (ConditionTracker.ConditionState condition in requiredConditions) {
            if (condition.IsMet()) {
                numberOfConditionsMet++;
            }
        }

        return numberOfConditionsMet == requiredConditions.Length;
    }

    public void Initialize() {
        this.SituationStateEnum = this.initialState;
    }

    public enum situationTypeEnum : int     {
        None = 0,
        PrimaryTask = 1,
    }
}
