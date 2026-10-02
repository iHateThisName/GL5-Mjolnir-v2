using UnityEngine;
using UnityEngine.InputSystem;

public class TVTester : MonoBehaviour
{
    [Header("Test References")]
    [SerializeField] private MeetingRoomTVController tvController;
    [SerializeField] private MailData testScamMail;

    private bool isShowingAnnotated = false; // Start with the normal version

    void Start()
    {
        // Show the original email as soon as the game starts
        if (tvController != null && testScamMail != null)
        {
            tvController.gameObject.SetActive(true);
            tvController.DisplayMail(testScamMail, isShowingAnnotated);
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (tvController != null && testScamMail != null)
            {
                // Flip between normal and annotated
                isShowingAnnotated = !isShowingAnnotated;

                // Update the TV text
                tvController.DisplayMail(testScamMail, isShowingAnnotated);
            }
        }
    }
}