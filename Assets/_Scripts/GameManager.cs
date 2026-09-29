using Assets._Scripts;
using Eflatun.SceneReference;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : Singleton<GameManager> {
    protected override bool IsPersistent => true;
    public CharacterController playerCharacterController => PlayerRefrenceProvider.Instance.PlayerCharacterController;
    public EnumPlayerState CurrentPlayerState = EnumPlayerState.Walking;

    [SerializeField] private Chair playerComputerChair; //TODO remove

    [SerializeField] private Dictionary<string, LevelData> levelDataLookup = new Dictionary<string, LevelData>();

    private void Start() {
        // lock the cursor to the center of the screen and make it invisible
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void OnLoadeLevel() {
        string sceneName = SceneManager.GetActiveScene().name;

        if(this.levelDataLookup.TryGetValue(sceneName, out LevelData levelData)) {

            foreach (ConditionTracker.ConditionState conditionState in levelData.InitialConditions) {
                ConditionTracker.Instance.SetCondition(conditionState);
            }

            foreach (SituationData situationData in levelData.Situations) {
                // Give the situation data to the SituationManager to handle
            }
        }

    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation) {
        this.playerCharacterController.enabled = false;

        this.playerCharacterController.transform.SetPositionAndRotation(
            position,
            rotation
        );

        this.playerCharacterController.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"Player teleported to {position} with rotation {rotation.eulerAngles}");
    }
}

public enum EnumPlayerState {
    None = 0,
    Walking = 1,
    UsingComputer = 2,
}

public enum EnumDifficulty { None = 0, Easy = 1, Medium = 2, Hard = 3, }