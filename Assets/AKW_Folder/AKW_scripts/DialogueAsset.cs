using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAsset", menuName = "Scriptable Objects/DialogueAsset"), System.Serializable]
public class DialogueAsset : SituationData
{
    [Header("Character name")]
    public string Name;

    [Header("Dialogue")]
    [TextArea] public string[] Lines;

}