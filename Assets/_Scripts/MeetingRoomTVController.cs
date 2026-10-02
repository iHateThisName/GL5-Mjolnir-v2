using TMPro;
using UnityEngine;

public class MeetingRoomTVController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

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