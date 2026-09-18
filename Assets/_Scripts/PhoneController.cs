using DG.Tweening;
using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour {

    [SerializeField] WorldInteractable GreenButton;
    [SerializeField] WorldInteractable RedButton;
    [SerializeField] WorldInteractable PhoneInteractable;
    [SerializeField] WorldInteractable PhoneBaseInteractable;

    [SerializeField] private GameObject phoneModel;
    private Transform phoneVisualTransform;
    [SerializeField] private Transform phoneDownPosition;
    [SerializeField] private Transform phoneUpPosition;

    [SerializeField] private SFXController ringtoneSound;
    [SerializeField] private SFXController scamCallSound;
    private bool isPhoneRinging = false;

    private Sequence phoneRiningSequence = null;

    public bool IsPhoneInHand { get; private set; } = false;

    private void OnEnable() {
        PhoneInteractable.OnInteract += OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract += EndPhoneCall;
    }
    private void OnDisable() {
        PhoneInteractable.OnInteract -= OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract -= EndPhoneCall;
    }

    private void Start() {
        this.phoneVisualTransform = this.transform.GetChild(0);

        this.phoneRiningSequence = DOTween.Sequence().SetId(this.phoneModel.transform).Pause();
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchScale(punch: new Vector3(0.25f, 0.25f, 0.25f), duration: 2f, vibrato: 5, elasticity: 0).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchPosition(punch: new Vector3(0.05f, 0, 0), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchRotation(punch: new Vector3(0, 0, 10), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
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
            this.phoneModel.transform.SetParent(this.phoneVisualTransform);
            this.phoneModel.transform.localPosition = this.phoneDownPosition.localPosition;
            this.phoneModel.transform.localRotation = this.phoneDownPosition.localRotation;
        }
    }

    [ContextMenu("Call Phone")]
    public void OnPhoneCall() { // Parameter should contain a scritable object. containe phone data.
        // Check if the phone is already in hand, if not play sound and animate the phone vibrating using DOTween.

        if (!IsPhoneInHand) {
            this.isPhoneRinging = true;
            // Play phone ringing sound
            this.ringtoneSound.PlaySFX();

            // Play the animation.
            this.phoneRiningSequence.Restart();
        } else {
            // The phone is already in hand, so we can accept the call directly.
            OnPhoneCallAccepted();
        }

    }

    [ContextMenu("Accept Phone Call")]
    public void OnPhoneCallAccepted() {
        this.isPhoneRinging = false;
        // Make sure the animation is not playing or the rinigning
        this.ringtoneSound.StopSFX();
        this.phoneRiningSequence.Pause();
        this.scamCallSound.PlaySFX();
    }

}
