using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPCDialoguePrototype2 : MonoBehaviour
{
    [SerializeField] WorldInteractable NpcCollider;

    public List<GameObject> DialogueBoxes;

    private int currentDialogueBox = -1;
    //private int oldDialogueBox = -2;

    public UnityEvent onDialogueFinished = new UnityEvent();

    public void OnEnable()
    {
        NpcCollider.OnInteract += OnNpcInteraction;
    }

    public void OnDisable()
    {
        NpcCollider.OnInteract -= OnNpcInteraction;
    }
    public void OnNpcInteraction()
    {
        //Debug.Log("NPC Interaction 2, electric boogaloo");
        Debug.Log($"Dialogue with {gameObject}, have started");
        currentDialogueBox += 1;

        DialogueBoxes[currentDialogueBox - 1].gameObject.SetActive(false);
        DialogueBoxes[currentDialogueBox].gameObject.SetActive(true);

        if (currentDialogueBox > DialogueBoxes.Count - 1)
        {
            DialogueBoxes[currentDialogueBox].gameObject.SetActive(false);
            currentDialogueBox = -1;
            Debug.Log($"Dialogue with {gameObject.name} ended");
        }
    }
}
