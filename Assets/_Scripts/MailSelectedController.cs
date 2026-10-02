using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MailSelectedController : MonoBehaviour, IInteractable
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

        if (this.currentSelectedMail.IsReplyCorrect) {
            SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Success, this.currentSelectedMail);
        } else {
            SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Failed, this.currentSelectedMail);

        }
    }

    private void OnDeleteSelectedMail()
    {
        if (this.currentSelectedMail != null)
        {
            if (this.currentSelectedMail.IsDeleteCorrect) {
                SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Success, this.currentSelectedMail);
            } else {
                SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Failed, this.currentSelectedMail);
            }
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
    public void Interact(GameObject interactor)
    {
        if (selectedMailBody == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(selectedMailBody, mousePosition, Camera.main);

        if (linkIndex != -1)
        {
            // Extract the ID and fire the event
            TMP_LinkInfo linkInfo = selectedMailBody.textInfo.linkInfo[linkIndex];
            string linkID = linkInfo.GetLinkID();

            selectedMailLinkClicked?.Invoke(linkID);
        }
    }
}