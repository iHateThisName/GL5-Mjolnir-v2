using TMPro;
using UnityEngine;
using Assets._Scripts;
using UnityEngine.SceneManagement;

public enum Day1States
{
    SitAtDesk,
    CheckMail1,
    CheckMail2,
    SitAtLindasDesk,
    CheckMail3,
    NPCInteraction,
    PhoneInteraction,
    CompleteDay,
    GameOver
}

public class EventManager : MonoBehaviour
{
    [Header("Player's Desk")]
    [SerializeField] private Chair officeChair;
    [SerializeField] private MailSelectedController mailController;
    [SerializeField] private MailBoxManager mailBoxManager;

    [Header("Linda's Desk")]
    [SerializeField] private Chair lindasChair; // Assign Linda's chair here
    [SerializeField] private MailSelectedController lindasMailController; // Linda's email UI
    [SerializeField] private MailBoxManager lindasMailBoxManager; 

    [Header("ScriptableObjects")]
    [SerializeField] private Mail firstMail;
    [SerializeField] private Mail secondMail;
    [SerializeField] private Mail thirdMail;

    [Header("Tutorial UI Panel")]
    [SerializeField] private TMP_Text stepText;

    private Day1States currentState;

    private void OnEnable()
    {
        // Subscribe to Player's PC events
        if (mailController != null)
        {
            mailController.selectedMailDeleted.AddListener(HandleMailDeleted);
            mailController.selectedMailReplay.AddListener(HandleMailReplied);
        }

        // Subscribe to Linda's PC events
        if (lindasMailController != null)
        {
            lindasMailController.selectedMailDeleted.AddListener(HandleMailDeleted);
            lindasMailController.selectedMailReplay.AddListener(HandleMailReplied);
        }

        // Listen for the player sitting down at specific chairs
        if (officeChair != null) officeChair.onPlayerSatDown.AddListener(HandlePlayerSatAtOwnDesk);
        if (lindasChair != null) lindasChair.onPlayerSatDown.AddListener(HandlePlayerSatAtLindasDesk);
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
    }

    private void Start()
    {
        ChangeState(Day1States.SitAtDesk);
    }

    private void HandlePlayerSatAtOwnDesk()
    {
        if (currentState == Day1States.SitAtDesk)
        {
            AdvanceState();
        }
    }

    private void HandlePlayerSatAtLindasDesk()
    {
        if (currentState == Day1States.SitAtLindasDesk)
        {
            AdvanceState();
        }
    }

    /// <summary>
    /// Returns the specific email the player is supposed to be interacting with right now.
    /// Returns null if the current task doesn't involve checking an email.
    /// </summary>
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
        // GUARD: Ignore this click if it's not the email we are currently waiting for
        if (mail != GetExpectedMailForCurrentState()) return;

        // Deleting legitimate emails results in a Game Over
        if (mail.EmailType == Mail.EnumMailType.Company ||
            mail.EmailType == Mail.EnumMailType.Personal)
        {
            ChangeState(Day1States.GameOver);
        }
        else
        {
            // Deleting Scam or Spam is correct, move to the next stage
            AdvanceState();
        }
    }

    private void HandleMailReplied(Mail mail)
    {
        // GUARD: Ignore this click if it's not the email we are currently waiting for
        if (mail != GetExpectedMailForCurrentState()) return;

        // Replying to malicious or junk emails results in a Game Over
        if (mail.EmailType == Mail.EnumMailType.Scam ||
            mail.EmailType == Mail.EnumMailType.Spam)
        {
            ChangeState(Day1States.GameOver);
        }
        else
        {
            // Replying to legitimate emails is correct, move to the next stage
            AdvanceState();
        }
    }

    public void AdvanceState()
    {
        switch (currentState)
        {
            case Day1States.SitAtDesk:
                ChangeState(Day1States.CheckMail1);
                break;
            case Day1States.CheckMail1:
                ChangeState(Day1States.CheckMail2);
                break;
            case Day1States.CheckMail2:
                ChangeState(Day1States.SitAtLindasDesk);
                break;
            case Day1States.SitAtLindasDesk:
                ChangeState(Day1States.CheckMail3);
                break;
            case Day1States.CheckMail3:
                ChangeState(Day1States.PhoneInteraction);
                break;
            case Day1States.PhoneInteraction:
                ChangeState(Day1States.NPCInteraction);
                break;
            case Day1States.NPCInteraction:
                ChangeState(Day1States.CompleteDay);
                break;
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

            case Day1States.PhoneInteraction:
                stepText.text = $"Your phone is calling!";
                //blabla;
                break;

            case Day1States.CompleteDay:
                stepText.text = "Day 1 Complete! Time to go home.";
                break;

            case Day1States.GameOver:
                stepText.text = "GAME OVER: You made the wrong choice!";
                SceneManager.LoadScene("LoseScene");
                break;
        }
    }
}