// --- DialogueSequenceController.cs ---

using Assets._Scripts;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueSequenceController : MonoBehaviour
{
    [Header("Sequence Trigger")]
    [SerializeField] private ConditionTracker.ConditionEnum triggerCondition;

    [Header("Dependencies")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private DialogueSystem npcDialogue;
    [SerializeField] private Chair targetChair;
    [SerializeField] private Chair workStationChair;

    [Header("Dialogue Data")]
    [SerializeField] private SituationData dialogueData;

    [Header("Optional TV Setup (For Meeting Room)")]
    [Tooltip("Leave empty if this sequence does not use a TV (e.g. Boss Office)")]
    [SerializeField] private MeetingRoomTVController optionalTvController;
    [SerializeField] private MailData optionalMailData;
    [SerializeField] private bool showAnnotatedMail = true;

    [Header("Optional Setup")]
    [Tooltip("Leave empty if you want to teleport back immediately after dialogue.")]
    [SerializeField] private Button continueButton;

    [Tooltip("Fires while the screen is fully black.")]
    [SerializeField] private UnityEvent onScreenFadedOut;

    private bool isSequenceRunning;
    private bool hasFinishedDialogue;

    private void OnEnable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged += this.OnConditionStateChanged;
        }
        if (this.npcDialogue != null)
        {
            this.npcDialogue.OnDialogueEnded += this.OnDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (ConditionTracker.Instance != null)
        {
            ConditionTracker.Instance.OnConditionStateChanged -= this.OnConditionStateChanged;
        }
        if (this.npcDialogue != null)
        {
            this.npcDialogue.OnDialogueEnded -= this.OnDialogueEnded;
        }
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state)
    {
        if (state.Condition == this.triggerCondition && state.ExpectedState)
        {
            _ = this.RunSequenceAsync();
        }
    }

    private void OnDialogueEnded()
    {
        this.hasFinishedDialogue = true;
    }

    private async Awaitable RunSequenceAsync()
    {
        if (this.isSequenceRunning) return;
        this.isSequenceRunning = true;
        this.hasFinishedDialogue = false;

        // 1. Fade out to black
        this.fadeCanvas.gameObject.SetActive(true);
        this.fadeCanvas.alpha = 0f;
        await this.fadeCanvas.DOFade(1f, 1f).AsyncWaitForCompletion();

        // 2. Teleport player and setup optional TV
        if (this.workStationChair != null) this.workStationChair.StandUp();

        if (this.optionalTvController != null && this.optionalMailData != null)
        {
            this.optionalTvController.DisplayMail(this.optionalMailData, this.showAnnotatedMail);
        }

        this.onScreenFadedOut?.Invoke();

        if (this.targetChair != null) this.targetChair.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);

        // 3. Fade back in
        await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
        this.fadeCanvas.gameObject.SetActive(false);

        // 4. Start Dialogue
        this.npcDialogue.SetDialogue(this.dialogueData, false);

        // 5. Wait for the player to click through the dialogue
        while (!this.hasFinishedDialogue)
        {
            await Awaitable.NextFrameAsync();
        }

        // 6. Handle the optional Continue button
        if (this.continueButton != null)
        {
            this.continueButton.gameObject.SetActive(true);
            await this.WaitForButtonPressAsync(this.continueButton);
            this.continueButton.gameObject.SetActive(false);
        }

        // 7. Fade out again to return
        this.fadeCanvas.gameObject.SetActive(true);
        await this.fadeCanvas.DOFade(1f, 1f).AsyncWaitForCompletion();

        // 8. Return TP to desk
        if (this.targetChair != null) this.targetChair.StandUp();
        if (this.workStationChair != null) this.workStationChair.Interact(PlayerRefrenceProvider.Instance.PlayerMovementController.gameObject);

        // 9. Fade back in to the workstation
        await this.fadeCanvas.DOFade(0f, 1f).AsyncWaitForCompletion();
        this.fadeCanvas.gameObject.SetActive(false);

        this.isSequenceRunning = false;
    }

    private async Awaitable WaitForButtonPressAsync(Button button)
    {
        bool isPressed = false;
        UnityEngine.Events.UnityAction action = () => isPressed = true;
        button.onClick.AddListener(action);

        while (!isPressed)
        {
            await Awaitable.NextFrameAsync();
        }

        button.onClick.RemoveListener(action);
    }
}