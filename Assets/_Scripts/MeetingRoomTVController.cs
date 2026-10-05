using System;
using TMPro;
using UnityEngine;

public class MeetingRoomTVController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

    private void OnEnable() {
        ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
    }
    
    private void OnDisable() {
        ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == ConditionTracker.ConditionEnum.FailedPasswordExpiredScam && state.ExpectedState) {
            Debug.Log("MeetingRoomTVController: Displaying password expired scam email on TV.");
        }
    }

    /// <summary>
    /// Displays the mail. Set showAnnotated to true for the TV reveal, false for the normal PC view.
    /// </summary>
    public void DisplayMail(MailData mail, bool showAnnotated)
    {
        if (tvSenderAddressText != null)
            tvSenderAddressText.text = showAnnotated ? mail.AnnotatedEmailAddress : mail.EmailAddress;

        if (tvSubjectText != null)
            tvSubjectText.text = mail.EmailHeader;

        if (tvBodyText != null)
        {
            string[] bodyToUse = showAnnotated ? mail.AnnotatedEmailBody : mail.EmailBody;
            tvBodyText.text = string.Join("\n", bodyToUse);
        }
    }
}