using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-9)]
public class SituationManager : Singleton<SituationManager>
{
    [field: SerializeField] public List<SituationData> Situations { get; private set; } = new List<SituationData>();

    [Header("Provisional Checklist Policy (not specified by the GDD)")]
    [SerializeField] private bool penalizeIncorrectFlags = true;

    private readonly Dictionary<SituationData, SituationStateEnum> states =
        new Dictionary<SituationData, SituationStateEnum>();
    private readonly Dictionary<MailData, MailResolution> mailResults =
        new Dictionary<MailData, MailResolution>();
    private readonly HashSet<SituationData> appliedResultConditions = new HashSet<SituationData>();
    private ConditionTracker conditionTracker;
    private bool started;
    private bool ready;
    private bool evaluating;
    private bool evaluateAgain;

    public event Action<SituationData> OnSituationStateChange;
    public event Action RuntimeReady;
    public bool IsReady => ready;
    public event Action<MailResolution> OnMailPresentationCompleted;

    // One decision sequence globally, including when several coworker mailboxes exist.
    public MailResolution PendingMailPresentation { get; private set; }
    public bool HasPendingMailPresentation => PendingMailPresentation != null;
    public int TotalScamReportScore => states.Keys.OfType<MailData>()
        .Sum(MailReportScoring.MaximumFor);
    public int CurrentScamReportScore => mailResults.Values
        .Where(result => result.Decision == MailDecision.Report)
        .Sum(result => result.EvidenceScore.Points);
    // Read-only compatibility for old score displays.
    public int currentScamReportsScore => CurrentScamReportScore;

    private void OnEnable()
    {
        if (!started) return;
        SubscribeConditions();
        CheckAllSituationState();
    }

    private System.Collections.IEnumerator Start()
    {
        started = true;
        SubscribeConditions();
        foreach (SituationData situation in Situations.ToArray()) RegisterSituation(situation);
        // Let every component's Start initialize its conditions before auto-activation.
        yield return null;
        ready = true;
        CheckAllSituationState();
        RuntimeReady?.Invoke();
    }

    private void OnDisable()
    {
        if (conditionTracker != null)
            conditionTracker.OnConditionStateChanged -= OnCoditionChanged;
        conditionTracker = null;
    }

    private void SubscribeConditions()
    {
        if (conditionTracker != null) return;
        conditionTracker = ConditionTracker.Instance;
        if (conditionTracker != null)
            conditionTracker.OnConditionStateChanged += OnCoditionChanged;
        else
            Debug.LogError("SituationManager requires an initialized ConditionTracker.", this);
    }

    public void RegisterSituation(SituationData situation)
    {
        if (situation == null || states.ContainsKey(situation)) return;
        // Insert first so circular references cannot recurse forever during registration.
        states.Add(situation, situation.InitialState);
        foreach (SituationData prerequisite in situation.RequiredSituations)
            RegisterSituation(prerequisite);
        if (situation.ParentTask != null) RegisterSituation(situation.ParentTask);
    }

    public void AddSituation(SituationData situation)
    {
        if (situation == null) return;
        bool isNew = !states.ContainsKey(situation);
        if (!Situations.Contains(situation)) Situations.Add(situation);
        RegisterSituation(situation);
        if (isNew && GetSituationState(situation) != SituationStateEnum.Inactive)
            OnSituationStateChange?.Invoke(situation);
        if (ready) CheckAllSituationState();
    }

    public SituationStateEnum GetSituationState(SituationData situation)
    {
        if (situation == null) return SituationStateEnum.Inactive;
        RegisterSituation(situation);
        return states[situation];
    }

    public void OnCoditionChanged(ConditionTracker.ConditionState changed) => CheckAllSituationState();

    public void CheckSituationState(SituationData situation)
    {
        RegisterSituation(situation);
        CheckAllSituationState();
    }

