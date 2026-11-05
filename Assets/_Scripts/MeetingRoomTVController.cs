// --- MeetingRoomTVController.cs ---

using TMPro;
using UnityEngine;

public class MeetingRoomTVController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text tvSenderAddressText;
    [SerializeField] private TMP_Text tvSubjectText;
    [SerializeField] private TMP_Text tvBodyText;

    // This is now purely a UI update script. It holds no specific mail data.
    public void DisplayMail(MailData mail, bool showAnnotated)
    {
        if (mail == null)
        {
            Debug.LogWarning("MeetingRoomTVController received no email to display.", this);
            return;
        }

        if (this.tvSenderAddressText != null)
        {
            this.tvSenderAddressText.richText = showAnnotated;
            this.tvSenderAddressText.text = showAnnotated ? mail.AnnotatedEmailAddress : mail.EmailAddress;
        }

        if (this.tvSubjectText != null)
        {
            this.tvSubjectText.richText = false;
            this.tvSubjectText.text = mail.EmailHeader;
        }

        if (this.tvBodyText != null)
        {
            this.tvBodyText.richText = true;
            var bodyToUse = showAnnotated ? mail.AnnotatedEmailBody : mail.EmailBody;
            this.tvBodyText.text = string.Join("\n", bodyToUse);
        }
    }
}