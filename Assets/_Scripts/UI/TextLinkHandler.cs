using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TextLinkHandler : MonoBehaviour, IPointerClickHandler
{

    public event Action<string> OnLinkClickedEvent;

    private TMP_Text text;
    private Camera mainCamera;

    // Dynamic Hover Prefix (New)
    private const string HOVER_PREFIX = "Hover_";

    // Standard Link Ids
    private const string RICK_ROLL_LINK_ID = "RickRoll";
    private const string RICK_ROLL_UNDERSTAND_LINK_ID = "RickRollUnderstand";

    //Day2
    private const string LAW_FIRM_ORDER_LINK_ID = "LawFirmOrder";
    private const string FAKE_BOSS_BONUS_LINK_ID = "FakeBossBonus";
    private const string CUSTOMS_FEE_SCAM_LINK_ID = "CustomsFeeScam";
    private const string BRUCE_RECEIPT_LINK_ID = "BruceReceipt";
    private const string BIFROST_BUDGET_LINK_ID = "BifrostBudget";

    // Day 3
    private const string SKATTEETATEN_VAT_LINK_ID = "SkatteetatenVAT";

    private void Awake()
    {
        this.text = this.GetComponent<TMP_Text>();
        this.mainCamera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Camera eventCamera = eventData.pressEventCamera ?? mainCamera;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.text, eventData.position, eventCamera);

        if (linkIndex == -1) return;

        OnLinkPressed(linkIndex);
    }

    private void OnLinkPressed(int linkIndex)
    {
        TMP_LinkInfo linkInfo = this.text.textInfo.linkInfo[linkIndex];
        string linkId = linkInfo.GetLinkID();

        // Check if this is a hover-only link (like a Sender Address). If so, do nothing on click!
        if (linkId.StartsWith(HOVER_PREFIX))
        {
            return;
        }

        // Broadcast the click to the MailSelectedController for actionable links
        OnLinkClickedEvent?.Invoke(linkId);

        switch (linkId)
        {
            case RICK_ROLL_LINK_ID:
                Application.OpenURL("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
                break;
            case RICK_ROLL_UNDERSTAND_LINK_ID:
                Debug.Log("I understand");
                break;
            case LAW_FIRM_ORDER_LINK_ID:
                Debug.Log("https://intranet.mjolnir-office.no/ord510");
                break;
            case FAKE_BOSS_BONUS_LINK_ID:
                Debug.Log("http://mj0lnir-login-steal.xyz/auth");
                break;
            case CUSTOMS_FEE_SCAM_LINK_ID:
                Debug.Log("http://steal-credit-card.ru/pay");
                break;
            case BRUCE_RECEIPT_LINK_ID:
                Debug.Log("https://sporing.posten.no/receipt-882");
                break;
            case BIFROST_BUDGET_LINK_ID:
                Debug.Log("http://free-robux-malware-download.net/hack");
                break;

            // Day 3
            case SKATTEETATEN_VAT_LINK_ID:
                Debug.Log("https://altinn.skatteetaten-innlogging.no/auth");
                break;
            default:
                Debug.LogWarning($"Unhandled link ID: {linkId}, link text: {linkInfo.GetLinkText()}");
                break;
        }
    }

    private string GetHoverText(int linkIndex)
    {
        if (linkIndex == -1) return string.Empty;

        string linkId = this.text.textInfo.linkInfo[linkIndex].GetLinkID();

        // If it's a dynamic hover tag, return everything after "Hover_"
        if (linkId.StartsWith(HOVER_PREFIX))
        {
            return $"<{linkId.Substring(HOVER_PREFIX.Length)}>";
        }

        switch (linkId)
        {
            case RICK_ROLL_LINK_ID:
                return "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            case RICK_ROLL_UNDERSTAND_LINK_ID:
                return "I understand";
            case LAW_FIRM_ORDER_LINK_ID:
                return "https://intranet.mjolnir-office.no/ord510";
            case FAKE_BOSS_BONUS_LINK_ID:
                return "http://mj0lnir-login-steal.xyz/auth";
            case CUSTOMS_FEE_SCAM_LINK_ID:
                return "http://steal-credit-card.ru/pay";
            case BRUCE_RECEIPT_LINK_ID:
                return "https://sporing.posten.no/receipt-882";
            case BIFROST_BUDGET_LINK_ID:
                return "http://free-robux-malware-download.net/hack";
            // Day 3
            case SKATTEETATEN_VAT_LINK_ID:
                return "https://altinn.skatteetaten-innlogging.no/auth";
            default:
                Debug.LogWarning($"Unhandled link ID: {linkId}, link text: {this.text.textInfo.linkInfo[linkIndex].GetLinkText()}");
                break;
        }
        return string.Empty;
    }

    public bool IsHoveringLink(out string hoverText)
    {
        hoverText = string.Empty;
        if (Mouse.current == null) return false;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.text, Mouse.current.position.ReadValue(), this.mainCamera);
        if (linkIndex == -1) return false;

        hoverText = GetHoverText(linkIndex);
        return true;
    }
}