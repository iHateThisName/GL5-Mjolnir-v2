using System;
using TMPro;
using UnityEngine;

public class StickyNoteController : MonoBehaviour
{
    [Serializable]
    private class TaskEntry
    {
        public SituationData situation;

        [TextArea(2, 5)]
        public string instruction;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text taskText;

    [Header("Tasks — first active entry takes priority")]
    [SerializeField] private TaskEntry[] tasks = Array.Empty<TaskEntry>();

    [Header("Fallback Messages")]
    [TextArea(2, 4)]
    [SerializeField]
    private string waitingText =
        "No active task. Check for new instructions.";

    [TextArea(2, 4)]
    [SerializeField]
    private string completedText =
        "All listed tasks are finished.";

    private SituationManager manager;

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        // Covers initialization order if the manager was unavailable
        // when this component was enabled.
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (manager != null)
        {
            manager.RuntimeReady -= Refresh;
            manager.OnSituationStateChange -= OnSituationChanged;
            manager.OnMailPresentationCompleted -= OnMailFinished;
        }

        manager = null;
    }

    private void Subscribe()
    {
        if (manager != null)
            return;

        manager = SituationManager.Instance;

        if (manager == null)
            return;

        manager.RuntimeReady += Refresh;
        manager.OnSituationStateChange += OnSituationChanged;
        manager.OnMailPresentationCompleted += OnMailFinished;
    }

    private void OnSituationChanged(SituationData situation)
    {
        Refresh();
    }

    private void OnMailFinished(MailResolution resolution)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (taskText == null)
            return;

        if (manager == null || !manager.IsReady)
        {
            taskText.text = "Checking today's tasks...";
            return;
        }

        // Do not direct the player elsewhere while an email's
        // feedback/cutscene sequence is still running.
        if (manager.HasPendingMailPresentation)
        {
            taskText.text = "Finish reviewing the email feedback.";
            return;
        }

        bool hasTasks = false;
        bool allCompleted = true;

        foreach (TaskEntry entry in tasks)
        {
            if (entry == null || entry.situation == null)
                continue;

            hasTasks = true;

            SituationManager.SituationStateEnum state =
                manager.GetSituationState(entry.situation);

            if (state == SituationManager.SituationStateEnum.Active)
            {
                taskText.text =
                    string.IsNullOrWhiteSpace(entry.instruction)
                        ? entry.situation.SituationName
                        : entry.instruction;

                return;
            }

            bool completed =
                state == SituationManager.SituationStateEnum.Success ||
                state == SituationManager.SituationStateEnum.Failed;

            if (!completed)
                allCompleted = false;
        }

        taskText.text = hasTasks && allCompleted
            ? completedText
            : waitingText;
    }
}