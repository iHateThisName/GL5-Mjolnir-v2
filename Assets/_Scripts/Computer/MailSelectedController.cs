using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour
{
    [field: SerializeField]
    public ComputerController ComputerController { get; private set; }

    [SerializeField] private MailOutcomeSequence outcomeSequence;

    [Header("Main Email Panel UI")]
    [SerializeField] private TMP_Text selectedMailSender;
    [SerializeField] private TMP_Text selectedMailSubject;
    [SerializeField] private TMP_Text selectedMailBody;

    [Header("Link Handling")]
    [Tooltip("Attach the TextLinkHandler that sits on the selectedMailBody TMP_Text component.")]
    [SerializeField] private TextLinkHandler bodyLinkHandler;

    [Header("Attachment UI")]
    [Tooltip("The static button sitting at the bottom of the email content.")]
    [SerializeField] private Button attachmentButton;
    [SerializeField] private TMP_Text attachmentButtonText;

    [Header("Primary Decisions")]
    [SerializeField] private Button reportButton;
    [SerializeField] private UnityEngine.UI.ScrollRect emailScrollView;

    public UnityEvent<MailData> selectedMailOpened = new UnityEvent<MailData>();
    public UnityEvent<MailData> selectedMailFinished = new UnityEvent<MailData>();

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
        SituationManager.Instance.GetSituationState(CurrentMail) == SituationManager.SituationStateEnum.Active;

    private void Awake()
    {
        if (reportButton != null)
            reportButton.onClick.AddListener(RequestReport);

        if (attachmentButton != null)
            attachmentButton.onClick.AddListener(AcceptCurrentMail);

        if (outcomeSequence != null)
            outcomeSequence.Completed += OnSequenceCompleted;

        if (bodyLinkHandler != null)
            bodyLinkHandler.OnLinkClickedEvent += HandleBodyLinkClicked;

        if (outcomeSequence == null || !outcomeSequence.IsConfigured || reportButton == null)
        {
            Debug.LogError("MailSelectedController requires report button and a fully configured MailOutcomeSequence.", this);
        }

        RenderMail();
    }

    private void OnDestroy()
    {
        if (reportButton != null)
            reportButton.onClick.RemoveListener(RequestReport);

        if (attachmentButton != null)
            attachmentButton.onClick.RemoveListener(AcceptCurrentMail);

        if (outcomeSequence != null)
            outcomeSequence.Completed -= OnSequenceCompleted;

        if (bodyLinkHandler != null)
            bodyLinkHandler.OnLinkClickedEvent -= HandleBodyLinkClicked;
    }

    public void SelectMail(MailData mail)
    {
        SituationManager manager = SituationManager.Instance;

        if (IsBusy || mail == null || manager == null || !manager.IsReady)
            return;

        if (manager.GetSituationState(mail) != SituationManager.SituationStateEnum.Active)
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

    private void HandleBodyLinkClicked(string linkId)
    {
        // Clicking ANY valid body link registers as an action/acceptance
        AcceptCurrentMail();
    }

    public void AcceptCurrentMail()
    {
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

    private bool Resolve(MailDecision decision, IEnumerable<MailRedFlag> flags)
    {
        if (!CanDecide)
            return false;

        SituationManager manager = SituationManager.Instance;

        if (!manager.TryResolveMail(CurrentMail, decision, flags, out MailResolution result))
        {
            return false;
        }

        RenderMail();
        outcomeSequence.Begin(result);

        return true;
    }

    private void OnSequenceCompleted(MailResolution resolution)
    {
        SituationManager manager = SituationManager.Instance;

        if (manager == null || !manager.CompleteMailPresentation(resolution))
        {
            return;
        }

        CurrentMail = null;
        reportMode = false;

        RenderMail();
        selectedMailFinished?.Invoke(resolution.Mail);
    }

    private void RenderMail()
    {
        if (emailScrollView != null)
        {
            emailScrollView.normalizedPosition = new Vector2(0, 1);
        }

        if (selectedMailSender != null)
        {
            // This MUST be true now so it can read the <link> tag
            selectedMailSender.richText = true;

            if (CurrentMail == null)
            {
                selectedMailSender.text = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(CurrentMail.DisplayName))
            {
                // If there is no display name, just show the email normally
                selectedMailSender.text = CurrentMail.EmailAddress;
            }
            else
            {
                // Wraps the display name in a link tag. 
                // E.g., <link="Hover_hacker@evil.com">Boss</link>
                selectedMailSender.text = $"<link=\"Hover_{CurrentMail.EmailAddress}\">{CurrentMail.DisplayName}</link>";
            }
        }

        if (selectedMailSubject != null)
        {
            selectedMailSubject.richText = false;
            selectedMailSubject.text = CurrentMail != null ? CurrentMail.EmailHeader : string.Empty;
        }

        if (selectedMailBody != null)
        {
            selectedMailBody.text = CurrentMail != null ? string.Join("\n", CurrentMail.EmailBody) : string.Empty;
        }

        bool buttonsEnabled = CanDecide && !reportMode;

        if (reportButton != null)
            reportButton.interactable = buttonsEnabled;

        RenderAttachment(buttonsEnabled);
    }

    private void RenderAttachment(bool buttonsEnabled)
    {
        if (attachmentButton == null) return;

        bool hasAttachment = CurrentMail != null &&
                             CurrentMail.AttachmentFiles != null &&
                             CurrentMail.AttachmentFiles.Count > 0;

        attachmentButton.gameObject.SetActive(hasAttachment);

        if (hasAttachment)
        {
            if (attachmentButtonText != null)
            {
                attachmentButtonText.text = CurrentMail.AttachmentFiles[0];
            }
            attachmentButton.interactable = buttonsEnabled;
        }
    }
}