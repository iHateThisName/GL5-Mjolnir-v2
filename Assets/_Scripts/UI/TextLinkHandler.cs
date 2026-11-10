using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class TextLinkHandler : MonoBehaviour, IPointerClickHandler {

    private TMP_Text text;
    private Camera mainCamera;

    // Link Ids
    private const string RICK_ROLL_LINK_ID = "RickRoll";
    private const string RICK_ROLL_UNDERSTAND_LINK_ID = "RickRollUnderstand";

    private void Start() {
        this.text = this.GetComponent<TMP_Text>();
        this.mainCamera = Camera.main;
    }
    public void OnPointerClick(PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.text, eventData.position, null);

        if (linkIndex == -1) return;

        TMP_LinkInfo linkInfo = this.text.textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();

        OnLinkPressed(linkIndex);

    }

    private void OnLinkPressed(int linkIndex) {
        TMP_LinkInfo linkInfo = this.text.textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();

        switch (linkId) {
            case RICK_ROLL_LINK_ID:
                Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                break;

            case RICK_ROLL_UNDERSTAND_LINK_ID:
                Debug.Log("I understand");
                break;

            default:
                Debug.LogWarning($"Unhandled link ID: {linkId}, link text: {linkInfo.GetLinkText()}");
                break;

        }
    }
    private string GetHoverText(int linkIndex) {
        if (linkIndex == -1) return string.Empty;

        string linkId = this.text.textInfo.linkInfo[linkIndex].GetLinkID();

        switch (linkId) {
            case RICK_ROLL_LINK_ID:
                return "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            case RICK_ROLL_UNDERSTAND_LINK_ID:
                return "I understand";

            default:
                Debug.LogWarning($"Unhandled link ID: {linkId}, link text: {this.text.textInfo.linkInfo[linkIndex].GetLinkText()}");
                break;

        }
        return string.Empty;

    }

    public bool IsHoveringLink(out string hoverText) {
        hoverText = string.Empty;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.text, Mouse.current.position.ReadValue(), this.mainCamera);
        if (linkIndex == -1) return false;

        hoverText = GetHoverText(linkIndex);
        return true;
    }
}
