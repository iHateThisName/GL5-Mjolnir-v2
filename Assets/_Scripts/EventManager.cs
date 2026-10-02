using TMPro;
using UnityEngine;
using Assets._Scripts;
using UnityEngine.SceneManagement;

public enum Day1States : int
{
    SitAtDesk,
    CheckMail1,
    CheckMail2,
    CheckMail3,
    CheckMail4,
    CheckMail5,
    CheckMail6,
    SitAtLindasDesk,
    CheckMail7,
    PhoneRinging1,      // Scam Call
    PhoneActiveCall1,
    PhoneRinging2,      // NEW: Legit Call
    PhoneActiveCall2,
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

    // NEW: Call Scriptable Objects
    [SerializeField] private Call scamCall;
    [SerializeField] private Call legitCall;

    [Header("NPC Interactions")]
    [SerializeField] private NPCDialoguePrototype npcDialogue;

    [Header("Mail ScriptableObjects")]
    [SerializeField] private Mail firstMail;
    [SerializeField] private Mail newCompanyMail;
    [SerializeField] private Mail newScamMail1;
    [SerializeField] private Mail newPersonalMail;
    [SerializeField] private Mail newScamMail2;
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
    private int totalErrors = 0;

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
        if (npcDialogue != null) npcDialogue.onDialogueFinished.AddListener(HandleNPCInteractionFinished);
    }

    private void OnDisable()
    {
        // (Same cleanup as before)
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
        if (npcDialogue != null) npcDialogue.onDialogueFinished.RemoveListener(HandleNPCInteractionFinished);
    }

    private void Start()
    {
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
            case Day1States.CheckMail2: return newCompanyMail;
            case Day1States.CheckMail3: return newScamMail1;
            case Day1States.CheckMail4: return newPersonalMail;
            case Day1States.CheckMail5: return newScamMail2;
            case Day1States.CheckMail6: return secondMail;
            case Day1States.CheckMail7: return thirdMail;
            default: return null;
        }
    }

    private void HandleMailDeleted(MailData mail)
    {
        if (mail != GetExpectedMailForCurrentState()) return;

        if (mail.MailType == MailData.MailTypeEnum.Company || mail.MailType == MailData.MailTypeEnum.Personal)
            TriggerDeleteWarning();
        else
            AdvanceState();
    }

    private void HandleMailReplied(MailData mail)
    {
        if (mail != GetExpectedMailForCurrentState()) return;

        if (mail.MailType == MailData.MailTypeEnum.Scam || mail.MailType == MailData.MailTypeEnum.Spam)
            TriggerScamWarning();
        else
            AdvanceState();
    }

    private void HandlePhonePickedUp()
    {
        if (currentState == Day1States.PhoneRinging1 || currentState == Day1States.PhoneRinging2)
            AdvanceState();
    }

    private void HandlePhoneApproved()
    {
        if (currentState == Day1States.PhoneActiveCall1)
        {
            TriggerScamWarning(); // Approved the scam call!
        }
        else if (currentState == Day1States.PhoneActiveCall2)
        {
            AdvanceState(); // Approved the legit call! Good job.
        }
    }

    private void HandlePhoneHungUp()
    {
        if (currentState == Day1States.PhoneActiveCall1)
        {
            AdvanceState(); // Hung up on scam call! Good job.
        }
        else if (currentState == Day1States.PhoneActiveCall2)
        {
            TriggerDeleteWarning(); // Hung up on legit call! (Error)
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
            case Day1States.CheckMail2: ChangeState(Day1States.CheckMail3); break;
            case Day1States.CheckMail3: ChangeState(Day1States.CheckMail4); break;
            case Day1States.CheckMail4: ChangeState(Day1States.CheckMail5); break;
            case Day1States.CheckMail5: ChangeState(Day1States.CheckMail6); break;
            case Day1States.CheckMail6: ChangeState(Day1States.SitAtLindasDesk); break;
            case Day1States.SitAtLindasDesk: ChangeState(Day1States.CheckMail7); break;
            case Day1States.CheckMail7: ChangeState(Day1States.PhoneRinging1); break;
            case Day1States.PhoneRinging1: ChangeState(Day1States.PhoneActiveCall1); break;

            // NEW TRANSITIONS:
            case Day1States.PhoneActiveCall1: ChangeState(Day1States.PhoneRinging2); break;
            case Day1States.PhoneRinging2: ChangeState(Day1States.PhoneActiveCall2); break;
            case Day1States.PhoneActiveCall2: ChangeState(Day1States.NPCInteraction); break;

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
                stepText.text = $"Task: You have a new message from {newCompanyMail.EmailAddress}.";
                mailBoxManager.AddMail(newCompanyMail);
                break;
            case Day1States.CheckMail3:
                stepText.text = $"Task: You have a new message from {newScamMail1.EmailAddress}.";
                mailBoxManager.AddMail(newScamMail1);
                break;
            case Day1States.CheckMail4:
                stepText.text = $"Task: You have a new message from {newPersonalMail.EmailAddress}.";
                mailBoxManager.AddMail(newPersonalMail);
                break;
            case Day1States.CheckMail5:
                stepText.text = $"Task: You have a new message from {newScamMail2.EmailAddress}.";
                mailBoxManager.AddMail(newScamMail2);
                break;
            case Day1States.CheckMail6:
                stepText.text = $"Task: You have a new message from {secondMail.EmailAddress}.";
                mailBoxManager.AddMail(secondMail);
                break;
            case Day1States.SitAtLindasDesk:
                stepText.text = "Task: Go sit at Linda's desk and help her out.";
                break;
            case Day1States.CheckMail7:
                stepText.text = $"Task: You have a new message from {thirdMail.EmailAddress} on Linda's computer.";
                lindasMailBoxManager.AddMail(thirdMail);
                break;

            case Day1States.PhoneRinging1:
                stepText.text = "Task: Your phone is ringing. Pick it up.";
                phoneController.OnPhoneCall(scamCall); // Pass Scam Call
                break;
            case Day1States.PhoneActiveCall1:
                stepText.text = "Task: Listen to the caller. Should you approve their request?";
                break;

            case Day1States.PhoneRinging2:
                stepText.text = "Task: Your phone is ringing again. Pick it up.";
                phoneController.OnPhoneCall(legitCall); // Pass Legit Call
                break;
            case Day1States.PhoneActiveCall2:
                stepText.text = "Task: Listen to the caller. Should you approve their request?";
                break;

            case Day1States.NPCInteraction:
                stepText.text = "Task: Go and talk to Greeb!";
                break;

            case Day1States.CompleteDay:
                stepText.text = "Task: Shift over.";
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                if (winPanel != null) winPanel.SetActive(true);
                if (winSummaryText != null)
                {
                    winSummaryText.text = $"End of day: Day 1 Complete!\n\nYou made {totalErrors} error(s) today.";
                }
                break;
        }
    }
}