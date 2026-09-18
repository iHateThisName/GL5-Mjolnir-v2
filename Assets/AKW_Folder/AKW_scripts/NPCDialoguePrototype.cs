using UnityEngine;

public class NPCDialoguePrototype : MonoBehaviour
{

    [SerializeField] WorldInteractable NpcCollider;
    public GameObject DialogueBox;

    public void OnEnable()
    {
        NpcCollider.OnInteract += OnNpcInteraction;
    }

    public void OnDisable()
    {
        NpcCollider.OnInteract -= OnNpcInteraction;
    }
    public async void OnNpcInteraction()
    {
        Debug.Log("NPC Interaction");
        await Awaitable.WaitForSecondsAsync(0.1f);
        if (DialogueBox.activeSelf == false)
        {
            DialogueBox.SetActive(true);
            Debug.Log("NPC Interaction is going on");            
        }
        else
        {
            DialogueBox.SetActive(false);
            Debug.Log("NPC Interaction is going on");
        }
    }   
}
