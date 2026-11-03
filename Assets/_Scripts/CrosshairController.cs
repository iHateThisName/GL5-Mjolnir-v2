using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class CrosshairController : MonoBehaviour {

    [SerializeField] private TMP_Text crooshairPromtText;
    private bool isHoveringTextDisplayed = false;

    private void Update() {
        if (IsHoveringInteractable(out string hoverText)) {
            StartCoroutine(DisplayCrosshairPromptText(hoverText));
        } else if (isHoveringTextDisplayed) {
            StartCoroutine(FadeOutCrosshairPromtText());
        }
    }

    private IEnumerator DisplayCrosshairPromptText(string hoverText) {
        if (this.isHoveringTextDisplayed && this.crooshairPromtText.text == hoverText) yield break; // If the text is already displayed and matches the hoverText, do nothing
        StopAllCoroutines();
        this.crooshairPromtText.text = hoverText;
        this.isHoveringTextDisplayed = true;

    }

    private IEnumerator FadeOutCrosshairPromtText() {
        yield break;
        this.crooshairPromtText.text = string.Empty;
        this.isHoveringTextDisplayed = false;
    }

    private bool IsHoveringInteractable(out string hoverText) {
        hoverText = string.Empty;
        // Check if the player is hovering a IInteractable object and update the crosshair accordingly

        if (EventSystem.current.IsPointerOverGameObject()) return false; // return if hovering over a UI element

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f)) {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable)) {
                hoverText = interactable.HoverText();
                return true;
            }
        }
        return false;
    }
}
