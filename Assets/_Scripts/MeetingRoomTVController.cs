using System;
using TMPro;
using UnityEngine;

public class MeetingRoomTVController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

    [SerializeField] private MailData passwordExpiredScamMailData; // Reference to the specific mail data for the password expired scam email

    private void OnEnable() {
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChanged;
    }
    
    private void OnDisable() {
        SituationManager.Instance.OnSituationStateChange -= OnSituationStateChanged;
    }

    private void OnSituationStateChanged(SituationData data) {

        // Check if the situation data is of type MailData and matches the specific mail data for the password expired scam email
        if (data is MailData mailData && this.passwordExpiredScamMailData == mailData) {

            if (mailData.SituationStateEnum == SituationManager.SituationStateEnum.Success) {

                // Success

                DisplayMail(mailData, true);

            } else if (mailData.SituationStateEnum == SituationManager.SituationStateEnum.Failed) {

                // Failed

            }
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