using Assets._Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MeetingRoomTVController : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

    [SerializeField] Button ContinueButton;
    [SerializeField] Chair chair;
    [SerializeField] Chair workStationChair;

    [SerializeField] private MailData passwordExpiredScamMailData; // Reference to the specific mail data for the password expired scam email

    public UnityEvent OnContinueDay;
    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
    }

    private void OnDisable() {
        ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == ConditionTracker.ConditionEnum.FailedPasswordExpiredScam && state.ExpectedState) {
            this.workStationChair.StandUp();
            DisplayMail(passwordExpiredScamMailData, true);
            this.chair.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);
        }
    }

    /// <summary>
    /// Displays the mail. Set showAnnotated to true for the TV reveal, false for the normal PC view.
    /// </summary>
    public void DisplayMail(MailData mail, bool showAnnotated) {
        if (tvSenderAddressText != null)
            tvSenderAddressText.text = showAnnotated ? mail.AnnotatedEmailAddress : mail.EmailAddress;

        if (tvSubjectText != null)
            tvSubjectText.text = mail.EmailHeader;

        if (tvBodyText != null) {
            string[] bodyToUse = showAnnotated ? mail.AnnotatedEmailBody : mail.EmailBody;
            tvBodyText.text = string.Join("\n", bodyToUse);
        }
    }

    public void ContinueDayButton() {
        // Meeting is completed, set the condition to true
        OnContinueDay?.Invoke();
        chair.StandUp();

        Debug.Log("Continue button pressed! Continuing the day.");
    }
}