using System.Collections;
using UnityEngine;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class WorldInteractable : MonoBehaviour, IInteractable {

    public event System.Action OnInteract;
    private static bool isInteractable = true;
    private const float interactionCooldown = 0.5f; // Cooldown duration in seconds
    [SerializeField] private string hoverText = string.Empty;
    public string HoverText() => this.hoverText;
    public void Interact(GameObject interactor) {
        if (!isInteractable) return;

        OnInteract?.Invoke();
        StartCoroutine(Cooldown());

        Debug.Log($"{interactor.name} interacted with {gameObject.name}");
    }


    private IEnumerator Cooldown() {
        isInteractable = false;
        yield return new WaitForSeconds(interactionCooldown);
        isInteractable = true;
    }
}