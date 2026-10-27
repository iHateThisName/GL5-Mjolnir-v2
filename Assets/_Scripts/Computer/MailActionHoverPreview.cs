using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MailActionHoverPreview :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private MailSelectedController mailController;
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private TMP_Text previewText;

    [Header("Debug")]
    [SerializeField] private bool logPointerEvents = true;

    private bool hovered;
    private bool configured;

    private void Awake()
    {
        configured =
            mailController != null &&
            previewPanel != null &&
            previewText != null;

        if (!configured)
        {
            Debug.LogError(
                "Hover preview needs a MailSelectedController, " +
                "Preview Panel, and Preview Text.",
                this);
            return;
        }

        // Hiding the preview must not hide this button or its parents.
        if (transform.IsChildOf(previewPanel.transform))
        {
            configured = false;

            Debug.LogError(
                "Preview Panel must be a separate UI object, " +
                "not this button or an ancestor of this button.",
                this);
            return;
        }

        previewText.richText = false;

        // The preview must not intercept the pointer from the button.
        CanvasGroup group = previewPanel.GetComponent<CanvasGroup>();

        if (group == null)
            group = previewPanel.AddComponent<CanvasGroup>();

        group.interactable = false;
        group.blocksRaycasts = false;

        HidePreview();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (logPointerEvents)
            Debug.Log("Pointer entered Action button.", this);

        hovered = true;
        RefreshPreview();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (logPointerEvents)
            Debug.Log("Pointer left Action button.", this);

        hovered = false;
        HidePreview();
    }

    private void LateUpdate()
    {
        if (hovered)
            RefreshPreview();
    }

    private void OnDisable()
    {
        hovered = false;
        HidePreview();
    }

    private void RefreshPreview()
    {
        if (!configured)
            return;

        MailData mail = mailController.CurrentMail;

        if (mail == null ||
            mailController.IsBusy ||
            string.IsNullOrWhiteSpace(mail.HoverPreviewUrl))
        {
            HidePreview();
            return;
        }

        previewText.text = mail.HoverPreviewUrl;
        previewPanel.SetActive(true);
    }

    private void HidePreview()
    {
        if (configured && previewPanel != null)
            previewPanel.SetActive(false);
    }
}