using Microsoft.CodeAnalysis.CSharp.Syntax;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour {
    public event System.Action OnDialogueEnded;

    [Header("")]
    [SerializeField] private WorldInteractable npcCollider;

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

    [SerializeField] private Button dialoguePanelButton;
    [SerializeField] private bool AutoExitDialogueState = true;

    public bool DisableInteraction = false;

    private static IDialogue CurrentDialogue;
    public static bool IsDialogueActive => CurrentDialogue != null;
    public static Button CurrentDialogueButton { get; private set; }

    private void OnEnable() {
        if (this.npcCollider != null) {
            this.npcCollider.OnInteract += this.OnInteractionShowDialogue;
        }

        this.dialoguePanelButton.onClick.AddListener(() => this.AdvanceDialogue(this.SituationData as IDialogue));
    }

    private void OnDisable() {
        if (this.npcCollider != null) {
            this.npcCollider.OnInteract -= this.OnInteractionShowDialogue;
        }

        this.nameText.text = null;
        this.dialogueText.text = null;
        this.dialoguePanel.SetActive(false);

        this.dialoguePanelButton.onClick.RemoveAllListeners();
    }

    private void Awake() {
        this.dialogueCanvas.SetActive(false);
        CurrentDialogue = null;
        CurrentDialogueButton = null;
    }

    private void Start() {
        this.Initlize();
    }

    public void Initlize(bool disableInteraction = false) {
        if (this.SituationData == null) return;
        this.dialogue = this.SituationData as IDialogue;
        this.nameText.text = this.dialogue.CharacterName;
        this.dialogueText.text = this.dialogue.Lines[this.dialogIndex];
        this.dialoguePanel.SetActive(true);
        this.DisableInteraction = disableInteraction;

        if (this.dialogue.Portrait != null) this.characterPortrait.sprite = this.dialogue.Portrait;
    }

    private void OnInteractionShowDialogue() {
        if (this.DisableInteraction) return;
        else if (npcCollider.isActiveAndEnabled) {
            npcCollider.enabled = false; // Disable the collider to prevent further interactions during dialogue
            this.AdvanceDialogue(this.SituationData as IDialogue);
        }
    }

    public void SetDialogue(SituationData situationData, bool disableInteraction = false) {
        if (situationData == null) {
            Debug.LogWarning($"The new situation data is null.");
            return;
        }
        //Check if the new situation data is a IDialogue, if not, log an error and return
        if (situationData is not IDialogue) {
            Debug.LogError($"The new situation data {situationData.name} is not a IDialogue.");
            return;
        }

        this.SituationData = situationData;
        this.dialogIndex = 0;
        this.Initlize(disableInteraction: disableInteraction);
    }

    public void StartDialogue() {
        this.AdvanceDialogue(this.dialogue);
    }

    public void AdvanceDialogue(IDialogue dialogue) {
        if (dialogue == null) {
            if (this.SituationData == null) {
                Debug.LogError($"No situation data available for {this.gameObject.name}");
            }
            this.dialogue = this.SituationData as IDialogue;
            dialogue = this.dialogue;

            if (dialogue == null) {
                Debug.LogError($"No dialogue available for {this.gameObject.name}");
                return;
            }
        }

        if (CurrentDialogue == null) {
            CurrentDialogue = dialogue;
            CurrentDialogueButton = this.dialoguePanelButton;
        } else if (CurrentDialogue != dialogue) {
            Debug.LogWarning($"Trying to advance dialogue for {dialogue.CharacterName} while another dialogue is active.");
            return;
        }

        if (this.dialogIndex >= dialogue.Lines.Length) {
            this.EndDialogue();
            this.dialogIndex = 0;
            this.dialogueCanvas.SetActive(false);

        } else {
            GameManager.Instance.SetPlayerState(EnumPlayerState.Dialogue);

            string line = dialogue.Lines[this.dialogIndex];
            this.ShowDialogue(dialogue: line, name: dialogue.CharacterName);
            this.dialogueCanvas.SetActive(true);

            this.dialogIndex++;
        }
    }

    public void ShowDialogue(string dialogue, string name) {
        this.nameText.text = name;
        this.dialogueText.text = dialogue;
        this.dialoguePanel.SetActive(true);
    }

    public void EndDialogue() {
        this.nameText.text = null;
        this.dialogueText.text = null;
        this.dialoguePanel.SetActive(false);
        this.dialogueCanvas.SetActive(false);
        CurrentDialogue = null;
        CurrentDialogueButton = null;

        if (this.npcCollider != null) {
            this.npcCollider.enabled = true;
        }

        // Set the state to completed
        if (this.SituationData != null) {
            SituationManager.Instance.SetSituationState(SituationManager.SituationStateEnum.Success, this.SituationData);
        }

        if (this.AutoExitDialogueState) {
            GameManager.Instance.SetPlayerStateToPreviouseState();
        }

        this.OnDialogueEnded?.Invoke();
    }
}