using DG.Tweening;
using UnityEngine;
using UnityEngine.Events; // Needed for UnityEvent

public class PhoneController : MonoBehaviour
{

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

    // Events for the EventManager
    public UnityEvent onPhonePickedUp = new UnityEvent();
    public UnityEvent onPhoneApproved = new UnityEvent();
    public UnityEvent onPhoneHungUp = new UnityEvent();

    private void OnEnable()
    {
        PhoneInteractable.OnInteract += OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract += EndPhoneCall;

        // Link the buttons
        GreenButton.OnInteract += ApproveCall;
        RedButton.OnInteract += EndPhoneCall;
    }

    private void OnDisable()
    {
        PhoneInteractable.OnInteract -= OnPhoneInteract;
        this.PhoneBaseInteractable.OnInteract -= EndPhoneCall;

        GreenButton.OnInteract -= ApproveCall;
        RedButton.OnInteract -= EndPhoneCall;
    }

    private void Start()
    {
        this.phoneVisualTransform = this.transform.GetChild(0);

        this.phoneRiningSequence = DOTween.Sequence().SetId(this.phoneModel.transform).Pause();
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchScale(punch: new Vector3(0.25f, 0.25f, 0.25f), duration: 2f, vibrato: 5, elasticity: 0).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchPosition(punch: new Vector3(0.05f, 0, 0), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
        this.phoneRiningSequence.Insert(0f, this.phoneModel.transform.DOPunchRotation(punch: new Vector3(0, 0, 10), duration: 2f, vibrato: 5, elasticity: 1).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Restart));
    }

    private void OnPhoneInteract()
    {
        if (!IsPhoneInHand)
        {
            this.IsPhoneInHand = true;
            this.phoneModel.transform.SetParent(this.phoneUpPosition);
            this.phoneModel.transform.localPosition = Vector3.zero;
            this.phoneModel.transform.localRotation = Quaternion.identity;

            if (this.isPhoneRinging) OnPhoneCallAccepted();

            // Notify the EventManager
            onPhonePickedUp?.Invoke();
        }
    }

    private void ApproveCall()
    {
        if (IsPhoneInHand)
        {
            // Player pressed green button while holding phone
            onPhoneApproved?.Invoke();
        }
    }

    public void EndPhoneCall()
    {
        if (IsPhoneInHand)
        {
            this.IsPhoneInHand = false;
            this.phoneModel.transform.SetParent(this.phoneVisualTransform);
            this.phoneModel.transform.localPosition = this.phoneDownPosition.localPosition;
            this.phoneModel.transform.localRotation = this.phoneDownPosition.localRotation;

            // Stop the talking sound if they hang up early
            this.scamCallSound.StopSFX();

            // Notify the EventManager
            onPhoneHungUp?.Invoke();
        }
    }

    [ContextMenu("Call Phone")]
    public void OnPhoneCall()
    {
        if (!IsPhoneInHand)
        {
            this.isPhoneRinging = true;
            this.ringtoneSound.PlaySFX();
            this.phoneRiningSequence.Restart();
        }
        else
        {
            OnPhoneCallAccepted();
        }
    }

    [ContextMenu("Accept Phone Call")]
    public void OnPhoneCallAccepted()
    {
        this.isPhoneRinging = false;
        this.ringtoneSound.StopSFX();
        this.phoneRiningSequence.Pause();
        // Reset scale/rotation just in case the tween leaves it offset
        this.phoneModel.transform.localScale = Vector3.one;
        this.scamCallSound.PlaySFX();
    }
}