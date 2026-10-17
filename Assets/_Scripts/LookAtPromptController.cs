using TMPro;
using UnityEngine;

public class LookAtPromptController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float interactionDistance = 2.5f;
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField]
    private QueryTriggerInteraction triggerInteraction =
        QueryTriggerInteraction.Ignore;

    private InteractionPromptTarget currentTarget;
    private bool configured;

    private void Awake()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        if (viewCamera == null ||
            promptPanel == null ||
            promptText == null)
        {
            Debug.LogError(
                "LookAtPromptController requires a Camera, " +
                "Prompt Panel, and Prompt Text.",
                this);

            enabled = false;
            return;
        }

        // Hiding the panel must not disable this controller.
        if (transform.IsChildOf(promptPanel.transform))
        {
            Debug.LogError(
                "Put LookAtPromptController outside PromptPanel, " +
                "for example on the HUD root.",
                this);

            enabled = false;
            return;
        }

        configured = true;
        promptText.richText = false;

        HidePrompt();
    }

    private void LateUpdate()
    {
        if (!configured)
            return;

        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentPlayerState !=
                EnumPlayerState.Walking ||
            Cursor.lockState != CursorLockMode.Locked ||
            Time.timeScale <= 0f)
        {
            HidePrompt();
            return;
        }

        Ray ray = viewCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                raycastLayers,
                triggerInteraction))
        {
            HidePrompt();
            return;
        }

        InteractionPromptTarget target =
            hit.collider.GetComponentInParent<InteractionPromptTarget>();

        if (target == null || !target.isActiveAndEnabled)
        {
            HidePrompt();
            return;
        }

        ShowPrompt(target);
    }

    private void ShowPrompt(InteractionPromptTarget target)
    {
        if (currentTarget == target && promptPanel.activeSelf)
            return;

        currentTarget = target;

        promptText.text = string.IsNullOrWhiteSpace(target.Instruction)
            ? target.DisplayName
            : $"{target.DisplayName}\n{target.Instruction}";

        promptPanel.SetActive(true);
    }

    private void HidePrompt()
    {
        currentTarget = null;

        if (configured && promptPanel.activeSelf)
            promptPanel.SetActive(false);
    }

    private void OnDisable()
    {
        HidePrompt();
    }
}