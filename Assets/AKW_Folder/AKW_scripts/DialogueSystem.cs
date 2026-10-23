using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("")]
    [SerializeField] WorldInteractable NpcCollider;

    [Header("Dialogue stuff")]
    public SituationData SituationData;
    private IDialogue dialogue;
    private int dialogIndex = 0;
    [Header("")]
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image characterPortrait;

    public bool DisableInteraction = false;


    private void OnEnable()
    {
        NpcCollider.OnInteract += OnInteractionShowDialogue;

    }

    private void OnDisable()
    {
        NpcCollider.OnInteract -= OnInteractionShowDialogue;

        nameText.text = null;
        dialogueText.text = null;
        dialoguePanel.SetActive(false);
    }

    private void Start() {
        Initlize();
    }

    public void Initlize(bool disableInteraction = false) {
        if (this.SituationData == null) return;
        this.dialogue = this.SituationData as IDialogue;
        nameText.text = this.dialogue.CharacterName;
        dialogueText.text = this.dialogue.Lines[dialogIndex];
        dialoguePanel.SetActive(true);
        DisableInteraction = disableInteraction;

        if (this.dialogue.Portrait != null) this.characterPortrait.sprite = this.dialogue.Portrait;

    }

    private void OnInteractionShowDialogue() {

        if (DisableInteraction) return;
        else AdvanceDialogue(this.SituationData as IDialogue);
    }

    public void SetDialogue(SituationData situationData, bool disableInteraction = false) {
        this.SituationData = situationData;
        dialogIndex = 0;
        Initlize(disableInteraction: disableInteraction);
        AdvanceDialogue(this.SituationData as IDialogue);
    }

    public void AdvanceDialogue(IDialogue dialogue) {
        if (dialogue == null) {
            if (this.SituationData == null) Debug.LogError($"No situation data available for {gameObject.name}");
            this.dialogue = this.SituationData as IDialogue;
            dialogue = this.dialogue;
        }
        // Check if the dialog is finished
        if (dialogIndex >= dialogue.Lines.Length) {
            EndDialogue();
            dialogIndex = 0;
            dialogueCanvas.SetActive(false);
        } else {
            // Show the dialog line
            string line = dialogue.Lines[dialogIndex];
            ShowDialogue(dialogue: line, name: dialogue.CharacterName);
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
        this.dialogueCanvas.SetActive(false);
    }
}
