using Assets._Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeetingRoomTVController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

    [SerializeField] Button ContinueButton;
    [SerializeField] Chair chair;
    [SerializeField] Chair workStationChair;

    [SerializeField] private MailData passwordExpiredScamMailData; // Reference to the specific mail data for the password expired scam email

    private void OnEnable() {
        SituationManager.Instance.OnSituationStateChange += OnSituationStateChanged;
    }
    
    private void OnDisable() {
        SituationManager.Instance.OnSituationStateChange -= OnSituationStateChanged;
    }

    private void OnSituationStateChanged(SituationData data) {

        // Check if the situation data is of type MailData and matches the specific mail data for the password expired scam email
        if (data is MailData mailData) {

            if (this.passwordExpiredScamMailData == mailData) {

                if (mailData.SituationStateEnum == SituationManager.SituationStateEnum.Success) {

                    // Success

                    //DisplayMail(mailData, true);

                } else if (mailData.SituationStateEnum == SituationManager.SituationStateEnum.Failed) {

                    // Failed
                    this.workStationChair.StandUp();
                    DisplayMail(mailData, true);
                    this.chair.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);

                }
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

    public void ContinueDayButton()
    {
        ConditionTracker.ConditionState conditionState = new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.hasCompletedMeeting, true);
        ConditionTracker.Instance.SetCondition(conditionState);
        Debug.Log("Clicked");
        chair.StandUp();
    }
}