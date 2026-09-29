using UnityEngine;

[CreateAssetMenu(fileName = "TaskData", menuName = "Scriptable Objects/Situation/TaskData")]
public class TaskData : SituationData {
    [Header("Task Data")]
    [SerializeField] private TaskTypeEnum taskType = TaskTypeEnum.None;
    public TaskTypeEnum TaskType => taskType;

    public enum TaskTypeEnum { None, Main, Side, Optional }
}
