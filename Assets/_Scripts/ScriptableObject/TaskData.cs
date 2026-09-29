using UnityEngine;

public abstract class TaskData : SituationData {
    [Header("Task Data")]
    [SerializeField] private TaskTypeEnum taskType = TaskTypeEnum.None;

    // Public property to access the private field
    public TaskTypeEnum TaskType => taskType;

    public enum TaskTypeEnum : int { None = 0, Main = 1, Side = 2, Optional = 3 }
}
