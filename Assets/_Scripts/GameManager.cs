using Assets._Scripts;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected override bool IsPersistent => true;
    [SerializeField] private EnumPlayerState startingPlayerState = EnumPlayerState.Intro;
    private EnumPlayerState currentPlayerState = EnumPlayerState.None;
    private EnumPlayerState previousPlayerState = EnumPlayerState.None;
    public EnumPlayerState CurrentPlayerState {
        get => this.currentPlayerState;
        set => SetPlayerState(value);
    }
    [SerializeField] private Chair playerComputerChair; //TODO remove

    [SerializeField, Range(1, 50)] private float lookSensitivity = 2f;
    public float LookSensitivity {
        get => this.lookSensitivity;
        set => SetLookSensitivity(value);
    }

    //[SerializeField] private Dictionary<string, LevelData> levelDataLookup = new Dictionary<string, LevelData>();

    private void Start() {
        this.CurrentPlayerState = this.startingPlayerState;
        SetLookSensitivity(this.LookSensitivity);
    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation) {
        // Get the player's CharacterController refrence.
        CharacterController playerController = PlayerRefrenceProvider.Instance.PlayerCharacterController;

        // Disable the CharacterController to avoid collision issues during teleportation.
        playerController.enabled = false;

        // Set the player's position and rotation to the specified values.
        playerController.transform.SetPositionAndRotation(position, rotation);

        // Re-enable the CharacterController after teleportation.
        playerController.enabled = true;
    }

    public void SetPlayerState(EnumPlayerState newState) {
        if (newState == this.currentPlayerState) return;

        switch (newState) {
            case EnumPlayerState.None:
                return;

            case EnumPlayerState.Walking:
                // Lock the cursor and make it invisible when the player is in walking state.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Making sure the player walk camera is active when the player is in walking state.
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(true);
                Helper.EnableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);

                // Make the crosshair visible when the player is in walking state.
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = true;
                break;

            case EnumPlayerState.Sitting:

                // Unlock the cursor and make it visible when the player is in sitting state.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Making sure the player walk camera is inactive when the player is in sitting state.
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(false);

                // Make the crosshair invisible when the player is in sitting state.
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                break;

            case EnumPlayerState.Dialogue:
                // ADD THESE TWO LINES:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                Helper.DisableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);
                break;

            case EnumPlayerState.Cutscene:
                // Hide cursor and crosshair, disable camera rotation so they can't look around
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                Helper.DisableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);
                break;

            case EnumPlayerState.Intro:

                // Allow interacting with the introduction panel.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Keep the camera active behind the introduction.
                PlayerRefrenceProvider.Instance.PlayerWalkCamera
                    .gameObject.SetActive(true);

                // Hide the crosshair.
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                break;

            default:
                Debug.LogError($"Unknown player state: {newState}");
                break;

        }
        this.previousPlayerState = this.currentPlayerState;
        this.currentPlayerState = newState;
    }

    public void SetPlayerStateToPreviouseState() {
        this.SetPlayerState(this.previousPlayerState);
    }

    public void SetLookSensitivity(float newSensitivity) {
        CinemachineInputAxisController axisController = PlayerRefrenceProvider.instance.PlayerWalkCamera.GetComponent<CinemachineInputAxisController>();
        string xLookName = "Look X (Pan)";
        string yLookName = "Look Y (Tilt)";

        foreach (var componment in axisController.Controllers) {
            if (componment.Name == xLookName) {
                componment.Input.Gain = newSensitivity;

            } else if (componment.Name == yLookName) {
                componment.Input.Gain = -newSensitivity;

            }
        }
        this.lookSensitivity = newSensitivity;
    }
}

public enum EnumPlayerState { None = 0, Walking = 1, Sitting = 2, Intro = 3, Dialogue = 4, Cutscene = 5 }
public enum EnumDifficulty { None = 0, Easy = 1, Medium = 2, Hard = 3, };
[System.Serializable] public enum CommunicationTypeEnum : int { None = 0, Company = 1, Personal = 2, Spam = 3, Scam = 4, }