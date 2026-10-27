using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailController : MonoBehaviour
{
    [SerializeField] private Image mailIcon;
    // Original field spelling retained to preserve the prefab reference.
    [SerializeField] private TMP_Text mailAddreass;
    [SerializeField] private TMP_Text mailSubject;
    public Button mailButton;

    private void Awake()
    {
        if (mailButton == null) mailButton = GetComponent<Button>();
    }

    public void Initialize(MailData mail)
    {
        if (mail == null) return;
        if (mailAddreass != null)
        {
            mailAddreass.richText = false;
            mailAddreass.text = string.IsNullOrWhiteSpace(mail.DisplayName)
                ? mail.EmailAddress : $"{mail.DisplayName}\n<{mail.EmailAddress}>";
        }
        if (mailSubject != null)
        {
            mailSubject.richText = false;
            mailSubject.text = mail.EmailHeader;
        }
    }

    // Preserve the old method spelling for callers using MailData.
    public void Initilize(MailData mail) => Initialize(mail);
}
