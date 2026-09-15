using UnityEngine;

public class WorldInteractable : MonoBehaviour, IInteractable {

    public event System.Action OnInteract;
    public void Interact(GameObject interactor) {
        OnInteract?.Invoke();
        Debug.Log($"{interactor.name} interacted with {gameObject.name}");
    }
}
