using Unity.Cinemachine;
using UnityEngine;

public class PlayerRefrenceProvider : Singleton<PlayerRefrenceProvider> {
    [field:SerializeField] public CharacterController PlayerCharacterController { get; private set; }
    public Transform PlayerTransform => PlayerCharacterController.transform;
    [field:SerializeField] public Transform PlayerHeadTransform {  get; private set; }
    [SerializeField] private MovementController playerMovementController;
    public MovementController PlayerMovementController => GetMovementController();
    public CinemachineCamera PlayerWalkCamera => PlayerMovementController.PlayerCamera;

    private MovementController GetMovementController() {
        if (playerMovementController == null) {
            this.playerMovementController = PlayerCharacterController.GetComponent<MovementController>();
        }
        return playerMovementController;
    }

}
