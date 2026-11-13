using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public class DialogueSignal : MonoBehaviour {

    [SerializeField] private DialogueSystem dialogueSystem;
    [SerializeField] private UnityEvent OnDialogEnded;

    private void Awake() {
        if (this.dialogueSystem == null) {
            this.dialogueSystem = GetComponent<DialogueSystem>();
        }
    }

    private void OnEnable() {
        this.dialogueSystem.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable() {
        this.dialogueSystem.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueEnded() {
        this.OnDialogEnded?.Invoke();
    }
}
