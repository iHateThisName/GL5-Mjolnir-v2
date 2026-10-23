using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class PhoneController : MonoBehaviour {

    [SerializeField] private List<CallData> callDataList = new List<CallData>();

    [SerializeField] WorldInteractable PhoneInteractable;
    [SerializeField] WorldInteractable PhoneBaseInteractable;
    [SerializeField] private GameObject phoneModel;
    [SerializeField] private Transform phoneDownPosition;
    [SerializeField] private Transform phoneUpPosition;

    [SerializeField] private SFXController ringtoneSound;
    [SerializeField] private SFXController callSoundController;
    [SerializeField] private DialogueSystem dialogueSystem;

    public CallData CurrentActiveCall { get; private set; }
    public List<CallData> CallQueue = new List<CallData>(); // Calls waiting to be answered

    private bool isPhoneRinging = false;
    private Sequence phoneRiningSequence = null;


    public bool IsPhoneInHand { get; private set; } = false;

    private void OnEnable() {
        PhoneInteractable.OnInteract += OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract += EndPhoneCall;

        //GreenButton.OnInteract += ApproveCall;
        //RedButton.OnInteract += EndPhoneCall;

        SituationManager.Instance.OnSituationStateChange += OnSituationStateChange;
    }

    private void OnDisable() {
        PhoneInteractable.OnInteract -= OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract -= EndPhoneCall;

        //GreenButton.OnInteract -= ApproveCall;
        //RedButton.OnInteract -= EndPhoneCall;

        if (SituationManager.Instance != null) {
            SituationManager.Instance.OnSituationStateChange -= OnSituationStateChange;
        }
    }

    private void Start() {
        this.phoneRiningSequence = DOTween.Sequence().SetId(this.phoneModel.transform).Pause();
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchScale(punch: new Vector3(0.25f, 0.25f, 0.25f), duration: 2f, vibrato: 5, elasticity: 0).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchPosition(punch: new Vector3(0.05f, 0, 0), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchRotation(punch: new Vector3(0, 0, 10), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
    }
    


    private void OnSituationStateChange(SituationData data) {
        // Check if the situation is a CallData
        if (data is CallData callData) {

            // Check if this specific phone contains the call data
            if (callDataList.Contains(callData)) {
                // If the situation state is Active, trigger the phone call
                if (data.SituationStateEnum == SituationManager.SituationStateEnum.Active) {
                    if (this.CurrentActiveCall == null) {
                        this.CurrentActiveCall = callData;
                        OnPhoneCall(callData);
                    } else {
                        // If there's already an active call, add the new call to the queue
                        CallQueue.Add(callData);
                    }
                }
            }
        }
    }

    private void OnPhoneInteract() {
        if (!IsPhoneInHand) {
            this.IsPhoneInHand = true;
            this.phoneModel.transform.SetParent(this.phoneUpPosition);
            this.phoneModel.transform.localPosition = Vector3.zero;
            this.phoneModel.transform.localRotation = Quaternion.identity;

            if (this.isPhoneRinging) OnPhoneCallAccepted();

        }
    }
    public void EndPhoneCall() {
        if (IsPhoneInHand) {
            this.IsPhoneInHand = false;
            this.phoneModel.transform.SetParent(this.phoneDownPosition);
            this.phoneModel.transform.localPosition = Vector3.zero;
            this.phoneModel.transform.localRotation = Quaternion.identity;

            // Stop the talking sound if they hang up early
            this.callSoundController.StopSFX();

            // Flag the call task as completed
            SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Success, this.CurrentActiveCall);

            // End the dialog
            this.dialogueSystem.EndDialogue();

            if (this.CurrentActiveCall != null) {
                this.CurrentActiveCall = null;
                StartCoroutine(StartNextQueuedPhoneCall());
            }

        }
    }

    public IEnumerator StartNextQueuedPhoneCall() {
        if (CallQueue.Count > 0) {
            yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(1f, 3f));
            CallData nextCall = CallQueue[0];
            CallQueue.RemoveAt(0);
            OnPhoneCall(nextCall);
        }
        yield return null;
    }

    public void OnPhoneCall(CallData callData) {
        this.CurrentActiveCall = callData;

        if (!IsPhoneInHand) {
            this.isPhoneRinging = true;
            this.ringtoneSound.PlaySFX();
            this.phoneRiningSequence.Restart();
        } else {
            OnPhoneCallAccepted();
        }
    }

    public void OnPhoneCallAccepted() {
        this.isPhoneRinging = false;
        this.ringtoneSound.StopSFX();
        this.phoneRiningSequence.Pause();
        this.phoneModel.transform.localScale = Vector3.one;

        // Play the phone conversation

        this.callSoundController.Initilize(clip: this.CurrentActiveCall.ConversationAudio);

        this.callSoundController.PlaySFX();


        //// Play the conversation audio from the current active call and wait for it to finish before proceeding
        //await AudioManager.Instance.PlaySFXAsync(this.CurrentActiveCall.ConversationAudio, 0.5f);

        //// Display the Options wait for and answer from the player. For the two buttons.
        //await DisplayCallOptionsAndAwaitAnswear();

        if (this.CurrentActiveCall == null) {
            Debug.Log("wofmw");
        }

        // Dialog system
        this.dialogueSystem.SetDialogue(this.CurrentActiveCall, disableInteraction: true);

    }
}