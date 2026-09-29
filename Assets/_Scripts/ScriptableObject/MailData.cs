using UnityEngine;
[CreateAssetMenu(fileName = "MailData", menuName = "Scriptable Objects/Situation/MailData")]
public class MailData : TaskData {
    [Header("Mail Data")]
    [SerializeField] private string emailAddress = string.Empty;
    [SerializeField] private string emailHeader = string.Empty;
    [SerializeField] private string emailBody = string.Empty;
    [SerializeField] private EnumDifficulty difficulty = EnumDifficulty.None;
    [SerializeField] private MailTypeEnum mailType = MailTypeEnum.None;
    public MailTypeEnum MailType => mailType;

    [System.Serializable]
    public enum MailTypeEnum : int { None = 0, Company = 1, Personal = 2, Spam = 3, Scam = 4, }
}

