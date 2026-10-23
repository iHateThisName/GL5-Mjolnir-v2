using UnityEngine;

public interface IDialogue {
    string CharacterName { get; }
    Sprite Portrait { get; }
    string[] Lines { get; }
}
