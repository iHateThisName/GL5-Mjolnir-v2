using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAsset", menuName = "Scriptable Objects/DialogueAsset"), System.Serializable]
public class DialogueAsset : SituationData
{
    [Header("Dialogue")]
    [TextArea] public string[] Lines;

}