using Assets._Scripts;
using UnityEngine;

public class SuccessSequence : MonoBehaviour
{
    [SerializeField] private DialogueSystem npcDialogue;
    [SerializeField] private SituationData genericSuccessDialogue; // Fallback

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
        // YOU WILL NEED TO ADD THIS ENUM TO ConditionTracker.cs!
        if (state.Condition == ConditionTracker.ConditionEnum.HasHandledMailCorrectly && state.ExpectedState)
        {
            MailData successMail = null;
            if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
            {
                successMail = SituationManager.Instance.PendingMailPresentation.Mail;
            }

            _ = this.RunSuccessSequenceAsync(successMail);
        }
    }

    private void OnDialogueEnded()
    {
        this.hasFinishedDialogue = true;
    }

    private async Awaitable RunSuccessSequenceAsync(MailData successMail)
    {
        if (this.isSequenceRunning) return;
        this.isSequenceRunning = true;
        this.hasFinishedDialogue = false;

        // 1. Lock the player
        GameManager.Instance.CurrentPlayerState = EnumPlayerState.Dialogue;

        // 2. Play the Dialogue FIRST
        SituationData activeDialogue = this.genericSuccessDialogue;
        if (successMail != null && successMail.SuccessDialogue != null)
        {
            activeDialogue = successMail.SuccessDialogue;
        }

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

        // Wait for player to click through the conversation
        while (!this.hasFinishedDialogue)
        {
            await Awaitable.NextFrameAsync();
        }
        if (this.npcDialogue != null) this.npcDialogue.OnDialogueEnded -= this.OnDialogueEnded;

        // 3. NOW spawn the prefab quietly on their home desk
        if (successMail != null && successMail.RewardPrefab != null && DeskRewardManager.Instance != null)
        {
            DeskRewardManager.Instance.SpawnReward(successMail.RewardPrefab);
        }

        // 4. Return control
        GameManager.Instance.CurrentPlayerState = EnumPlayerState.Sitting;

        ConditionTracker.Instance.SetCondition(new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.HasHandledMailCorrectly, false));
        this.isSequenceRunning = false;
    }
}