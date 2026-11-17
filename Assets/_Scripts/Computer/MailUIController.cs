using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailUIController : MonoBehaviour {
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
    // Left Sidebar Buttons
    [SerializeField] private Button sidebarInboxButton;
    [SerializeField] private Button sidebarSpoofedAddressButton;
    [SerializeField] private Button sidebarSuspiciousLinkButton;
    [SerializeField] private Button sidebarUrgencyButton;
    [SerializeField] private Button sidebarBadFileExtensionButton;
    [SerializeField] private Button sidebarAskingForCredentailsButton;
    //Button used to navigate back.
    [SerializeField] private Button backButton;
    [SerializeField] private Button[] closeCurrentWindowButton;

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

    [Header("Information Window")]
    [SerializeField] private RectTransform InformationPanel;
    [SerializeField] private RectTransform spoofedAddressContainer;
    [SerializeField] private RectTransform suspiciousLinkContainer;
    [SerializeField] private RectTransform urgencyContainer;
    [SerializeField] private RectTransform badFileExtensionContainer;
    [SerializeField] private RectTransform askingForCredentialsContainer;
    private Button CurrentActiveInformationButton = null;

    private bool Busy =>
        mailSelectedController != null &&
        mailSelectedController.IsBusy;

    private bool ReportOpen =>
        reportOverlay != null &&
        reportOverlay.activeSelf;

    private void Awake() {
        ResetChecklist();
        SetReportVisible(false);
        Show(MailState.Home);

        // Making sure the information panel is hidden at the start and that all the information containers are hidden as well.
        this.InformationPanel.gameObject.SetActive(false);
        this.spoofedAddressContainer.gameObject.SetActive(false);
        this.suspiciousLinkContainer.gameObject.SetActive(false);
        this.urgencyContainer.gameObject.SetActive(false);
        this.badFileExtensionContainer.gameObject.SetActive(false);
        this.askingForCredentialsContainer.gameObject.SetActive(false);
        this.CurrentActiveInformationButton = null;

    }

    private void OnEnable() {
        SetButtonListeners(true);

        if (mailSelectedController != null) {
            mailSelectedController.ReportRequested += OpenReport;
            mailSelectedController.selectedMailOpened.AddListener(
                OpenEmailReadView);
            mailSelectedController.selectedMailFinished.AddListener(
                OnMailFinished);
        }
    }

    private void OnDisable() {
        SetButtonListeners(false);

        if (mailSelectedController != null) {
            mailSelectedController.ReportRequested -= OpenReport;
            mailSelectedController.selectedMailOpened.RemoveListener(
                OpenEmailReadView);
            mailSelectedController.selectedMailFinished.RemoveListener(
                OnMailFinished);
        }

        CancelReport();
    }

    private void SetButtonListeners(bool subscribe) {
        // Sidebar Buttons
        Bind(sidebarInboxButton, OpenInbox, subscribe);

        Bind(sidebarSpoofedAddressButton, OnSidebarSpoofedButton, subscribe);
        Bind(sidebarSuspiciousLinkButton, OnSidebarSuspiciousButton, subscribe);
        Bind(sidebarUrgencyButton, OnSidebarUrgencyButton, subscribe);
        Bind(sidebarBadFileExtensionButton, OnSidebarBadFileExtensionButton, subscribe);
        Bind(sidebarAskingForCredentailsButton, OnSidebarAskingForCredentialsButton, subscribe);
        //

        Bind(backButton, Back, subscribe);

        foreach (Button button in closeCurrentWindowButton) {
            Bind(button, () => this.mailSelectedController.ComputerController.CloseCurrentOpenComputerWindow(), subscribe);
        }

        Bind(submitReportButton, SubmitReport, subscribe);
        Bind(cancelReportButton, CancelReport, subscribe);

        Bind(spoofedAddressButton, ToggleSpoofedAddress, subscribe);
        Bind(suspiciousLinkButton, ToggleSuspiciousLink, subscribe);
        Bind(urgencyFearButton, ToggleUrgencyFear, subscribe);
        Bind(badFileExtensionButton, ToggleBadFileExtension, subscribe);
        Bind(credentialsButton, ToggleCredentials, subscribe);
    }

    private void OnSidebarSpoofedButton() => OnInformationButton(sidebarSpoofedAddressButton);
    private void OnSidebarSuspiciousButton() => OnInformationButton(sidebarSuspiciousLinkButton);
    private void OnSidebarUrgencyButton() => OnInformationButton(sidebarUrgencyButton);
    private void OnSidebarBadFileExtensionButton() => OnInformationButton(sidebarBadFileExtensionButton);
    private void OnSidebarAskingForCredentialsButton() => OnInformationButton(sidebarAskingForCredentailsButton);

    private void OnInformationButton(Button button) {
        if (this.CurrentActiveInformationButton == null) {
            this.CurrentActiveInformationButton = button;
            //this.CurrentActiveInformationButton.interactable = false;
            InformationWindowState(this.CurrentActiveInformationButton, true);
            this.InformationPanel.gameObject.SetActive(true);

        //} 
        //else if (this.CurrentActiveInformationButton == button) { // If the same button is clicked again, close the information window.
        //    this.CurrentActiveInformationButton = null;
        //    //button.interactable = true;
        //    InformationWindowState(button, false);
        } else { // If a different button is clicked, close the previous information window and open the new one.

            // Close the previous information window
            InformationWindowState(this.CurrentActiveInformationButton, false);

            // Enable the previous button and make the new button non-interactable
            //this.CurrentActiveInformationButton.interactable = true;
            this.CurrentActiveInformationButton = button;
            //button.interactable = false;

            // Open the new information window
            InformationWindowState(this.CurrentActiveInformationButton, true);
            this.InformationPanel.gameObject.SetActive(true);

        }
        // debug log what button was pressed and what the current active button is.
        Debug.Log($"Information button pressed: {button.name}. CurrentActiveInformationButton: {this.CurrentActiveInformationButton?.name ?? "null"}");
    }

    private void InformationWindowState(Button button, bool active) {

        if (button == this.sidebarSuspiciousLinkButton) {
            this.suspiciousLinkContainer.gameObject.SetActive(active);

        } else if (button == this.sidebarSpoofedAddressButton) {
            this.spoofedAddressContainer.gameObject.SetActive(active);

        } else if (button == this.sidebarUrgencyButton) {
            this.sidebarSuspiciousLinkButton.gameObject.SetActive(active);

        } else if (button == this.sidebarBadFileExtensionButton) {
            this.badFileExtensionContainer.gameObject.SetActive(active);

        } else if (button == this.sidebarAskingForCredentailsButton) {
            this.askingForCredentialsContainer.gameObject.SetActive(active);
        }

    }

    private static void Bind(
        Button button,
        UnityAction callback,
        bool subscribe) {
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

    private void ToggleFlag(MailRedFlag flag) {
        if (!ReportOpen || Busy)
            return;

        if (!selectedFlags.Add(flag))
            selectedFlags.Remove(flag);

        RefreshCheckMarks();
    }

    private void RefreshCheckMarks() {
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

    private void SetCheckMark(GameObject checkMark, MailRedFlag flag) {
        if (checkMark != null)
            checkMark.SetActive(selectedFlags.Contains(flag));
    }

    private void ResetChecklist() {
        selectedFlags.Clear();
        RefreshCheckMarks();
    }

    private bool ReportReferencesAssigned() {
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

    private void OpenReport() {
        if (mailSelectedController == null ||
            !mailSelectedController.CanDecide) {
            return;
        }

        if (!ReportReferencesAssigned()) {
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

    private void SubmitReport() {
        if (!ReportOpen ||
            mailSelectedController == null ||
            !mailSelectedController.CanDecide) {
            return;
        }

        // Copy before clearing the UI selection.
        var flags = new List<MailRedFlag>(selectedFlags);

        SetReportVisible(false);

        bool submitted = mailSelectedController.SubmitReport(flags);

        mailSelectedController.SetReportMode(false);
        ResetChecklist();

        if (!submitted) {
            Debug.LogWarning(
                "Report was not submitted. The email may already be " +
                "resolved, inactive, or blocked by another sequence.",
                this);
        }
    }

    private void CancelReport() {
        if (mailSelectedController != null)
            mailSelectedController.SetReportMode(false);

        ResetChecklist();
        SetReportVisible(false);
    }

    private void SetReportVisible(bool visible) {
        if (reportOverlay != null)
            reportOverlay.SetActive(visible);
    }

    public void OpenHome() {
        if (Busy)
            return;

        CancelReport();

        if (mailSelectedController != null)
            mailSelectedController.ClearSelection();

        Show(MailState.Home);
    }

    public void OpenInbox() {
        if (Busy)
            return;

        CancelReport();

        if (mailSelectedController != null)
            mailSelectedController.ClearSelection();

        Show(MailState.InboxList);
    }

    public void OpenEmailReadView(MailData mail) {
        CancelReport();
        Show(MailState.ReadingInbox);
    }

    private void OnMailFinished(MailData finished) {
        OpenInbox();
    }

    private void Back() {
        if (Busy)
            return;

        if (ReportOpen) {
            CancelReport();
            return;
        }

        if (currentState == MailState.ReadingInbox)
            OpenInbox();
        else
            OpenHome();
    }

    private void Show(MailState state) {
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

    private void CloseMailWindow() {
        if (Busy ||
            mailSelectedController == null ||
            mailSelectedController.ComputerController == null) {
            return;
        }

        CancelReport();
        mailSelectedController.ComputerController.OnMailButton();
    }
}