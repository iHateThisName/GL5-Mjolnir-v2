using Unity.Cinemachine;
using UnityEngine;

namespace Assets._Scripts {
    public class Chair : MonoBehaviour, IInteractable {
        [SerializeField] private Transform sitPosition;
        [SerializeField] private CinemachineCamera lockedCamera;
        [SerializeField] private CinemachineCamera WalkCamera;
        public Vector3 playerStandPosition;
        public Quaternion playerRotation;
        public void Interact(GameObject interactor) {
            Debug.Log($"{interactor.name} interacted with {gameObject.name}");
            if (GameManager.Instance.CurrentPlayerState == EnumPlayerState.UsingComputer) {
                Debug.Log("Already using a computer. Cannot sit down.");
                return;
            }
            MovementController controller = interactor.GetComponent<MovementController>();
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.UsingComputer;
            this.playerStandPosition = interactor.transform.root.position;
            this.playerRotation = interactor.transform.root.rotation;


            GameManager.Instance.TeleportPlayer(sitPosition.position, sitPosition.rotation);
            lockedCamera.gameObject.SetActive(true);
            this.WalkCamera.gameObject.SetActive(false);
            PlayerRefrenceProvider.Instance.PlayerHeadTransform.localRotation = Quaternion.identity;

        }

        public void StandUp() {
            GameManager.Instance.TeleportPlayer(this.playerStandPosition, this.playerRotation);
            lockedCamera.gameObject.SetActive(false);
            this.WalkCamera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Walking;

        }
    }
}