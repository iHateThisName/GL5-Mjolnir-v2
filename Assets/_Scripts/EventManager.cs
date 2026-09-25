using TMPro;
using UnityEngine;
using Assets._Scripts;
using UnityEngine.SceneManagement;

public enum Day1States : int
{
    SitAtDesk,
    CheckMail1,
    CheckMail2,
    SitAtLindasDesk,
    CheckMail3,
    PhoneRinging,
    PhoneActiveCall,
    NPCInteraction,
    CompleteDay
}

public class EventManager : MonoBehaviour
{
    [Header("Player's Desk")]
    [SerializeField] private Chair officeChair;
    [SerializeField] private MailSelectedController mailController;
    [SerializeField] private MailBoxManager mailBoxManager;

    [Header("Linda's Desk")]
    [SerializeField] private Chair lindasChair;
    [SerializeField] private MailSelectedController lindasMailController;
    [SerializeField] private MailBoxManager lindasMailBoxManager;

    [Header("Phone Interactions")]
    [SerializeField] private PhoneController phoneController;

    [Header("NPC Interactions")]
    [SerializeField] private NPCDialoguePrototype npcDialogue;

    [Header("ScriptableObjects")]
    [SerializeField] private Mail firstMail;
    [SerializeField] private Mail secondMail;
    [SerializeField] private Mail thirdMail;

    [Header("UI & Panels")]
    [SerializeField] private TMP_Text stepText;
    [SerializeField] private GameObject scamWarningPanel;
    [SerializeField] private GameObject deleteWarningPanel;

    [Header("End of Day UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text winSummaryText;

    private Day1States currentState;
    private int totalErrors = 0; // Tracks player mistakes

    private void OnEnable()
    {
        if (mailController != null)
        {
            mailController.selectedMailDeleted.AddListener(HandleMailDeleted);
            mailController.selectedMailReplay.AddListener(HandleMailReplied);
        }

        if (lindasMailController != null)
        {
            lindasMailController.selectedMailDeleted.AddListener(HandleMailDeleted);
            lindasMailController.selectedMailReplay.AddListener(HandleMailReplied);
        }

        if (officeChair != null) officeChair.onPlayerSatDown.AddListener(HandlePlayerSatAtOwnDesk);
        if (lindasChair != null) lindasChair.onPlayerSatDown.AddListener(HandlePlayerSatAtLindasDesk);

        if (phoneController != null)
        {
            phoneController.onPhonePickedUp.AddListener(HandlePhonePickedUp);
            phoneController.onPhoneApproved.AddListener(HandlePhoneApproved);
            phoneController.onPhoneHungUp.AddListener(HandlePhoneHungUp);
        }

        if (npcDialogue != null)
        {
            npcDialogue.onDialogueFinished.AddListener(HandleNPCInteractionFinished);
        }
    }

    private void OnDisable()
    {
        if (mailController != null)
        {
            mailController.selectedMailDeleted.RemoveListener(HandleMailDeleted);
            mailController.selectedMailReplay.RemoveListener(HandleMailReplied);
        }

        if (lindasMailController != null)
        {
            lindasMailController.selectedMailDeleted.RemoveListener(HandleMailDeleted);
            lindasMailController.selectedMailReplay.RemoveListener(HandleMailReplied);
        }

        if (officeChair != null) officeChair.onPlayerSatDown.RemoveListener(HandlePlayerSatAtOwnDesk);
        if (lindasChair != null) lindasChair.onPlayerSatDown.RemoveListener(HandlePlayerSatAtLindasDesk);

        if (phoneController != null)
        {
            phoneController.onPhonePickedUp.RemoveListener(HandlePhonePickedUp);
            phoneController.onPhoneApproved.RemoveListener(HandlePhoneApproved);
            phoneController.onPhoneHungUp.RemoveListener(HandlePhoneHungUp);
        }

        if (npcDialogue != null)
        {
            npcDialogue.onDialogueFinished.RemoveListener(HandleNPCInteractionFinished);
        }
    }

    private void Start()
    {
        // Ensure panels are hidden at the start
        if (scamWarningPanel != null) scamWarningPanel.SetActive(false);
        if (deleteWarningPanel != null) deleteWarningPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        ChangeState(Day1States.SitAtDesk);
    }

    private void HandlePlayerSatAtOwnDesk()
    {
        if (currentState == Day1States.SitAtDesk) AdvanceState();
    }

    private void HandlePlayerSatAtLindasDesk()
    {
        if (currentState == Day1States.SitAtLindasDesk) AdvanceState();
    }

    private Mail GetExpectedMailForCurrentState()
    {
        switch (currentState)
        {
            case Day1States.CheckMail1: return firstMail;
            case Day1States.CheckMail2: return secondMail;
            case Day1States.CheckMail3: return thirdMail;
            default: return null;
        }
    }

    private void HandleMailDeleted(Mail mail)
    {
        if (mail != GetExpectedMailForCurrentState()) return;

        if (mail.EmailType == Mail.EnumMailType.Company || mail.EmailType == Mail.EnumMailType.Personal)
        {
            TriggerDeleteWarning();
        }
        else
        {
            AdvanceState();
        }
    }

    private void HandleMailReplied(Mail mail)
    {
        if (mail != GetExpectedMailForCurrentState()) return;

        if (mail.EmailType == Mail.EnumMailType.Scam || mail.EmailType == Mail.EnumMailType.Spam)
        {
            TriggerScamWarning();
        }
        else
        {
            AdvanceState();
        }
    }

    private void HandlePhonePickedUp()
    {
        if (currentState == Day1States.PhoneRinging) AdvanceState();
    }

    private void HandlePhoneApproved()
    {
        if (currentState == Day1States.PhoneActiveCall)
        {
            TriggerScamWarning();
        }
    }

    private void HandlePhoneHungUp()
    {
        if (currentState == Day1States.PhoneActiveCall)
        {
            AdvanceState();
        }
    }

    private void HandleNPCInteractionFinished()
    {
        if (currentState == Day1States.NPCInteraction) AdvanceState();
    }

    private void TriggerScamWarning()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        totalErrors++;
        if (scamWarningPanel != null) scamWarningPanel.SetActive(true);
    }

    private void TriggerDeleteWarning()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        totalErrors++;
        if (deleteWarningPanel != null) deleteWarningPanel.SetActive(true);
    }

