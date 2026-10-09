using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour, IInteractable {
    [SerializeField] private MailData currentSelectedMail;

    [Header("Main Email Panel UI")]
    [SerializeField] private TMP_Text selectedMailSender;
    [SerializeField] private TMP_Text selectedMailSubject;
    [SerializeField] private TMP_Text selectedMailBody;

    [Header("Deleted Panel UI")]
    [SerializeField] private TMP_Text deletedPanelBodyText; // NEW: The text component in your Deleted Panel

    [Header("UI Buttons")]
    [SerializeField] private Button deleteSelectedMail;
    [SerializeField] private Button replaySelectedMail;

    public UnityEvent<MailData> selectedMailOpened;
    public UnityEvent<MailData> selectedMailDeleted;
    public UnityEvent<MailData> selectedMailReplay;
    public UnityEvent<string> selectedMailLinkClicked;

    private void Start() {
        if (this.currentSelectedMail == null) ClearMailUI();
        else SelectMail(currentSelectedMail);

        this.deleteSelectedMail?.onClick.AddListener(OnDeleteSelectedMail);
        this.replaySelectedMail?.onClick.AddListener(OnReplaySelectedMail);
    }

    private void OnReplaySelectedMail() {
        if (currentSelectedMail != null) {
            currentSelectedMail.SituationStateEnum = currentSelectedMail.IsReplyCorrect
                ? SituationManager.SituationStateEnum.Success
                : SituationManager.SituationStateEnum.Failed;

            SituationManager.Instance.SetSituationState(currentSelectedMail.SituationStateEnum, currentSelectedMail);
            this.selectedMailReplay?.Invoke(currentSelectedMail);

            ClearMailUI();
            this.currentSelectedMail = null;
        }
    }

    private void OnDeleteSelectedMail() {
        if (this.currentSelectedMail != null) {
            // Send the annotated text to the Deleted Panel instantly
            if (this.deletedPanelBodyText != null) {
                this.deletedPanelBodyText.text = currentSelectedMail.AnnotatedEmailBody != null && currentSelectedMail.AnnotatedEmailBody.Length > 0
                    ? string.Join("\n", currentSelectedMail.AnnotatedEmailBody)
                    : "No annotated text available.";
            }

            currentSelectedMail.SituationStateEnum = currentSelectedMail.IsDeleteCorrect
                ? SituationManager.SituationStateEnum.Success
                : SituationManager.SituationStateEnum.Failed;

            SituationManager.Instance.SetSituationState(currentSelectedMail.SituationStateEnum, currentSelectedMail);
            this.selectedMailDeleted?.Invoke(currentSelectedMail);

            ClearMailUI();
            this.currentSelectedMail = null;
        }
    }

    public void SelectMail(MailData mailData) {
        this.currentSelectedMail = mailData;

        if (this.selectedMailSender != null) this.selectedMailSender.text = mailData.EmailAddress;
        if (this.selectedMailSubject != null) this.selectedMailSubject.text = mailData.EmailHeader;
        if (this.selectedMailBody != null) {
            this.selectedMailBody.text = mailData.EmailBody != null ? string.Join("\n", mailData.EmailBody) : "No body text found.";
        }

        if (this.deleteSelectedMail != null) this.deleteSelectedMail.gameObject.SetActive(true);
        if (this.replaySelectedMail != null) this.replaySelectedMail.gameObject.SetActive(true);

        this.selectedMailOpened?.Invoke(mailData);
    }

    private void ClearMailUI() {
        if (this.selectedMailSender != null) this.selectedMailSender.text = string.Empty;
        if (this.selectedMailSubject != null) this.selectedMailSubject.text = string.Empty;
        if (this.selectedMailBody != null) this.selectedMailBody.text = string.Empty;
    }

    public void Interact(GameObject interactor) {
        if (selectedMailBody == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(selectedMailBody, mousePosition, Camera.main);

        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = selectedMailBody.textInfo.linkInfo[linkIndex];
            selectedMailLinkClicked?.Invoke(linkInfo.GetLinkID());
        }
    }

    public void OnReport(bool isSuspiciousLinkReported, bool isUnknownSenderReported, bool isTimePressureReported, bool isSpellingErrorsReported) {
        SituationManager.Instance.ReportScam(this.currentSelectedMail, isSuspiciousLinkReported, isUnknownSenderReported, isTimePressureReported, isSpellingErrorsReported);
        OnDeleteSelectedMail();
        //currentSelectedMail
    }
}