    public void CheckAllSituationState()
    {
        if (!ready) return;
        // Condition callbacks can reenter evaluation. Queue a second pass instead.
        if (evaluating) { evaluateAgain = true; return; }
        evaluating = true;
        try
        {
            do
            {
                evaluateAgain = false;
                foreach (SituationData situation in Situations.ToArray()) RegisterSituation(situation);
                foreach (SituationData situation in states.Keys.ToArray())
                {
                    if (states[situation] == SituationStateEnum.Inactive &&
                        situation.IsRequiredConditionsMet() && situation.IsRequiredSituationsCompleted())
                    {
                        states[situation] = SituationStateEnum.Active;
                        OnSituationStateChange?.Invoke(situation);
                        evaluateAgain = true;
                    }
                    ApplyResultConditionsOnce(situation);
                }
            } while (evaluateAgain);
        }
        finally { evaluating = false; }
    }

    // Keep the familiar API, but only active situations may finish, once.
    public void SetSituationState(SituationStateEnum newState, SituationData situation)
    {
        // Mail outcomes must include a decision record and presentation sequence.
        if (situation is MailData)
        {
            Debug.LogError("Use TryResolveMail for mail decisions, not SetSituationState.", this);
            return;
        }
        TryFinishSituation(newState, situation);
    }

    private bool TryFinishSituation(SituationStateEnum newState, SituationData situation)
    {
        if (situation == null || GetSituationState(situation) != SituationStateEnum.Active) return false;
        if (newState != SituationStateEnum.Success && newState != SituationStateEnum.Failed) return false;
        states[situation] = newState;
        ApplyResultConditionsOnce(situation);
        OnSituationStateChange?.Invoke(situation);
        CheckAllSituationState();
        return true;
    }

    private void ApplyResultConditionsOnce(SituationData situation)
    {
        SituationStateEnum state = states[situation];
        if (state != SituationStateEnum.Success && state != SituationStateEnum.Failed) return;
        if (appliedResultConditions.Contains(situation) || !situation.IsRequiredConditionsMet()) return;
        if (conditionTracker == null) return;
        // Mark first: SetCondition can synchronously call us again.
        appliedResultConditions.Add(situation);
        var conditions = state == SituationStateEnum.Success
            ? situation.ResultingSuccessConditions : situation.ResultingFailedConditions;
        foreach (ConditionTracker.ConditionState condition in conditions)
            conditionTracker.SetCondition(condition);
    }

    public bool TryResolveMail(MailData mail, MailDecision decision,
        IEnumerable<MailRedFlag> selectedFlags, out MailResolution resolution)
    {
        resolution = null;
        if (!ready || mail == null || HasPendingMailPresentation || mailResults.ContainsKey(mail)) return false;
        if (decision != MailDecision.Accept && decision != MailDecision.Report) return false;
        if (GetSituationState(mail) != SituationStateEnum.Active) return false;

        resolution = new MailResolution(mail, decision, selectedFlags, penalizeIncorrectFlags);
        // Commit before raising any condition or state-change callback.
        mailResults.Add(mail, resolution);
        PendingMailPresentation = resolution;
        TryFinishSituation(resolution.ClassificationCorrect
            ? SituationStateEnum.Success : SituationStateEnum.Failed, mail);
        return true;
    }

    public bool TryGetMailResolution(MailData mail, out MailResolution resolution)
    {
        resolution = null;
        return mail != null && mailResults.TryGetValue(mail, out resolution);
    }

    public bool CompleteMailPresentation(MailResolution resolution)
    {
        if (resolution == null || !ReferenceEquals(PendingMailPresentation, resolution)) return false;
        PendingMailPresentation = null;
        OnMailPresentationCompleted?.Invoke(resolution);
        return true;
    }

    [ContextMenu("Debug Log Active Situations")]
    public void DebugLogActiveSituations()
    {
        foreach (SituationData situation in states.Keys.Where(s => states[s] == SituationStateEnum.Active))
            Debug.Log($"Active Situation: {situation.SituationName}");
    }

    [ContextMenu("Debug Log All Situations")]
    public void DebugLogAllSituations()
    {
        var text = new StringBuilder();
        foreach (SituationData situation in states.Keys)
            text.AppendLine($"{situation.SituationName}: {states[situation]}");
        Debug.Log(text.ToString());
    }

    [Serializable]
    public enum SituationStateEnum { Inactive = 0, Active = 1, Success = 2, Failed = 3 }
}
