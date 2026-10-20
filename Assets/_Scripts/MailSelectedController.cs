using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour, IInteractable
{
    [field: SerializeField]
    public ComputerController ComputerController { get; private set; }

    [SerializeField] private MailOutcomeSequence outcomeSequence;

    [Header("Main Email Panel UI")]
    [SerializeField] private TMP_Text selectedMailSender;
    [SerializeField] private TMP_Text selectedMailSubject;
    [SerializeField] private TMP_Text selectedMailBody;

    [Header("Primary Decisions")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button reportButton;

    public UnityEvent<MailData> selectedMailOpened =
        new UnityEvent<MailData>();

    public UnityEvent<MailData> selectedMailFinished =
        new UnityEvent<MailData>();

    public event Action ReportRequested;

    public MailData CurrentMail { get; private set; }

    private bool reportMode;

    public bool IsBusy =>
        SituationManager.Instance != null &&
        SituationManager.Instance.HasPendingMailPresentation;

    public bool CanDecide =>
        CurrentMail != null &&
        !IsBusy &&
        outcomeSequence != null &&
        outcomeSequence.CanBegin &&
        SituationManager.Instance != null &&
        SituationManager.Instance.IsReady &&
        SituationManager.Instance.GetSituationState(CurrentMail) ==
            SituationManager.SituationStateEnum.Active;

    private void Awake()
    {
        if (actionButton != null)
            actionButton.onClick.AddListener(AcceptCurrentMail);

        if (reportButton != null)
            reportButton.onClick.AddListener(RequestReport);

        if (outcomeSequence != null)
            outcomeSequence.Completed += OnSequenceCompleted;

        if (outcomeSequence == null ||
            !outcomeSequence.IsConfigured ||
            actionButton == null ||
            reportButton == null)
        {
            Debug.LogError(
                "MailSelectedController requires both decision buttons " +
                "and a fully configured MailOutcomeSequence.",
                this);
        }

        RenderMail();
    }

    private void OnDestroy()
    {
        if (actionButton != null)
            actionButton.onClick.RemoveListener(AcceptCurrentMail);

        if (reportButton != null)
            reportButton.onClick.RemoveListener(RequestReport);

        if (outcomeSequence != null)
            outcomeSequence.Completed -= OnSequenceCompleted;
    }

    public void SelectMail(MailData mail)
    {
        SituationManager manager = SituationManager.Instance;

        if (IsBusy || mail == null || manager == null || !manager.IsReady)
            return;

        if (manager.GetSituationState(mail) !=
            SituationManager.SituationStateEnum.Active)
        {
            return;
        }

        CurrentMail = mail;
        reportMode = false;

        RenderMail();
        selectedMailOpened?.Invoke(mail);
    }

    public void ClearSelection()
    {
        if (IsBusy)
            return;

        CurrentMail = null;
        reportMode = false;

        RenderMail();
    }

    public void SetReportMode(bool open)
    {
        reportMode = open;
        RenderMail();
    }

    public void AcceptCurrentMail()
    {
        // Prevent accepting through the checklist overlay.
        if (reportMode)
            return;

        Resolve(MailDecision.Accept, null);
    }

    private void RequestReport()
    {
        if (CanDecide && !reportMode)
            ReportRequested?.Invoke();
    }

    public bool SubmitReport(IEnumerable<MailRedFlag> flags)
    {
        return reportMode && Resolve(MailDecision.Report, flags);
    }

    private bool Resolve(
        MailDecision decision,
        IEnumerable<MailRedFlag> flags)
    {
        if (!CanDecide)
            return false;

        SituationManager manager = SituationManager.Instance;

        if (!manager.TryResolveMail(
                CurrentMail,
                decision,
                flags,
                out MailResolution result))
        {
            return false;
        }

        // The manager has committed the result and locked further decisions.
        RenderMail();
        outcomeSequence.Begin(result);

        return true;
    }

    private void OnSequenceCompleted(MailResolution resolution)
    {
        SituationManager manager = SituationManager.Instance;

        if (manager == null ||
            !manager.CompleteMailPresentation(resolution))
        {
            return;
        }

        CurrentMail = null;
        reportMode = false;

        RenderMail();

        // Only now may the mailbox hide this email and select the next one.
        selectedMailFinished?.Invoke(resolution.Mail);
    }

    private void RenderMail()
    {
        if (selectedMailSender != null)
        {
            selectedMailSender.richText = false;

            selectedMailSender.text = CurrentMail == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(CurrentMail.DisplayName)
                    ? CurrentMail.EmailAddress
                    : $"{CurrentMail.DisplayName} <{CurrentMail.EmailAddress}>";
        }

        if (selectedMailSubject != null)
        {
            selectedMailSubject.richText = false;
            selectedMailSubject.text = CurrentMail != null
                ? CurrentMail.EmailHeader
                : string.Empty;
        }

        if (selectedMailBody != null)
        {
            selectedMailBody.text = CurrentMail != null
                ? string.Join("\n", CurrentMail.EmailBody)
                : string.Empty;
        }

        if (actionButtonText != null)
        {
            actionButtonText.text = CurrentMail != null
                ? CurrentMail.ActionButtonLabel
                : "ACTION";
        }

        bool buttonsEnabled = CanDecide && !reportMode;

        if (actionButton != null)
            actionButton.interactable = buttonsEnabled;

        if (reportButton != null)
            reportButton.interactable = buttonsEnabled;
    }

    public void Interact(GameObject interactor)
    {
        if (reportMode ||
            !CanDecide ||
            selectedMailBody == null ||
            !selectedMailBody.gameObject.activeInHierarchy ||
            Mouse.current == null)
        {
            return;
        }

        Canvas canvas = selectedMailBody.canvas;

        Camera uiCamera = null;

        if (canvas == null ||
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas != null && canvas.worldCamera != null
                ? canvas.worldCamera
                : Camera.main;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            selectedMailBody,
            Mouse.current.position.ReadValue(),
            uiCamera);

        if (linkIndex >= 0)
        {
            // In-body links use the same guarded Accept decision.
            // Never open a real browser or execute an attachment.
            AcceptCurrentMail();
        }
    }
}