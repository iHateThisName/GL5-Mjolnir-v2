using System.Collections;
using UnityEngine;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class WorldInteractable : MonoBehaviour, IInteractable {

    public event System.Action OnInteract;
    private static bool isInteractable = true;
    [SerializeField] private string hoverText = string.Empty;

    public string HoverText() => this.hoverText;
    public void Interact(GameObject interactor) {
        if (!isInteractable || !isActiveAndEnabled) return;

        OnInteract?.Invoke();
        StartCoroutine(Cooldown());

        Debug.Log($"{interactor.name} interacted with {gameObject.name}");
    }


    private IEnumerator Cooldown() {
        isInteractable = false;
        yield return new WaitForSeconds(Helper.InteractionCooldown);
        isInteractable = true;
    }
}