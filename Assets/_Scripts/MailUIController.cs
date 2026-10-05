using UnityEngine;
using UnityEngine.UI;

public class MailUIController : MonoBehaviour
{
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

    private void OnEnable()
    {
        // Setup core navigation
        inboxButton.onClick.AddListener(OpenInbox);
        backButton.onClick.AddListener(OpenInbox);
        closeTabButton.onClick.AddListener(CloseMailWindow);
        deletedButton.onClick.AddListener(OpenDeleted);

        // Setup placeholder buttons
        settingsButton.onClick.AddListener(() => Debug.Log("Settings button clicked - Not implemented yet"));
        deletedButton.onClick.AddListener(() => Debug.Log("Deleted button clicked - Not implemented yet"));

        // Force the app to open the Inbox view by default when turned on
        OpenInbox();
    }

    private void OnDisable()
    {
        inboxButton.onClick.RemoveListener(OpenInbox);
        backButton.onClick.RemoveListener(OpenInbox);
        closeTabButton.onClick.RemoveListener(CloseMailWindow);
        settingsButton.onClick.RemoveAllListeners();
        deletedButton.onClick.RemoveAllListeners();
        deletedButton.onClick.RemoveListener(OpenDeleted);
    }

    /// <summary>
    /// Opens the Email List Panel and hides the reading view.
    /// </summary>
    public void OpenInbox()
    {
        leftContainer.SetActive(true);
        emailListPanel.SetActive(true);

        emailPanel.SetActive(false);
        backButton.gameObject.SetActive(false); // Hide back button on home screen
    }

    public void OpenDeleted()
    {
        leftContainer.SetActive(true);
        emailListPanel.SetActive(false); // Hide inbox
        if (deletedListPanel != null) deletedListPanel.SetActive(true);

        emailPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when an email is clicked to open the reading view.
    /// </summary>
    public void OpenEmailReadView(MailData mailData)
    {
        emailListPanel.SetActive(false);

        emailPanel.SetActive(true);
        backButton.gameObject.SetActive(true); // Show back button to return to list
    }

    private void CloseMailWindow()
    {
        // Disables the entire mail window, returning the player to the desktop
        gameObject.SetActive(false);
    }
}