using Assets._Scripts;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    protected override bool IsPersistent => true;
    public CharacterController playerCharacterController => PlayerRefrenceProvider.Instance.PlayerCharacterController;
    public EnumPlayerState CurrentPlayerState = EnumPlayerState.Walking;

    [SerializeField] private Chair playerComputerChair;

    private void Start() {
        // lock the cursor to the center of the screen and make it invisible
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
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