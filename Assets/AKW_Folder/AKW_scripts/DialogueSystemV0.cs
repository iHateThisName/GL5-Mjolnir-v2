using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueSystemV0 : MonoBehaviour
{
    [Header("")]
    [SerializeField] WorldInteractable NpcCollider;

    [Header("Dialogue stuff")]
    public DialogueAsset dialogue;
    private int dialogIndex = 0;
    [Header("")]
    [SerializeField] GameObject dialogueCanvas;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject characterPortrait;

    private void OnEnable()
    {
        NpcCollider.OnInteract += ShowDialogue;
        //dialogueCanvas.SetActive(true);
        nameText.text = name;
        dialogueText.text = dialogue.Lines[dialogIndex];
        dialoguePanel.SetActive(true);
    }

    private void OnDisable()
    {
        NpcCollider.OnInteract -= ShowDialogue;
        //dialogueCanvas.SetActive(false);
        nameText.text = null;
        dialogueText.text = null; ;
        dialoguePanel.SetActive(false);
    }

    private void ShowDialogue()
    {
        Debug.Log(dialogue.Lines[dialogIndex]);
        dialogIndex++;
    }

    //public void ShowDialogue(string dialogue, string name)
    //{
    //    nameText.text = name + "...";
    //    dialogueText.text = dialogue;
    //    dialoguePanel.SetActive(true);
    //}

    //public void EndDialogue()
    //{
    //    nameText.text = null;
    //    dialogueText.text = null; ;
    //    dialoguePanel.SetActive(false);
    //}
}
