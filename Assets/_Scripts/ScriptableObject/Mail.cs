using UnityEngine;

[CreateAssetMenu(fileName = "Mail", menuName = "Scriptable Objects/Mail")]
public class Mail : ScriptableObject {
    [Header("Email Information")]
    public string EmailAddress;
    public string EmailSubject;
    public string EmailBody;
    public EnumMailType EmailType;

    public enum EnumMailType : int {
        None = 0,
        Company = 1,
        Personal = 2,
        Spam = 3,
        Scam = 4,
    }
}
