using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailController : MonoBehaviour
{
    [SerializeField] private Image mailIcon;
    [SerializeField] private TMP_Text mailAddreass;
    [SerializeField] private TMP_Text mailSubject;
    public Button mailButton;

    private void Awake()
    {
        if (this.mailButton == null)
        {
            this.mailButton = GetComponent<Button>();
        }
    }

    public void Initilize(Mail mail)
    {
        //mailIcon.sprite = mail.Icon;
        mailAddreass.text = mail.EmailAddress;
        mailSubject.text = mail.EmailSubject;
    }

    public void Initilize(MailData mailData)
    {
        // Added defensive checks just in case the Inspector links are empty
        if (mailAddreass != null) mailAddreass.text = mailData.EmailAddress;
        if (mailSubject != null) mailSubject.text = mailData.EmailHeader;
    }
}