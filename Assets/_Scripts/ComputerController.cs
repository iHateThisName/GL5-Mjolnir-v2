using Assets._Scripts;
using UnityEngine;
using UnityEngine.UI;

public class ComputerController : MonoBehaviour {

    public bool isActive = true;
    [SerializeField] private Chair chair;
    [SerializeField] private PhoneController phoneController;

    // Screen buttons
    [SerializeField] private Button powerButton;
    [SerializeField] private Button mailButton;

    [Header("Windows")]
    [SerializeField] private GameObject WindowMailGameobject;
    public void OnTestDebug() => Debug.Log("Test button pressed!");
    public void OnPowerButton() {
        if (!this.isActive) return;

        // Leveing the computer
        //isActive = false;
        this.chair.StandUp();
        if (this.phoneController != null) this.phoneController.EndPhoneCall();

        Debug.Log("Power button pressed! Leaving the computer.");
    }

    [ContextMenu("Window/Toggle Mail Window")]
    public void OnMailButton() {
        if (!this.isActive) return;

        // Toggle the mail window
        this.WindowMailGameobject.SetActive(!this.WindowMailGameobject.activeSelf);

        Debug.Log("Mail button pressed! Toggling the mail window.");
    }
}
