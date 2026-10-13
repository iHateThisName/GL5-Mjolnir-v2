using UnityEngine;

[CreateAssetMenu(fileName = "CallData", menuName = "Scriptable Objects/Situation/CallData")]
public class CallData : TaskData {
    [Header("Call Data")]
    [SerializeField] private CommunicationTypeEnum calllType = CommunicationTypeEnum.None;
    [SerializeField] private AudioClip conversationAudio;
    public AudioClip ConversationAudio => conversationAudio;
    public CommunicationTypeEnum CallType => calllType;

    [Header("Response Option One")]
    // First option
    [SerializeField] private AudioClip responesOptionOneAudio;
    [SerializeField] private string buttonTextOptionOne = "Option 1";

    public AudioClip ResponesOptionOneAudio => responesOptionOneAudio;
    public string ButtonTextOptionOne => buttonTextOptionOne;

    [Header("Response Option Two")]
    // Second option
    [SerializeField] private AudioClip responesOptionTwoAudio;
    [SerializeField] private string buttonTextOptionTwo = "Option 2";

    public AudioClip ResponesOptionTwoAudio => responesOptionTwoAudio;
    public string ButtonTextOptionTwo => buttonTextOptionTwo;


}
