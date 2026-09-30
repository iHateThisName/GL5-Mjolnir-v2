using Assets._Scripts;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ComputerController : MonoBehaviour {

    [SerializeField] private Chair chair;
    [SerializeField] private PhoneController phoneController;

    // Screen buttons
    [SerializeField] private Button powerButton;
    [SerializeField] private Button mailButton;
    [SerializeField] private Button newMailButton;
    [SerializeField] private EventSystem computerEventSystem;

    [Header("Windows")]
    [SerializeField] private GameObject WindowMailGameobject;
    [SerializeField] private GameObject NewWindowMailGameobject;
    private void Start() {
        // Checking if the scene have a event system.
        if (EventSystem.current == null) {
            this.computerEventSystem.gameObject.SetActive(true);
        }
    }

    private void OnEnable() {
        this.powerButton.onClick.AddListener(OnPowerButton);
        this.mailButton.onClick.AddListener(OnMailButton);
        this.newMailButton.onClick.AddListener(OnNewMailButton);
    }

    private void OnDisable() {
        this.powerButton.onClick.RemoveListener(OnPowerButton);
        this.mailButton.onClick.RemoveListener(OnMailButton);
        this.newMailButton.onClick.RemoveListener(OnNewMailButton);
    }

    public void OnPowerButton() {

        // Leveing the computer
        //isActive = false;
        this.chair.StandUp();
        if (this.phoneController != null) this.phoneController.EndPhoneCall();

        Debug.Log("Power button pressed! Leaving the computer.");
    }

    [ContextMenu("Window/Toggle Mail Window")]
    public void OnMailButton() {
        ToggleComputerWindow(this.WindowMailGameobject);
    }

    [ContextMenu("Window/Toggle New Mail Window")]
    public void OnNewMailButton() {
        ToggleComputerWindow(this.NewWindowMailGameobject);
    }

    /// <summary>
    /// Default window behaviour.
    /// </summary>
    private void ToggleComputerWindow(GameObject window) {

        // Toggle the window
        window.SetActive(!window.activeSelf);

        // Move on the top of other windows
        window.transform.SetAsLastSibling();

        Debug.Log($"{window.name} pressed. Toffling the window");
    }
}
