using System.Collections;
using UnityEngine;
using static ConditionTracker;

public class ElevatorAnimationController : MonoBehaviour
{
    [Header("EndOfDay")]
    public bool TheEndOfDay = false;
    [Header("stuff")]
    public GameObject Doors2Animate;
    public GameObject ElevatorButtonInside;
    public GameObject DoorBlock;
    [Header("Buttons")]
    public WorldInteractable insideButton;
    public WorldInteractable outsideButton;
    [Header("Animator")]
    public Animator Animator;


    void Start()
    {
        DoorBlock.SetActive(false);
        //Animator.Play("ElevatorDoorsOpen");
    }

    private void OnTriggerExit(Collider other)
    {
        DoorBlock.SetActive(true);
        Animator.SetTrigger("CloseDoor");
        ConditionTracker.Instance.SetCondition(new ConditionState(ConditionEnum.HasExitedElevator, true));
    }

    private void OnEnable()
    {
        this.insideButton.OnInteract += () => OnElevatorButtonPressed(true);
        this.outsideButton.OnInteract += () => OnElevatorButtonPressed(false);

        ConditionTracker.Instance.OnConditionStateChanged += OnConditionStateChanged;
    }

    private void OnDisable()
    {
        this.insideButton.OnInteract -= () => OnElevatorButtonPressed(true);
        this.outsideButton.OnInteract -= () => OnElevatorButtonPressed(false);

        if (ConditionTracker.Instance != null) ConditionTracker.Instance.OnConditionStateChanged -= OnConditionStateChanged;
    }

    private void OnConditionStateChanged(ConditionTracker.ConditionState state)
    {
        if (state.Condition == ConditionTracker.ConditionEnum.IsDayComplete)
        {
            this.TheEndOfDay = state.ExpectedState;
        }
    }

    private void OnElevatorButtonPressed(bool isInnsideButton)
    {
        if (isInnsideButton)
        {
            if (this.TheEndOfDay)
            {
                StartCoroutine(DelayedShowSummary());
            }
            else
            {
                Debug.Log("Button Pressed");
                Animator.SetTrigger("OpenDoor");
            }

        }
        else if (TheEndOfDay)
        {
            EndOfDay();
        }
    }

    private IEnumerator DelayedShowSummary()
    {
        this.insideButton.OnInteract -= () => OnElevatorButtonPressed(true);
        yield return new WaitForSecondsRealtime(1f);
        ConditionState conditionState = new(condition: ConditionEnum.ShowSummary, expectedState: true);
        ConditionTracker.Instance.SetCondition(conditionState);
    }

    private void EndOfDay()
    {
        DoorBlock.SetActive(false);
        Animator.SetTrigger("OpenDoor");
    }
}
