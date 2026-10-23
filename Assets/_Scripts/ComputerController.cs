using Assets._Scripts;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>
public class ComputerController : MonoBehaviour {

    [SerializeField] private Chair chair;
    [SerializeField] private PhoneController phoneController;

    // Screen buttons
    [SerializeField] private Button powerButton;
    [SerializeField] private Button mailButton;
    [SerializeField] private Button calenderButton;
    [SerializeField] private EventSystem computerEventSystem;

    [Header("Windows")]
    [SerializeField] private GameObject WindowMailGameobject;
    [SerializeField] private GameObject WindowCalenderGameobject;

    private void Start() {
        // Checking if the scene have a event system.
        if (EventSystem.current == null) {
            this.computerEventSystem.gameObject.SetActive(true);
        }
    }

    private void OnEnable() {
        this.powerButton.onClick.AddListener(OnPowerButton);
        this.mailButton.onClick.AddListener(OnMailButton);
        this.calenderButton.onClick.AddListener(OnCalenderButton);
    }

    private void OnDisable() {
        this.powerButton.onClick.RemoveListener(OnPowerButton);
        this.mailButton.onClick.RemoveListener(OnMailButton);
        this.calenderButton.onClick.RemoveListener(OnCalenderButton);
    }

    public void OnPowerButton() {

        // Leveing the computer
        //isActive = false;
        this.chair.StandUp();
        if (this.phoneController != null) this.phoneController.EndPhoneCall();

        Debug.Log("Power button pressed! Leaving the computer.");
    }

    [ContextMenu("Window/Toggle New Mail Window")]
    public void OnMailButton() {
        ToggleComputerWindow(this.WindowMailGameobject);
    }

    [ContextMenu("Window/Toggle Calendar Window")]
    private void OnCalenderButton() {
        ToggleComputerWindow(this.WindowCalenderGameobject);
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
