using UnityEngine;

public class InteractionPromptTarget : MonoBehaviour
{
    [SerializeField] private string displayName = "Object";

    [TextArea(1, 3)]
    [SerializeField]
    private string instruction =
        "Press Left Mouse Button to interact";

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? gameObject.name
            : displayName;

    public string Instruction => instruction;
}