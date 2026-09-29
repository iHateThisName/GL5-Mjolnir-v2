using Eflatun.SceneReference;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject {
    [Tooltip("The scene for this level")]
    [SerializeField] private SceneReference scene;

    [Tooltip("List of situations that can occur in the level")]
    [SerializeField] private SituationData[] situations;

    [Tooltip("Initial conditions for the level")]
    [SerializeField] private ConditionTracker.ConditionState[] initialConditions;

    // Public properties to access the private fields
    public SceneReference Scene => scene; // The scene for this level
    public SituationData[] Situations => situations; // List of situations that can occur in the level
    public ConditionTracker.ConditionState[] InitialConditions => initialConditions; // Initial conditions for the level
}
