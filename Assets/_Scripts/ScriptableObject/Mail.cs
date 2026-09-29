using UnityEngine;

[CreateAssetMenu(fileName = "Mail", menuName = "Scriptable Objects/Mail"), System.Serializable]
public class Mail : ScriptableObject {
    [Header("Email Information")]
    public string EmailAddress;
    public string EmailSubject;
    public string EmailBody;
    public MailData.MailTypeEnum EmailType;
}
