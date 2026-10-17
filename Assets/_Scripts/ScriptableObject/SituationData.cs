using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SituationData : ScriptableObject
{
    [Header("Situation Data")]
    [SerializeField, Tooltip("Name shown for this situation/task.")]
    private string situationName = string.Empty;
    [SerializeField] private SituationData parentSituation;
    [SerializeField] private SituationData[] requiredSituations = Array.Empty<SituationData>();
    [SerializeField]
    private ConditionTracker.ConditionState[] requiredConditions =
        Array.Empty<ConditionTracker.ConditionState>();
    [SerializeField]
    private ConditionTracker.ConditionState[] resultingSuccessConditions =
        Array.Empty<ConditionTracker.ConditionState>();
    [SerializeField]
    private ConditionTracker.ConditionState[] resultingFailedConditions =
        Array.Empty<ConditionTracker.ConditionState>();
    [SerializeField]
    private SituationManager.SituationStateEnum initialState =
        SituationManager.SituationStateEnum.Inactive;

    public string SituationName => situationName;
    public SituationData ParentTask => parentSituation;
    public SituationManager.SituationStateEnum InitialState => initialState;
    public IReadOnlyList<SituationData> RequiredSituations =>
        Array.AsReadOnly(requiredSituations ?? Array.Empty<SituationData>());
    public IReadOnlyList<ConditionTracker.ConditionState> RequiredConditions =>
        Array.AsReadOnly(requiredConditions ?? Array.Empty<ConditionTracker.ConditionState>());
    public IReadOnlyList<ConditionTracker.ConditionState> ResultingSuccessConditions =>
        Array.AsReadOnly(resultingSuccessConditions ?? Array.Empty<ConditionTracker.ConditionState>());
    public IReadOnlyList<ConditionTracker.ConditionState> ResultingFailedConditions =>
        Array.AsReadOnly(resultingFailedConditions ?? Array.Empty<ConditionTracker.ConditionState>());

    // Read-only compatibility facade for existing readers. No state is saved on this asset.
    public SituationManager.SituationStateEnum SituationStateEnum =>
        SituationManager.Instance != null
            ? SituationManager.Instance.GetSituationState(this) : initialState;

    public bool IsCompleted => SituationStateEnum == SituationManager.SituationStateEnum.Success ||
                               SituationStateEnum == SituationManager.SituationStateEnum.Failed;

    public bool IsRequiredSituationsCompleted()
    {
        foreach (SituationData prerequisite in RequiredSituations)
        {
            if (prerequisite == null || !prerequisite.IsRequiredConditionsMet() || !prerequisite.IsCompleted)
                return false;
        }
        return true;
    }

    public bool IsRequiredConditionsMet()
    {
        foreach (ConditionTracker.ConditionState condition in RequiredConditions)
            if (!condition.IsMet()) return false;
        return true;
    }
}
