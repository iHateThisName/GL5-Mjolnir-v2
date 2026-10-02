using UnityEngine;
[CreateAssetMenu(fileName = "MailData", menuName = "Scriptable Objects/Situation/MailData")]
public class MailData : TaskData {
    [Header("Mail Data")]
    [SerializeField] private string emailAddress = string.Empty;
    [SerializeField] private string emailHeader = string.Empty;

    [TextArea(3, 10)]
    [SerializeField] private string[] emailBody = new string[0];

    [SerializeField] private EnumDifficulty difficulty = EnumDifficulty.None;
    [SerializeField] private MailTypeEnum mailType = MailTypeEnum.None;

    [Header("Correct Choice")]
    [SerializeField] private bool isReplyCorrect = true;
    [SerializeField] private bool isDeleteCorrect = false;
    [SerializeField] private bool isLinkCorrect = false;

    // Public properties to access the private fields
    public string EmailAddress => emailAddress;
    public string EmailHeader => emailHeader;
    public string[] EmailBody => emailBody;
    public EnumDifficulty Difficulty => difficulty;
    public MailTypeEnum MailType => mailType;

    public bool IsReplyCorrect => isReplyCorrect;
    public bool IsDeleteCorrect => isDeleteCorrect;
    public bool IsLinkCorrect => isLinkCorrect;

    [Header("TV Reveal Data")]

    [Tooltip("Highlight and copy the tags you need from this box!")]
    [TextArea(5, 6)]
    [SerializeField]
    private string tagCheatSheet =
            "--- COPY TAGS FROM HERE ---\n" +
            "Spelling Error: <color=red>typo</color>\n" +
            "Suspicious Idea: <mark=#ff000055>trap</mark>\n" +
            "Fake Link: <color=#0055FF><u><link=\"virus\">fake.com</link></u></color>";

    [TextArea(3, 10)]
    [SerializeField] private string[] annotatedEmailBody = new string[0];
    [SerializeField] private string annotatedEmailAddress = string.Empty;

    private void Reset()
    {
        tagCheatSheet =
            "--- COPY TAGS FROM HERE ---\n" +
            "Spelling Error: <color=red>typo</color>\n" +
            "Suspicious Idea: <mark=#ff000055>trap</mark>\n" +
            "Fake Link: <color=#0055FF><u><link=\"virus\">fake.com</link></u></color>";
    }

    // Getters that fall back to the normal text if the annotated fields are left empty
    public string[] AnnotatedEmailBody => annotatedEmailBody.Length > 0 ? annotatedEmailBody : emailBody;
    public string AnnotatedEmailAddress => string.IsNullOrEmpty(annotatedEmailAddress) ? emailAddress : annotatedEmailAddress;

    public MailData(string emailAddress, string emailHeader, string[] emailBody, MailTypeEnum mailType) {
        this.emailAddress = emailAddress;
        this.emailHeader = emailHeader;
        this.emailBody = emailBody;
        this.mailType = mailType;
    }

    [System.Serializable]
    public enum MailTypeEnum : int { None = 0, Company = 1, Personal = 2, Spam = 3, Scam = 4, }
}

