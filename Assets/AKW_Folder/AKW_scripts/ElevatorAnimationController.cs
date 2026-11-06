using System;
using UnityEngine;

public class ElevatorAnimationController : MonoBehaviour
{
    public GameObject Doors2Animate;
    public GameObject ElevatorButtonInside;
    public GameObject DoorBlock;

    public WorldInteractable insideButton;
    public WorldInteractable outsideButton;

    public Animator Animator;

    private void OnEnable()
    {
        this.insideButton.OnInteract += OnElevatorButtonPressed;
        this.outsideButton.OnInteract += OnElevatorButtonPressed;
    }

    private void OnDisable()
    {
        this.insideButton.OnInteract -= OnElevatorButtonPressed;
        this.outsideButton.OnInteract -= OnElevatorButtonPressed;
    }

    private void OnElevatorButtonPressed()
    {
        Debug.Log("Button Pressed");
        Animator.Play("ElevatorDoorsOpen");
    }

    void Start()
    {
        DoorBlock.SetActive(false);
        //Animator.Play("ElevatorDoorsOpen");
    }

    private void OnTriggerExit(Collider other)
    {
        DoorBlock.SetActive(true);
        Animator.Play("ElevatorDoorClose");
    }
}
