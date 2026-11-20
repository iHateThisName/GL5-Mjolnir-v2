using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StickyNoteController : MonoBehaviour
{
    private const string BULLET_PREFIX = "• ";

    [Serializable]
    private class TaskEntry
    {
        public SituationData situation;

        [TextArea(2, 5)]
        public string instruction;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private GameObject noteContentRoot;
    [SerializeField] private GameObject minimizedIcon;

    [Header("Display Settings")]
    [SerializeField] private bool startMinimized = false;
    [SerializeField] private bool sortCompletedToBottom = true;
    [SerializeField] private string completedColorHex = "#6E6E6E";

    [Header("Tasks")]
    [SerializeField] private TaskEntry[] tasks = Array.Empty<TaskEntry>();

    [Header("Fallback Messages")]
    [TextArea(2, 4)]
    [SerializeField]
    private string waitingText = "No active task. Check for new instructions.";

    [TextArea(2, 4)]
    [SerializeField]
    private string completedText = "All listed tasks are finished.";

    private SituationManager manager;
    private bool isMinimized;
    private bool isHudVisible;

    private readonly List<string> activeLines = new List<string>();
    private readonly List<string> completedLines = new List<string>();
    private readonly StringBuilder stringBuilder = new StringBuilder();

    private void Awake()
    {
        this.isMinimized = this.startMinimized;
        this.isHudVisible = this.IsGameplayState();
        this.UpdateVisibility();
    }

    private void OnEnable()
    {
        this.Subscribe();
        this.Refresh();
    }

    private void Start()
    {
        // Covers initialization order if the manager was unavailable on OnEnable.
        this.Subscribe();
        this.isHudVisible = this.IsGameplayState();
        this.UpdateVisibility();
        this.Refresh();
    }

    private void Update()
    {
        bool canShow = this.IsGameplayState();

        if (canShow != this.isHudVisible)
        {
            this.isHudVisible = canShow;
            this.UpdateVisibility();

            if (this.isHudVisible)
                this.Refresh();
        }

        if (!this.isHudVisible)
            return;

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            this.ToggleMinimized();
    }

    private void OnDisable()
    {
        if (this.manager != null)
        {
            this.manager.RuntimeReady -= this.Refresh;
            this.manager.OnSituationStateChange -= this.OnSituationChanged;
            this.manager.OnMailPresentationCompleted -= this.OnMailFinished;
        }

        this.manager = null;
    }

    public void ToggleMinimized()
    {
        this.SetMinimized(!this.isMinimized);
    }

    public void SetMinimized(bool minimized)
    {
        this.isMinimized = minimized;
        this.UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!this.isHudVisible)
        {
            if (this.noteContentRoot != null)
                this.noteContentRoot.SetActive(false);

            if (this.minimizedIcon != null)
                this.minimizedIcon.SetActive(false);

            return;
        }

        if (this.noteContentRoot != null)
            this.noteContentRoot.SetActive(!this.isMinimized);

        if (this.minimizedIcon != null)
            this.minimizedIcon.SetActive(this.isMinimized);
    }

    private bool IsGameplayState()
    {
        if (GameManager.Instance == null)
            return true;

        EnumPlayerState state = GameManager.Instance.CurrentPlayerState;

        return state == EnumPlayerState.Walking ||
               state == EnumPlayerState.Sitting;
    }

    private void Subscribe()
    {
        if (this.manager != null)
            return;

        this.manager = SituationManager.Instance;

        if (this.manager == null)
            return;

        this.manager.RuntimeReady += this.Refresh;
        this.manager.OnSituationStateChange += this.OnSituationChanged;
        this.manager.OnMailPresentationCompleted += this.OnMailFinished;
    }

    private void OnSituationChanged(SituationData situation)
    {
        this.Refresh();
    }

    private void OnMailFinished(MailResolution resolution)
    {
        this.Refresh();
    }

    public void Refresh()
    {
        if (this.taskText == null || !this.isHudVisible)
            return;

        if (this.manager == null || !this.manager.IsReady)
        {
            this.taskText.text = "Checking today's tasks...";
            return;
        }

        if (this.manager.HasPendingMailPresentation)
        {
            this.taskText.text = "Finish reviewing the email feedback.";
            return;
        }

        this.activeLines.Clear();
        this.completedLines.Clear();
        this.stringBuilder.Clear();

        bool hasTasks = false;
        bool allCompleted = true;

        foreach (TaskEntry entry in this.tasks)
        {
            if (entry == null || entry.situation == null)
                continue;

            hasTasks = true;

            SituationManager.SituationStateEnum state =
                this.manager.GetSituationState(entry.situation);

            string label = string.IsNullOrWhiteSpace(entry.instruction)
                ? entry.situation.SituationName
                : entry.instruction;

            bool isCompleted =
                state == SituationManager.SituationStateEnum.Success ||
                state == SituationManager.SituationStateEnum.Failed;

            if (!isCompleted)
                allCompleted = false;

            if (state == SituationManager.SituationStateEnum.Active)
            {
                string line = $"{BULLET_PREFIX}{label}";
                if (this.sortCompletedToBottom)
                    this.activeLines.Add(line);
                else
                    this.AppendLine(line);
            }
            else if (isCompleted)
            {
                string line = $"<color={this.completedColorHex}><s>{BULLET_PREFIX}{label}</s></color>";
                if (this.sortCompletedToBottom)
                    this.completedLines.Add(line);
                else
                    this.AppendLine(line);
            }
        }

        if (this.sortCompletedToBottom)
        {
            foreach (string line in this.activeLines)
                this.AppendLine(line);

            foreach (string line in this.completedLines)
                this.AppendLine(line);
        }

        if (hasTasks && allCompleted)
        {
            if (this.stringBuilder.Length > 0)
                this.stringBuilder.Insert(0, $"{this.completedText}\n");
            else
                this.stringBuilder.Append(this.completedText);
        }
        else if (this.stringBuilder.Length == 0)
        {
            this.stringBuilder.Append(this.waitingText);
        }

        this.taskText.text = this.stringBuilder.ToString();
    }

    private void AppendLine(string line)
    {
        if (this.stringBuilder.Length > 0)
            this.stringBuilder.AppendLine();

        this.stringBuilder.Append(line);
    }
}