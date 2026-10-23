using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAsset", menuName = "Scriptable Objects/DialogueAsset"), System.Serializable]
public class DialogueAsset : SituationData, IDialogue
{
    [Header("Character portrait")]
    public Sprite Portrait;

    [Header("Character name")]
    public string Name;

    [Header("Dialogue")]
    [TextArea] public string[] Lines;

    string IDialogue.CharacterName => Name;

    Sprite IDialogue.Portrait => Portrait;

    string[] IDialogue.Lines => Lines;
}