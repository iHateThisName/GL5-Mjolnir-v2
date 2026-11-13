using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class CrosshairController : MonoBehaviour {

    [SerializeField] private TMP_Text crooshairPromtText;
    [SerializeField] private CanvasGroup crosshairTextCanvasGroup;
    [SerializeField] private Image crosshairFillImage;
    [SerializeField] private RectTransform rootCrooshairTransform;
    private bool isHoverTextDisplayed = false;
    private bool isHoveringInteractable = false;
    private bool isPointerOverButton = false; // Track if the pointer is over a button

    [SerializeField] private float fadeSpeed = 0.5f; // Adjust the speed of the fade-in and fade-out effects

    private void Start() {
        this.crosshairFillImage.enabled = false;
        this.crosshairTextCanvasGroup.alpha = 0f;
    }
    private void Update() {
        if (Cursor.lockState == CursorLockMode.Locked) {
            this.rootCrooshairTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        } else {
            this.rootCrooshairTransform.position = Mouse.current.position.ReadValue();
        }

        if (IsHoveringInteractable(out string hoverText)) {
            if (!this.isHoveringInteractable || (this.crooshairPromtText.text != hoverText)) {
                this.isHoveringInteractable = true;
                this.crosshairFillImage.enabled = true;
                this.crooshairPromtText.text = hoverText;

                int numberOfCharacters = hoverText.Length;
                Debug.Log($"Number of characters in the prompt text: {numberOfCharacters}");

                RectTransform rectTransform = crosshairTextCanvasGroup.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(85 + (numberOfCharacters > 10 ? (numberOfCharacters * 9) : 0), rectTransform.sizeDelta.y); // Adjust the width based on character count
            }

            if (this.crosshairTextCanvasGroup.alpha < 1f && !string.IsNullOrEmpty(hoverText)) {
                this.crosshairTextCanvasGroup.alpha = Mathf.MoveTowards(this.crosshairTextCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
            } else if (string.IsNullOrEmpty(hoverText) && this.crosshairTextCanvasGroup.alpha != 0) {
                this.crosshairTextCanvasGroup.alpha = 0f;
            }
        } else {
            if (this.isHoveringInteractable) {
                this.isHoveringInteractable = false;
                this.crosshairFillImage.enabled = false;
                this.crooshairPromtText.text = string.Empty;
            }

            if (this.crosshairTextCanvasGroup.alpha > 0f) {
                this.crosshairTextCanvasGroup.alpha = Mathf.MoveTowards(this.crosshairTextCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
            }
        }
    }

    private IEnumerator DisplayCrosshairPromptText(string hoverText) {
        if (this.isHoverTextDisplayed && this.crooshairPromtText.text == hoverText) yield break; // If the text is already displayed and matches the hoverText, do nothing
        StopAllCoroutines();

        this.isHoverTextDisplayed = true;
        this.crooshairPromtText.text = hoverText;

        // slowly fade in the text
        while (this.crosshairTextCanvasGroup.alpha < 1f) {
            this.crosshairTextCanvasGroup.alpha += Time.deltaTime * fadeSpeed; // Adjust the speed of the fade-in effect
            yield return null;
        }

        this.crosshairTextCanvasGroup.alpha = 1f; // Ensure the alpha is set to 1 after fading in

    }

    private IEnumerator FadeOutCrosshairPromtText() {
        StopAllCoroutines();

        // slowly fade out the text
        while (this.crosshairTextCanvasGroup.alpha > 0f) {
            this.crosshairTextCanvasGroup.alpha -= Time.deltaTime * fadeSpeed; // Adjust the speed of the fade-out effect
            yield return null;
        }
        this.crooshairPromtText.text = string.Empty;
        this.isHoverTextDisplayed = false;
        yield break;
    }

    private bool IsHoveringInteractable(out string hoverText) {
        hoverText = string.Empty;
        // Check if the player is hovering a IInteractable object and update the crosshair accordingly

        if (EventSystem.current.IsPointerOverGameObject()) {
            // If the pointer is over a UI element, check if it's a button and get the hover text from it
            return IsHoveringButton(out hoverText); // Return true if hovering over a button, otherwise false
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f)) {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable)) {
                // check if the interactable is enabled and active.
                if (interactable is WorldInteractable worldInteractable && !worldInteractable.isActiveAndEnabled) {
                    return false;
                }
                hoverText = interactable.HoverText();
                return true;
            }
        }
        return false;
    }

    private bool IsHoveringButton(out string hoverText) {
        hoverText = string.Empty;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new();

        EventSystem.current.RaycastAll(pointerData, results);
        bool isPointerOverButton = false;
        foreach (RaycastResult result in results) {
            Button button = result.gameObject.GetComponentInParent<Button>();

            if (button == null) {
                button = result.gameObject.GetComponent<Button>();
            }
            
            if (button != null) {
                if (DialogueSystem.IsDialogueActive) {
                    // Do not allow a button interaction when the dialog is happening on the dialog button is allow to interact with
                    if (button == DialogueSystem.CurrentDialogueButton) {
                        isPointerOverButton = true; // If the button is found, return true
                    }
                } else {
                    isPointerOverButton = true; // If the button is found and not in a dialogue, return true
                }
            }

            TextLinkHandler textLinkHandler = result.gameObject.GetComponent<TextLinkHandler>();
            if (textLinkHandler != null && textLinkHandler.IsHoveringLink(out hoverText)) {
                return true;
            }
        }

        return isPointerOverButton;
    }
}