    public void AcknowledgeWarning()
    {
        if (scamWarningPanel != null) scamWarningPanel.SetActive(false);
        if (deleteWarningPanel != null) deleteWarningPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;


        AdvanceState();
    }

    /// <summary>
    /// Call this from a Quit button on your Win Panel
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void AdvanceState()
    {
        switch (currentState)
        {
            case Day1States.SitAtDesk: ChangeState(Day1States.CheckMail1); break;
            case Day1States.CheckMail1: ChangeState(Day1States.CheckMail2); break;
            case Day1States.CheckMail2: ChangeState(Day1States.SitAtLindasDesk); break;
            case Day1States.SitAtLindasDesk: ChangeState(Day1States.CheckMail3); break;
            case Day1States.CheckMail3: ChangeState(Day1States.PhoneRinging); break;
            case Day1States.PhoneRinging: ChangeState(Day1States.PhoneActiveCall); break;
            case Day1States.PhoneActiveCall: ChangeState(Day1States.NPCInteraction); break;
            case Day1States.NPCInteraction: ChangeState(Day1States.CompleteDay); break;
        }
    }

    private void ChangeState(Day1States newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case Day1States.SitAtDesk:
                stepText.text = "Task: Go sit at your desk and turn on the computer.";
                break;

            case Day1States.CheckMail1:
                stepText.text = $"Task: Read the email from {firstMail.EmailAddress}.";
                mailBoxManager.AddMail(firstMail);
                break;

            case Day1States.CheckMail2:
                stepText.text = $"Task: You have a new message from {secondMail.EmailAddress}.";
                mailBoxManager.AddMail(secondMail);
                break;

            case Day1States.SitAtLindasDesk:
                stepText.text = "Task: Go sit at Linda's desk and help her out.";
                break;

            case Day1States.CheckMail3:
                stepText.text = $"Task: You have a new message from {thirdMail.EmailAddress} on Linda's computer.";
                lindasMailBoxManager.AddMail(thirdMail);
                break;

            case Day1States.PhoneRinging:
                stepText.text = "Task: Your phone is ringing. Pick it up.";
                phoneController.OnPhoneCall();
                break;

            case Day1States.PhoneActiveCall:
                stepText.text = "Task: Listen to the caller. Should you approve their request?";
                break;

            case Day1States.NPCInteraction:
                stepText.text = "Task: Go and talk to Greeb!";
                break;

            case Day1States.CompleteDay:
                stepText.text = "Task: Shift over.";
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                // Show the Win Panel and update the text inside it
                if (winPanel != null) winPanel.SetActive(true);

                if (winSummaryText != null)
                {
                    winSummaryText.text = $"End of day: Day 1 Complete!\n\nYou made {totalErrors} error(s) today.";
                }
                break;
        }
    }
}