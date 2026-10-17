using UnityEngine;
using UnityEngine.UI;

public class DirectoryPanelController : MonoBehaviour
{
    [SerializeField] private Button directoryButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject directoryPanel;

    private bool configured;

    private void Awake()
    {
        if (directoryButton == null || directoryPanel == null)
        {
            Debug.LogError(
                "DirectoryPanelController requires a Directory Button " +
                "and Directory Panel.",
                this);
            return;
        }

        // The controller must remain active when the panel is hidden.
        if (transform.IsChildOf(directoryPanel.transform))
        {
            Debug.LogError(
                "Put DirectoryPanelController outside DirectoryPanel, " +
                "for example on MailControllers.",
                this);
            return;
        }

        configured = true;
        directoryPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (!configured)
            return;

        directoryButton.onClick.AddListener(ToggleDirectory);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDirectory);
    }

    private void OnDisable()
    {
        if (directoryButton != null)
            directoryButton.onClick.RemoveListener(ToggleDirectory);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseDirectory);

        CloseDirectory();
    }

    public void ToggleDirectory()
    {
        if (!configured)
            return;

        directoryPanel.SetActive(!directoryPanel.activeSelf);
    }

    public void CloseDirectory()
    {
        if (!configured)
            return;

        directoryPanel.SetActive(false);
    }
}