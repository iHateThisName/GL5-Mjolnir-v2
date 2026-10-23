using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CallData", menuName = "Scriptable Objects/Situation/CallData")]
public class CallData : TaskData, IDialogue {
    [Header("Call Data")]
    [SerializeField] private CommunicationTypeEnum calllType = CommunicationTypeEnum.None;
    [SerializeField] private AudioClip conversationAudio;

    [Header("Dialogue")]
    [SerializeField] private string characterName;
    [SerializeField] private Sprite portrait;
    [SerializeField, TextArea(3, 10)] private string[] subtitiles = Array.Empty<string>();
    public AudioClip ConversationAudio => conversationAudio;
    public CommunicationTypeEnum CallType => calllType;
    public string CharacterName => this.characterName;

    public Sprite Portrait => this.portrait;

    public string[] Lines => this.subtitiles;


    //[Header("Response Option One")]
    //// First option
    //[SerializeField] private AudioClip responesOptionOneAudio;
    //[SerializeField] private string buttonTextOptionOne = "Option 1";

    //public AudioClip ResponesOptionOneAudio => responesOptionOneAudio;
    //public string ButtonTextOptionOne => buttonTextOptionOne;

    //[Header("Response Option Two")]
    //// Second option
    //[SerializeField] private AudioClip responesOptionTwoAudio;
    //[SerializeField] private string buttonTextOptionTwo = "Option 2";

    //public AudioClip ResponesOptionTwoAudio => responesOptionTwoAudio;
    //public string ButtonTextOptionTwo => buttonTextOptionTwo;


}
