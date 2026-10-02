using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour {
    [SerializeField] private MailData currentSelectedMail;

    [SerializeField] private TMP_Text selectedMailBody;
    // Buttons
    [SerializeField] private Button deleteSelectedMail;
    [SerializeField] private Button replaySelectedMail;

    public UnityEvent<MailData> selectedMailOpened;
    public UnityEvent<MailData> selectedMailDeleted;
    public UnityEvent<MailData> selectedMailReplay;

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

    public void SelectMail(MailData mailData) {
        this.currentSelectedMail = mailData;
        this.selectedMailBody.text = string.Join("\n", mailData.EmailBody);
        this.selectedMailOpened?.Invoke(mailData);

        Debug.Log($"Selected mail data: {mailData.EmailHeader}");
    }
}
