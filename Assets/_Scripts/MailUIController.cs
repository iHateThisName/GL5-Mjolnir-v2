using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailUIController : MonoBehaviour
{
    private enum MailState { Home, InboxList, ReadingInbox }

    private MailState currentState;

    private readonly HashSet<MailRedFlag> selectedFlags =
        new HashSet<MailRedFlag>();

    [SerializeField] private MailSelectedController mailSelectedController;

    [Header("Panels")]
    [SerializeField] private GameObject leftContainer;
    [SerializeField] private GameObject emailListPanel;
    [SerializeField] private GameObject emailPanel;

    [Header("Navigation")]
    [SerializeField] private Button sidebarInboxButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeTabButton;

    [Header("Report Overlay")]
    [SerializeField] private GameObject reportOverlay;
    [SerializeField] private Button submitReportButton;
    [SerializeField] private Button cancelReportButton;

    [Header("Spoofed Address")]
    [SerializeField] private Button spoofedAddressButton;
    [SerializeField] private GameObject spoofedAddressCheckMark;

    [Header("Suspicious Link URL")]
    [SerializeField] private Button suspiciousLinkButton;
    [SerializeField] private GameObject suspiciousLinkCheckMark;

    [Header("Urgency / Fear")]
    [SerializeField] private Button urgencyFearButton;
    [SerializeField] private GameObject urgencyFearCheckMark;

    [Header("Bad File Extension")]
    [SerializeField] private Button badFileExtensionButton;
    [SerializeField] private GameObject badFileExtensionCheckMark;

    [Header("Asking for Credentials")]
    [SerializeField] private Button credentialsButton;
    [SerializeField] private GameObject credentialsCheckMark;

    private bool Busy =>
        mailSelectedController != null &&
        mailSelectedController.IsBusy;

    private bool ReportOpen =>
        reportOverlay != null &&
        reportOverlay.activeSelf;

    private void Awake()
    {
        ResetChecklist();
        SetReportVisible(false);
        Show(MailState.Home);
    }

    private void OnEnable()
    {
        SetButtonListeners(true);

        if (mailSelectedController != null)
        {
            mailSelectedController.ReportRequested += OpenReport;
            mailSelectedController.selectedMailOpened.AddListener(
                OpenEmailReadView);
            mailSelectedController.selectedMailFinished.AddListener(
                OnMailFinished);
        }
    }

    private void OnDisable()
    {
        SetButtonListeners(false);

        if (mailSelectedController != null)
        {
            mailSelectedController.ReportRequested -= OpenReport;
            mailSelectedController.selectedMailOpened.RemoveListener(
                OpenEmailReadView);
            mailSelectedController.selectedMailFinished.RemoveListener(
                OnMailFinished);
        }

        CancelReport();
    }

    private void SetButtonListeners(bool subscribe)
    {
        Bind(sidebarInboxButton, OpenInbox, subscribe);
        Bind(backButton, Back, subscribe);
        Bind(closeTabButton, CloseMailWindow, subscribe);

        Bind(submitReportButton, SubmitReport, subscribe);
        Bind(cancelReportButton, CancelReport, subscribe);

        Bind(spoofedAddressButton, ToggleSpoofedAddress, subscribe);
        Bind(suspiciousLinkButton, ToggleSuspiciousLink, subscribe);
        Bind(urgencyFearButton, ToggleUrgencyFear, subscribe);
        Bind(badFileExtensionButton, ToggleBadFileExtension, subscribe);
        Bind(credentialsButton, ToggleCredentials, subscribe);
    }

    private static void Bind(
        Button button,
        UnityAction callback,
        bool subscribe)
    {
        if (button == null)
            return;

        // Remove only this controller's listener, not unrelated listeners.
        button.onClick.RemoveListener(callback);

        if (subscribe)
            button.onClick.AddListener(callback);
    }

    private void ToggleSpoofedAddress() =>
        ToggleFlag(MailRedFlag.SpoofedAddress);

    private void ToggleSuspiciousLink() =>
        ToggleFlag(MailRedFlag.SuspiciousLinkUrl);

    private void ToggleUrgencyFear() =>
        ToggleFlag(MailRedFlag.UrgencyOrFear);

    private void ToggleBadFileExtension() =>
        ToggleFlag(MailRedFlag.BadFileExtension);

    private void ToggleCredentials() =>
        ToggleFlag(MailRedFlag.AskingForCredentials);

    private void ToggleFlag(MailRedFlag flag)
    {
        if (!ReportOpen || Busy)
            return;

        if (!selectedFlags.Add(flag))
            selectedFlags.Remove(flag);

        RefreshCheckMarks();
    }

    private void RefreshCheckMarks()
    {
        SetCheckMark(
            spoofedAddressCheckMark,
            MailRedFlag.SpoofedAddress);

        SetCheckMark(
            suspiciousLinkCheckMark,
            MailRedFlag.SuspiciousLinkUrl);

        SetCheckMark(
            urgencyFearCheckMark,
            MailRedFlag.UrgencyOrFear);

        SetCheckMark(
            badFileExtensionCheckMark,
            MailRedFlag.BadFileExtension);

        SetCheckMark(
            credentialsCheckMark,
            MailRedFlag.AskingForCredentials);
    }

    private void SetCheckMark(GameObject checkMark, MailRedFlag flag)
    {
        if (checkMark != null)
            checkMark.SetActive(selectedFlags.Contains(flag));
    }

    private void ResetChecklist()
    {
        selectedFlags.Clear();
        RefreshCheckMarks();
    }

    private bool ReportReferencesAssigned()
    {
        return reportOverlay != null &&
               submitReportButton != null &&
               cancelReportButton != null &&
               spoofedAddressButton != null &&
               spoofedAddressCheckMark != null &&
               suspiciousLinkButton != null &&
               suspiciousLinkCheckMark != null &&
               urgencyFearButton != null &&
               urgencyFearCheckMark != null &&
               badFileExtensionButton != null &&
               badFileExtensionCheckMark != null &&
               credentialsButton != null &&
               credentialsCheckMark != null;
    }

    private void OpenReport()
    {
        if (mailSelectedController == null ||
            !mailSelectedController.CanDecide)
        {
            return;
        }

        if (!ReportReferencesAssigned())
        {
            Debug.LogError(
                "Assign the report overlay, submit/cancel buttons, " +
                "and all five checklist buttons and checkmarks.",
                this);
            return;
        }

        ResetChecklist();
        mailSelectedController.SetReportMode(true);
        SetReportVisible(true);
    }

    private void SubmitReport()
    {
        if (!ReportOpen ||
            mailSelectedController == null ||
            !mailSelectedController.CanDecide)
        {
            return;
        }

        // Copy before clearing the UI selection.
        var flags = new List<MailRedFlag>(selectedFlags);

        SetReportVisible(false);

        bool submitted = mailSelectedController.SubmitReport(flags);

        mailSelectedController.SetReportMode(false);
        ResetChecklist();

        if (!submitted)
        {
            Debug.LogWarning(
                "Report was not submitted. The email may already be " +
                "resolved, inactive, or blocked by another sequence.",
                this);
        }
    }

    private void CancelReport()
    {
        if (mailSelectedController != null)
            mailSelectedController.SetReportMode(false);

        ResetChecklist();
        SetReportVisible(false);
    }

    private void SetReportVisible(bool visible)
    {
        if (reportOverlay != null)
            reportOverlay.SetActive(visible);
    }

    public void OpenHome()
    {
        if (Busy)
            return;

        CancelReport();

        if (mailSelectedController != null)
            mailSelectedController.ClearSelection();

        Show(MailState.Home);
    }

    public void OpenInbox()
    {
        if (Busy)
            return;

        CancelReport();

        if (mailSelectedController != null)
            mailSelectedController.ClearSelection();

        Show(MailState.InboxList);
    }

    public void OpenEmailReadView(MailData mail)
    {
        CancelReport();
        Show(MailState.ReadingInbox);
    }

    private void OnMailFinished(MailData finished)
    {
        OpenInbox();
    }

    private void Back()
    {
        if (Busy)
            return;

        if (ReportOpen)
        {
            CancelReport();
            return;
        }

        if (currentState == MailState.ReadingInbox)
            OpenInbox();
        else
            OpenHome();
    }

    private void Show(MailState state)
    {
        currentState = state;

        if (leftContainer != null)
            leftContainer.SetActive(true);

        if (emailListPanel != null)
            emailListPanel.SetActive(state == MailState.InboxList);

        if (emailPanel != null)
            emailPanel.SetActive(state == MailState.ReadingInbox);

        if (backButton != null)
            backButton.gameObject.SetActive(state != MailState.Home);
    }

    private void CloseMailWindow()
    {
        if (Busy ||
            mailSelectedController == null ||
            mailSelectedController.ComputerController == null)
        {
            return;
        }

        CancelReport();
        mailSelectedController.ComputerController.OnMailButton();
    }
}