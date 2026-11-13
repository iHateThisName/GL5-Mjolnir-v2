using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MailData", menuName = "Scriptable Objects/Situation/MailData")]
public class MailData : TaskData
{
    [Header("Sender")]
    [SerializeField] private string displayName = string.Empty;
    [SerializeField] private string emailAddress = string.Empty;

    [Header("Content")]
    [SerializeField] private string emailHeader = string.Empty;
    [TextArea(3, 10)]
    [SerializeField] private string[] emailBody = Array.Empty<string>();
    [SerializeField] private EnumDifficulty difficulty = EnumDifficulty.None;
    [SerializeField] private CommunicationTypeEnum mailType = CommunicationTypeEnum.None;

    [Header("Attachments")]
    [Tooltip("List of attachment file names (e.g., 'Invoice.pdf'). Leave empty if no attachments.")]
    [SerializeField] private string[] attachmentFiles = Array.Empty<string>();

    [Header("Primary Action")]
    [SerializeField] private string actionButtonLabel = "ACTION";
    [Tooltip("Actual destination for a future hover preview. Not opened by this system.")]
    [SerializeField] private string hoverPreviewUrl = string.Empty;

    [Header("Answer Key")]
    [SerializeField] private bool isScam;
    [SerializeField] private MailRedFlag[] correctRedFlags = Array.Empty<MailRedFlag>();

    [Header("Successful Outcome Reply")]
    [Tooltip("Scam: thank-you monologue. Legitimate: order confirmation. Success-only text.")]
    [TextArea(3, 10)]
    [SerializeField] private string feedbackReplyText = string.Empty;

    [Header("Meeting Reveal")]
    [TextArea(3, 10)]
    [SerializeField] private string[] annotatedEmailBody = Array.Empty<string>();
    [SerializeField] private string annotatedEmailAddress = string.Empty;

    [Header("Meeting Reveal (Scams Only)")]
    [Tooltip("Leave empty if this is a legitimate email.")]
    [SerializeField] private GameObject ruinedCoworkerPrefab;
    [Tooltip("The specific dialogue the Boss says when this scam is failed.")]
    [SerializeField] private SituationData scoldingDialogue;

    [Header("Boss Meeting (False Reports)")]
    [Tooltip("Dialogue when player falsely reports this legitimate email.")]
    [SerializeField] private SituationData falseReportDialogue;

    [Header("Success Rewards")]
    [Tooltip("What the Boss says when you handle this correctly.")]
    [SerializeField] private SituationData successDialogue;

    [Tooltip("The prefab to spawn on your desk (e.g., a plant, a mug).")]
    [SerializeField] private GameObject rewardPrefab;

    public GameObject RuinedCoworkerPrefab => this.ruinedCoworkerPrefab;
    public SituationData ScoldingDialogue => this.scoldingDialogue;
    public SituationData FalseReportDialogue => this.falseReportDialogue;
    public SituationData SuccessDialogue => this.successDialogue;
    public GameObject RewardPrefab => this.rewardPrefab;

    public string DisplayName => displayName;
    public string EmailAddress => emailAddress;
    public string EmailHeader => emailHeader;
    public IReadOnlyList<string> EmailBody => Array.AsReadOnly(emailBody ?? Array.Empty<string>());
    public IReadOnlyList<string> AttachmentFiles => Array.AsReadOnly(attachmentFiles ?? Array.Empty<string>());
    public EnumDifficulty Difficulty => difficulty;
    public CommunicationTypeEnum MailType => mailType;
    public string ActionButtonLabel => string.IsNullOrWhiteSpace(actionButtonLabel) ? "ACTION" : actionButtonLabel;
    public string HoverPreviewUrl => hoverPreviewUrl;
    public bool IsScam => isScam;
    public IReadOnlyList<MailRedFlag> CorrectRedFlags => Array.AsReadOnly(correctRedFlags ?? Array.Empty<MailRedFlag>());
    public string FeedbackReplyText => feedbackReplyText;
    public IReadOnlyList<string> AnnotatedEmailBody => annotatedEmailBody != null && annotatedEmailBody.Length > 0 ? Array.AsReadOnly(annotatedEmailBody) : EmailBody;
    public string AnnotatedEmailAddress => string.IsNullOrWhiteSpace(annotatedEmailAddress) ? emailAddress : annotatedEmailAddress;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!isScam && correctRedFlags != null && correctRedFlags.Length > 0)
            Debug.LogWarning($"{name}: legitimate mail should have no red flags.", this);
        if (isScam && (correctRedFlags == null || correctRedFlags.Length == 0))
            Debug.LogWarning($"{name}: scam has no evidence configured; checklist maximum will be zero.", this);
    }
#endif
}