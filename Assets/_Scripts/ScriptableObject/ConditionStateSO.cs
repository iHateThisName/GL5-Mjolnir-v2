using UnityEngine;
using static ConditionTracker;

[CreateAssetMenu(fileName = "ConditionStateSO", menuName = "Scriptable Objects/Situation/ConditionStateSO")]
public class ConditionStateSO : ScriptableObject {
    public ConditionEnum Condition; // The condition that this state is about
    public bool ExpectedState; // The expected state of the condition (true for active, false for inactive)
}
