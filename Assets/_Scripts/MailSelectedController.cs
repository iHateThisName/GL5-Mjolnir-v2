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
        if (currentSelectedMail == null) {
            this.selectedMailBody.text = string.Empty;
        } else {
            SelectMail(currentSelectedMail);
        }

        deleteSelectedMail.onClick.AddListener(OnDeleteSelectedMail);
        replaySelectedMail.onClick.AddListener(OnReplaySelectedMail);
    }

    private void OnReplaySelectedMail() {
        Debug.Log("Replaying selected mail");
    }

    private void OnDeleteSelectedMail() {
        if (currentSelectedMail != null) {
            selectedMailDeleted?.Invoke(currentSelectedMail);
            currentSelectedMail = null;
            selectedMailBody.text = string.Empty;
        }
    }

    public void SelectMail(Mail mail) {
        this.currentSelectedMail = mail;
        selectedMailBody.text = mail.EmailBody;
        selectedMailOpened?.Invoke(mail);

        Debug.Log($"Selected mail: {mail.EmailSubject}");
    }
}
