using System;
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
    }

    private void OnPhoneInteract() {
        if (!IsPhoneInHand) {
            this.IsPhoneInHand = true;
            this.phoneModel.transform.SetParent(this.phoneUpPosition);
            this.phoneModel.transform.localPosition = Vector3.zero;
            this.phoneModel.transform.localRotation = Quaternion.identity;
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
}
