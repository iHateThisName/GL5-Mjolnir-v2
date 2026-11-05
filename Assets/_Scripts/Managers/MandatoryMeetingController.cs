using UnityEngine;
using Unity.Cinemachine;

public class MandatoryMeetingController : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineCamera PlayerCamera;
    public CinemachineCamera MeetingCamera;

    [Header("Room Setup")]
    public Transform CoworkerSpawnPoint;
    public MeetingRoomTVController TvController;

    [Header("Timing")]
    public float MeetingDuration = 5.0f;
    public float DelayBeforeCut = 1.5f; // Gives time for ScamFailureFeedback to play

    private GameObject currentCoworkerInstance;
    private bool isMeetingActive;

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
        // Trigger when the new HasBeenScammed condition is met
        if (state.Condition == ConditionTracker.ConditionEnum.HasBeenScammed && state.ExpectedState)
        {
            _ = this.RunSmashCutMeetingAsync();
        }
    }

    private async Awaitable RunSmashCutMeetingAsync()
    {
        if (this.isMeetingActive) return;
        this.isMeetingActive = true;

        // 1. Wait for the alarm/red lights from ScamFailureFeedback to play out
        await Awaitable.WaitForSecondsAsync(this.DelayBeforeCut);

        // 2. Setup TV and Spawn Coworker
        if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
        {
            MailData failedMail = SituationManager.Instance.PendingMailPresentation.Mail;

            if (this.TvController != null)
            {
                this.TvController.DisplayMail(failedMail, true);
            }

            // Spawns the specific coworker assigned to this email
            if (failedMail.RuinedCoworkerPrefab != null && this.CoworkerSpawnPoint != null)
            {
                this.currentCoworkerInstance = Instantiate(failedMail.RuinedCoworkerPrefab, this.CoworkerSpawnPoint.position, this.CoworkerSpawnPoint.rotation);
            }
        }

        // 3. Smash cut cameras
        this.PlayerCamera.Priority = 0;
        this.MeetingCamera.Priority = 10;

        // 4. Let the player sit in shame for a few seconds
        await Awaitable.WaitForSecondsAsync(this.MeetingDuration);

        // 5. Smash cut back to desk
        this.MeetingCamera.Priority = 0;
        this.PlayerCamera.Priority = 10;

        // Clean up the coworker from the meeting room
        if (this.currentCoworkerInstance != null)
        {
            Destroy(this.currentCoworkerInstance);
        }

        // 6. Mark meeting as completed so the rest of the game can continue
        ConditionTracker.Instance.SetCondition(new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.HasCompletedMeeting, true));

        // 7. Tell SituationManager the mail sequence is completely done
        if (SituationManager.Instance != null && SituationManager.Instance.HasPendingMailPresentation)
        {
            SituationManager.Instance.CompleteMailPresentation(SituationManager.Instance.PendingMailPresentation);
        }

        // 8. Reset the scammed condition so it can trigger again for the next email!
        ConditionTracker.Instance.SetCondition(new ConditionTracker.ConditionState(ConditionTracker.ConditionEnum.HasBeenScammed, false));

        this.isMeetingActive = false;
    }
}