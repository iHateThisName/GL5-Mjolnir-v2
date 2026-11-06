using System;
using UnityEngine;

public class ElevatorAnimationController : MonoBehaviour
{
    [Header("stuff")]
    public GameObject Doors2Animate;
    public GameObject ElevatorButtonInside;
    public GameObject DoorBlock;
    [Header("Buttons")]
    public WorldInteractable insideButton;
    public WorldInteractable outsideButton;
    [Header("Animator")]
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
        Animator.SetTrigger("OpenDoor");
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
