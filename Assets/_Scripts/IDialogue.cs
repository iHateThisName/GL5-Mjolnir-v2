using UnityEngine;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public interface IDialogue {
    string CharacterName { get; }
    Sprite Portrait { get; }
    string[] Lines { get; }
}
