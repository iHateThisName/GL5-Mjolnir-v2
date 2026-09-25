using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// 
/// Centralized manager responsible for tracking and querying global game conditions.
/// Conditions represent boolean world or gameplay states that other systems can
/// read or modify in order to coordinate game progression, tasks, events, and interactions.
///
/// Examples:
/// - Tutorial has been completed.
/// - Printer is on fire
///
/// This class acts as a shared source of truth for condition-based logic.
/// </summary>

public class ConditionTracker : Singleton<ConditionTracker> {

    // HashSet to store the current conditions. Using a HashSet allows for less expensive lookups and ensures that each condition is unique(Not allwing duplicates).
    private readonly HashSet<ConditionEnum> conditions = new HashSet<ConditionEnum>();

    /// <summary>
    /// Checks whether the specified condition is currently active.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <returns>
    /// True if the condition is currently active; otherwise false.
    /// </returns>
    public bool HasCondition(ConditionEnum condition) {
        return conditions.Contains(condition);
    }

    /// <summary>
    /// Updates the state of a condition.
    ///
    /// If the expected state is true, the condition is activated.
    /// If the expected state is false, the condition is deactivated.
    /// </summary>
    /// <param name="conditionState">
    /// Contains the target condition and the state it should be set to.
    /// </param>
    public void SetCondition(ConditionState conditionState) {
        if (conditionState.Condition == ConditionEnum.None) {
            Debug.LogWarning("ConditionEnum.None should never be used.");
            return;
        }
        if (conditionState.ExpectedState) {
            this.conditions.Add(conditionState.Condition);
        } else {
            this.conditions.Remove(conditionState.Condition);
        }
    }

    /// <summary>
    /// Retrieves the current state of a condition.
    /// </summary>
    /// <param name="condition">The condition to retrieve.</param>
    /// <returns>
    /// A ConditionState containing the condition and its current state.
    /// </returns>
    public ConditionState GetCondition(ConditionEnum condition) {
        ConditionState state = new ConditionState(condition, this.HasCondition(condition));
        return state;
    }

    /// <summary>
    /// Defines all trackable game conditions used by the ConditionTracker.
    /// Each value represents a boolean world state that may influence
    /// gameplay, task availability, event triggering, or progression.
    /// </summary>
    public enum ConditionEnum : int {
        None = 0, // Should never be used.
        HasCompletedTutorial = 1,
    }

    /// <summary>
    /// Represents a condition together with its expected state.
    /// This can be build upon to define complex condition requirements or how it interacts with tasks, events, or interactions.
    /// </summary>
    public readonly struct ConditionState {
        public readonly ConditionEnum Condition; // The condition that this state is about
        public readonly bool ExpectedState; // The expected state of the condition (true for active, false for inactive)
        public ConditionState(ConditionEnum condition, bool expectedState) {
            this.Condition = condition;
            this.ExpectedState = expectedState;
        }
    }
}
