using UnityEngine;
[CreateAssetMenu(fileName = "MailData", menuName = "Scriptable Objects/Situation/MailData")]
public class MailData : TaskData {
    [Header("Mail Data")]
    [SerializeField] private string emailAddress = string.Empty;
    [SerializeField] private string emailHeader = string.Empty;
    [SerializeField] private string[] emailBody = new string[0];
    [SerializeField] private EnumDifficulty difficulty = EnumDifficulty.None;
    [SerializeField] private MailTypeEnum mailType = MailTypeEnum.None;

    // Public properties to access the private fields
    public string EmailAddress => emailAddress;
    public string EmailHeader => emailHeader;
    public string[] EmailBody => emailBody;
    public EnumDifficulty Difficulty => difficulty;
    public MailTypeEnum MailType => mailType;

    public MailData(string emailAddress, string emailHeader, string[] emailBody, MailTypeEnum mailType) {
        this.emailAddress = emailAddress;
        this.emailHeader = emailHeader;
        this.emailBody = emailBody;
        this.mailType = mailType;
    }

    [System.Serializable]
    public enum MailTypeEnum : int { None = 0, Company = 1, Personal = 2, Spam = 3, Scam = 4, }
}

