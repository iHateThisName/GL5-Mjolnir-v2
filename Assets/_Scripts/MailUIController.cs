using UnityEngine;
using UnityEngine.UI;

public class MailUIController : MonoBehaviour {
    // Tracks where the player is so the Back button knows what to close
    private enum MailState { Home, InboxList, DeletedList, ReadingInbox, ReadingDeleted }
    private MailState currentState = MailState.Home;

    [Header("Panels")]
    [SerializeField] private GameObject leftContainer;
    [SerializeField] private GameObject emailListPanel;
    [SerializeField] private GameObject emailPanel;
    [SerializeField] private GameObject deletedListPanel;

    [Header("Buttons")]
    [SerializeField] private Button inboxButton;
    [SerializeField] private Button deletedButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeTabButton;
    [SerializeField] private Button bossButton;

    [SerializeField] private GameObject BossCanvasOverlay;

    private void OnEnable() {
        // Setup core navigation
        inboxButton.onClick.AddListener(OpenInbox);
        deletedButton.onClick.AddListener(OpenDeleted);
        backButton.onClick.AddListener(OnBackButtonClicked); // Now uses dynamic logic
        closeTabButton.onClick.AddListener(CloseMailWindow);
        ConditionTracker.Instance.OnConditionStateChanged += OnConditonChanged;

        // Setup placeholder buttons
        settingsButton.onClick.AddListener(() => Debug.Log("Settings button clicked - Not implemented yet"));

        // Force the app to open just the sidebar by default
        OpenHome();
    }

    private void OnConditonChanged(ConditionTracker.ConditionState state) {
        if (state.Condition == ConditionTracker.ConditionEnum.DeletedCompanyMail && state.ExpectedState)
            BossCanvasOverlay.SetActive(true);
    }

    public void ContinueFromBossOverlay() {
        BossCanvasOverlay.SetActive(false);
    }

    private void OnDisable() {
        inboxButton.onClick.RemoveListener(OpenInbox);
        deletedButton.onClick.RemoveListener(OpenDeleted);
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        closeTabButton.onClick.RemoveListener(CloseMailWindow);
        settingsButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Default state: Shows only the sidebar. Hides all lists and emails.
    /// </summary>
    public void OpenHome() {
        currentState = MailState.Home;

        leftContainer.SetActive(true);
        emailListPanel.SetActive(false);
        if (deletedListPanel != null) deletedListPanel.SetActive(false);
        emailPanel.SetActive(false);

        backButton.gameObject.SetActive(false); // Hide back button on home screen
    }

    public void OpenInbox() {
        currentState = MailState.InboxList;

        leftContainer.SetActive(true);
        emailListPanel.SetActive(true);
        if (deletedListPanel != null) deletedListPanel.SetActive(false);
        emailPanel.SetActive(false);

        backButton.gameObject.SetActive(true); // Show back button to hide the inbox list
    }

    public void OpenDeleted() {
        currentState = MailState.DeletedList;

        leftContainer.SetActive(true);
        emailListPanel.SetActive(false);
        if (deletedListPanel != null) deletedListPanel.SetActive(true); // Show deleted list
        emailPanel.SetActive(false);

        backButton.gameObject.SetActive(true); // Show back button to hide the deleted list
    }

    /// <summary>
    /// Called when an email is clicked to open the reading view.
    /// </summary>
    public void OpenEmailReadView(MailData mailData) {
        // Track which list we came from so the Back button works correctly later
        if (currentState == MailState.InboxList) currentState = MailState.ReadingInbox;
        else if (currentState == MailState.DeletedList) currentState = MailState.ReadingDeleted;

        emailListPanel.SetActive(false);
        if (deletedListPanel != null) deletedListPanel.SetActive(false);

        emailPanel.SetActive(true);
        backButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Evaluates the current state and steps backwards one level.
    /// </summary>
    private void OnBackButtonClicked() {
        switch (currentState) {
            case MailState.InboxList:
            case MailState.DeletedList:
                OpenHome(); // Closes the lists and returns to the Sidebar
                break;
            case MailState.ReadingInbox:
                OpenInbox(); // Closes the email and returns to Inbox list
                break;
            case MailState.ReadingDeleted:
                OpenDeleted(); // Closes the email and returns to Deleted list
                break;
        }
    }

    private void CloseMailWindow() {
        // Disables the entire mail window, returning the player to the desktop
        gameObject.SetActive(false);
    }
}