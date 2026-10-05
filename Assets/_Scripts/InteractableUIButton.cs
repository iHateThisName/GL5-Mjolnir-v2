using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InteractableUIButton : MonoBehaviour, IInteractable
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Interact(GameObject interactor)
    {
        // When your 3D player raycast hits the Box Collider and calls Interact(), 
        // we force the UI button to click itself!
        button.onClick.Invoke();
    }
}