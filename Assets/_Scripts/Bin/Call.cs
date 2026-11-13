using UnityEngine;

[CreateAssetMenu(fileName = "Call", menuName = "Scriptable Objects/Call"), System.Serializable]
public class Call : ScriptableObject {
    [Header("Call Information")]
    public AudioClip CallAudio;
    public EnumCallType CallType;

    public enum EnumCallType : int {
        None = 0,
        Company = 1,
        Personal = 2,
        Spam = 3,
        Scam = 4,
    }
}
