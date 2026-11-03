using UnityEngine;

public interface IInteractable {
    void Interact(GameObject interactor);
    string HoverText() => string.Empty;
}
