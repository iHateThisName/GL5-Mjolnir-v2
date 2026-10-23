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
        if (ConditionTracker.Instance != null) {
            ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
        }
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
    public void DisplayMail(MailData mail, bool showAnnotated)
    {
        if (mail == null)
        {
            Debug.LogWarning(
                "MeetingRoomTVController received no email to display.",
                this);
            return;
        }

        if (tvSenderAddressText != null)
        {
            tvSenderAddressText.richText = showAnnotated;
            tvSenderAddressText.text = showAnnotated
                ? mail.AnnotatedEmailAddress
                : mail.EmailAddress;
        }

        if (tvSubjectText != null)
        {
            tvSubjectText.richText = false;
            tvSubjectText.text = mail.EmailHeader;
        }

        if (tvBodyText != null)
        {
            tvBodyText.richText = true;

            var bodyToUse = showAnnotated
                ? mail.AnnotatedEmailBody
                : mail.EmailBody;

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