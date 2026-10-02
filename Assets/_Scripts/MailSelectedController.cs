using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private MailData currentSelectedMail;

    [SerializeField] private TMP_Text selectedMailBody;

    [SerializeField] private Button deleteSelectedMail;
    [SerializeField] private Button replaySelectedMail;

    public UnityEvent<MailData> selectedMailOpened;
    public UnityEvent<MailData> selectedMailDeleted;
    public UnityEvent<MailData> selectedMailReplay;

    // Fires when player clicks a link
    public UnityEvent<string> selectedMailLinkClicked;

    private void Start()
    {
        if (this.currentSelectedMail == null)
        {
            this.selectedMailBody.text = string.Empty;
        }
        else
        {
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

    private void OnDeleteSelectedMail()
    {
        if (this.currentSelectedMail != null)
        {
            this.selectedMailDeleted?.Invoke(currentSelectedMail);
            this.currentSelectedMail = null;
            this.selectedMailBody.text = string.Empty;
        }
    }

    public void SelectMail(MailData mailData)
    {
        this.currentSelectedMail = mailData;
        this.selectedMailBody.text = string.Join("\n", mailData.EmailBody);
        this.selectedMailOpened?.Invoke(mailData);

        Debug.Log($"Selected mail data: {mailData.EmailHeader}");
    }

    // Link click logic
    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectedMailBody == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(selectedMailBody, eventData.position, eventData.pressEventCamera);

        if (linkIndex != -1) // no link
        {
            TMP_LinkInfo linkInfo = selectedMailBody.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            Debug.Log($"Player clicked a link with ID: {linkID}");

            // Tell eventManager
            selectedMailLinkClicked?.Invoke(linkID);
        }
    }
}