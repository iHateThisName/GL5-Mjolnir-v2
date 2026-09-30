using Assets._Scripts;
using Eflatun.SceneReference;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {
    protected override bool IsPersistent => true;
    private EnumPlayerState currentPlayerState = EnumPlayerState.None;
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
        this.CurrentPlayerState = EnumPlayerState.Walking;
        SetLookSensitivity(this.LookSensitivity);
    }

    //public void OnLoadeLevel() {
    //    string sceneName = SceneManager.GetActiveScene().name;

    //    if(this.levelDataLookup.TryGetValue(sceneName, out LevelData levelData)) {

    //        foreach (ConditionTracker.ConditionState conditionState in levelData.InitialConditions) {
    //            ConditionTracker.Instance.SetCondition(conditionState);
    //        }

    //        foreach (SituationData situationData in levelData.Situations) {
    //            // Give the situation data to the SituationManager to handle
    //        }
    //    }

    //}

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

        switch (newState) {
            case EnumPlayerState.Walking:
                this.currentPlayerState = EnumPlayerState.Walking;

                // Lock the cursor and make it invisible when the player is in walking state.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Making sure the player walk camera is active when the player is in walking state.
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(true);
                break;

            case EnumPlayerState.Sitting:
                this.currentPlayerState = EnumPlayerState.Sitting;

                // Unlock the cursor and make it visible when the player is in sitting state.
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Making sure the player walk camera is inactive when the player is in sitting state.
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(false);
                break;

            default:
                Debug.LogError($"Unknown player state: {newState}");
                break;

        }
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

public enum EnumPlayerState { None = 0, Walking = 1, Sitting = 2, }
public enum EnumDifficulty { None = 0, Easy = 1, Medium = 2, Hard = 3, };