using Assets._Scripts;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class ScamFailureSequence : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private DialogueSystem npcDialogue;
    [SerializeField] private MeetingRoomTVController tvController;

    [Header("Cameras & Polish")]
    [SerializeField] private CinemachineCamera meetingWideCamera;
    [SerializeField] private CinemachineCamera tvZoomCamera;
    [SerializeField] private GameObject meetingNpcsContainer;

    [Header("Timing")]
    [SerializeField] private float standUpDelay = 1.0f;
    [SerializeField] private float delayBeforeFade = 1.5f;
    [SerializeField] private float stareDuration = 1.5f;

    private bool isSequenceRunning;
    private bool hasFinishedDialogue;

    private void OnEnable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged += this.OnConditionStateChanged;
        }
    }

    private void OnDisable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged -= this.OnConditionStateChanged;
        }
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state)
    {
        if (state.Condition == ConditionTracker.ConditionEnum.HasBeenScammed && state.ExpectedState)
        {
            _ = this.RunFailureSequenceAsync();
        }
    }

    private void OnDialogueEnded()
    {
        this.hasFinishedDialogue = true;
    }

    private async Awaitable RunFailureSequenceAsync()
    {
        if (this.isSequenceRunning) return;

        this.isSequenceRunning = true;
        this.hasFinishedDialogue = false;

        try
        {
            Chair chairToReturnTo = Chair.ActiveChair;

            if (chairToReturnTo != null)
            {
                chairToReturnTo.StandUp();
            }

            await Awaitable.WaitForSecondsAsync(this.standUpDelay);

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Cutscene;
            await Awaitable.WaitForSecondsAsync(this.delayBeforeFade);

            this.fadeCanvas.gameObject.SetActive(true);
            this.fadeCanvas.alpha = 0f;
            await this.fadeCanvas.DOFade(1f, 1f).AsyncWaitForCompletion();

            // We will store the dialogue here when we grab the mail
            SituationData activeDialogue = null;

            if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
            {
                MailData failedMail = SituationManager.Instance.PendingMailPresentation.Mail;
                if (this.tvController != null) this.tvController.DisplayMail(failedMail, true);

                // Grab the specific dialogue for this scam!
                activeDialogue = failedMail.ScoldingDialogue;
            }

            if (this.meetingWideCamera != null) this.meetingWideCamera.Priority = 50;
            if (this.meetingNpcsContainer != null) this.meetingNpcsContainer.SetActive(true);

            await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
            this.fadeCanvas.gameObject.SetActive(false);

            await Awaitable.WaitForSecondsAsync(this.stareDuration);

            if (this.tvZoomCamera != null) this.tvZoomCamera.Priority = 100;

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Dialogue;

            // Play the dynamic dialogue
            if (activeDialogue != null)
            {
                if (this.npcDialogue != null) this.npcDialogue.OnDialogueEnded += this.OnDialogueEnded;
                this.npcDialogue.SetDialogue(activeDialogue, false);
            }
            else
            {
                Debug.LogWarning("No scolding dialogue data assigned to this email, skipping dialogue wait.");
                this.hasFinishedDialogue = true;
            }

            while (!this.hasFinishedDialogue)
            {
                await Awaitable.NextFrameAsync();
            }
            if (this.npcDialogue != null) this.npcDialogue.OnDialogueEnded -= this.OnDialogueEnded;

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Cutscene;
            this.fadeCanvas.gameObject.SetActive(true);
            await this.fadeCanvas.DOFade(1f, 1f).AsyncWaitForCompletion();

            if (this.meetingWideCamera != null) this.meetingWideCamera.Priority = 0;
            if (this.tvZoomCamera != null) this.tvZoomCamera.Priority = 0;
            if (this.meetingNpcsContainer != null) this.meetingNpcsContainer.SetActive(false);

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Walking;

            if (chairToReturnTo != null)
            {
                chairToReturnTo.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);
            }

            if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
            {
                SituationManager.Instance.CompleteMailPresentation(SituationManager.Instance.PendingMailPresentation);
            }

            ConditionTracker.Instance.SetCondition(new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.HasBeenScammed, false));

            await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
            this.fadeCanvas.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError($"CRITICAL ERROR in ScamSequence: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            this.isSequenceRunning = false;
        }
    }
}