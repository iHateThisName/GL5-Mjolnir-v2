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
        if (this.selectedMailBody != null)
        {
            if (this.currentSelectedMail == null)
            {
                this.selectedMailBody.text = string.Empty;
            }
            else
            {
                SelectMail(currentSelectedMail);
            }
        }
        else
        {
            Debug.LogWarning("Selected Mail Body is missing in the Inspector!");
        }

        this.deleteSelectedMail?.onClick.AddListener(OnDeleteSelectedMail);
        this.replaySelectedMail?.onClick.AddListener(OnReplaySelectedMail);
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

        // Check if this email has already been processed/deleted
        bool isHandled = mailData.SituationStateEnum == SituationManager.SituationStateEnum.Success ||
                         mailData.SituationStateEnum == SituationManager.SituationStateEnum.Failed;

        if (isHandled)
        {
            // Show annotated text and hide action buttons
            this.selectedMailBody.text = string.Join("\n", mailData.AnnotatedEmailBody);
            this.deleteSelectedMail.gameObject.SetActive(false);
            this.replaySelectedMail.gameObject.SetActive(false);
        }
        else
        {
            // Show normal text and allow actions
            this.selectedMailBody.text = string.Join("\n", mailData.EmailBody);
            this.deleteSelectedMail.gameObject.SetActive(true);
            this.replaySelectedMail.gameObject.SetActive(true);
        }

        this.selectedMailOpened?.Invoke(mailData);
        Debug.Log($"Selected mail data: {mailData.EmailHeader}. Handled: {isHandled}");
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