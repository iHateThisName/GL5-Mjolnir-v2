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
        NpcCollider.OnInteract += showDialogue;
        //ShowDialogue();
        //dialogueCanvas.SetActive(true);
        nameText.text = name;
        //characterPortrait.GetComponent(image.sourceImage) = dialogue.Portrait; //To change the npc portrait (work in progress)
        dialogueText.text = dialogue.Lines[dialogIndex];
        dialoguePanel.SetActive(true);
    }

    private void OnDisable()
    {
        NpcCollider.OnInteract -= showDialogue;
        //dialogueCanvas.SetActive(false);
        nameText.text = null;
        dialogueText.text = null;
        dialoguePanel.SetActive(false);
    }

    private void showDialogue()
    {

        // Check if the dialog is finished
        if (dialogIndex >= dialogue.Lines.Length)
        {
            EndDialogue();
            dialogIndex = 0;
            dialogueCanvas.SetActive(false);
        }
        else
        {
            // Show the dialog line
            string line = dialogue.Lines[dialogIndex];
            ShowDialogue(dialogue: line, name: dialogue.Name);
            dialogueCanvas.SetActive(true);

            dialogIndex++; // tell it to go to next line 
        }
    }

    public void ShowDialogue(string dialogue, string name)
    {
        nameText.text = name;
        dialogueText.text = dialogue;
        dialoguePanel.SetActive(true);
    }

    public void EndDialogue() // to reset the dialogue box for the next?
    {
        nameText.text = null;
        dialogueText.text = null; ;
        dialoguePanel.SetActive(false);
    }
}
