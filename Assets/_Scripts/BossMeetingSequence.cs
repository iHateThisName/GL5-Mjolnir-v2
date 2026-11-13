using Assets._Scripts;
using DG.Tweening;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class BossMeetingSequence : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private DialogueSystem npcDialogue;
    [Tooltip("Fallback dialogue if the specific email has no dialogue assigned")]
    [SerializeField] private SituationData genericBossScoldingData;

    [Header("Cameras & Polish")]
    [Tooltip("The camera looking at the Boss in their office")]
    [SerializeField] private CinemachineCamera bossOfficeCamera;

    [Header("Timing")]
    [SerializeField] private float standUpDelay = 1.0f;
    [SerializeField] private float delayBeforeFade = 1.5f;

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
        if (state.Condition == ConditionTracker.ConditionEnum.HasFalselyReported && state.ExpectedState)
        {
            MailData failedMail = null;
            if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
            {
                failedMail = SituationManager.Instance.PendingMailPresentation.Mail;
            }

            _ = this.RunBossSequenceAsync(failedMail);
        }
    }

    private void OnDialogueEnded()
    {
        this.hasFinishedDialogue = true;
    }

    private async Awaitable RunBossSequenceAsync(MailData failedMail)
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

            // Watch the alarm/red lights blink
            await Awaitable.WaitForSecondsAsync(this.delayBeforeFade);

            this.fadeCanvas.gameObject.SetActive(true);
            this.fadeCanvas.alpha = 0f;
            await this.fadeCanvas.DOFade(1f, 1f).AsyncWaitForCompletion();

            SituationData activeDialogue = this.genericBossScoldingData;
            if (failedMail != null && failedMail.FalseReportDialogue != null)
            {
                activeDialogue = failedMail.FalseReportDialogue;
            }

            // Swap to Boss Camera
            if (this.bossOfficeCamera != null) this.bossOfficeCamera.Priority = 100;

            CinemachineBrain cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (cameraBrain != null)
            {
                // FIX: Wait 0.1s so Cinemachine registers the priority change
                await Awaitable.WaitForSecondsAsync(0.1f);
                while (cameraBrain.IsBlending)
                {
                    await Awaitable.NextFrameAsync();
                }
            }

            await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
            this.fadeCanvas.gameObject.SetActive(false);

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Dialogue;

            if (activeDialogue != null)
            {
                if (this.npcDialogue != null) this.npcDialogue.OnDialogueEnded += this.OnDialogueEnded;
                this.npcDialogue.SetDialogue(activeDialogue, false);
                this.npcDialogue.StartDialogue();
            }
            else
            {
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

            // Reset Camera
            if (this.bossOfficeCamera != null) this.bossOfficeCamera.Priority = 0;

            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Walking;

            if (chairToReturnTo != null)
            {
                // Sit the player back down
                chairToReturnTo.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);
            }

            // FIX: Wait for the camera to blend back to the PC monitor while screen is still black!
            if (cameraBrain != null)
            {
                await Awaitable.WaitForSecondsAsync(0.1f);
                while (cameraBrain.IsBlending)
                {
                    await Awaitable.NextFrameAsync();
                }
            }

            ConditionTracker.Instance.SetCondition(new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.HasFalselyReported, false));

            await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
            this.fadeCanvas.gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError($"CRITICAL ERROR in BossMeetingSequence: {e.Message}\n{e.StackTrace}");
        }
        finally
        {
            this.isSequenceRunning = false;
        }
    }
}