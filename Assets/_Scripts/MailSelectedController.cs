using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour {
    [SerializeField] private Mail currentSelectedMail;

    [SerializeField] private TMP_Text selectedMailBody;
    // Buttons
    [SerializeField] private Button deleteSelectedMail;
    [SerializeField] private Button replaySelectedMail;

    public UnityEvent<Mail> selectedMailOpened;
    public UnityEvent<Mail> selectedMailDeleted;
    public UnityEvent<Mail> selectedMailReplay;

    private void Start() {
        if (this.currentSelectedMail == null) {
            this.selectedMailBody.text = string.Empty;
        } else {
            SelectMail(currentSelectedMail);
        }

        this.deleteSelectedMail.onClick.AddListener(OnDeleteSelectedMail);
        this.replaySelectedMail.onClick.AddListener(OnReplaySelectedMail);
    }

    private void OnReplaySelectedMail()
    {
        Debug.Log("Replaying selected mail");
        if (currentSelectedMail != null)
        {
            this.selectedMailReplay?.Invoke(currentSelectedMail);
        }
    }

    private void OnDeleteSelectedMail() {
        if (this.currentSelectedMail != null) {
            this.selectedMailDeleted?.Invoke(currentSelectedMail);
            this.currentSelectedMail = null;
            this.selectedMailBody.text = string.Empty;
        }
    }

    public void SelectMail(Mail mail) {
        this.currentSelectedMail = mail;
        this.selectedMailBody.text = mail.EmailBody;
        this.selectedMailOpened?.Invoke(mail);

        Debug.Log($"Selected mail: {mail.EmailSubject}");
    }
}
