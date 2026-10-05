using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Assets._Scripts {
    public class Chair : MonoBehaviour, IInteractable {
        [SerializeField] private Transform sitPosition;
        [SerializeField] private CinemachineCamera lockedCamera;
        public Vector3 playerStandPosition;
        public Quaternion playerRotation;

        public UnityEvent onPlayerSatDown;
        [SerializeField] private BoxCollider boxCollider;
        public async void Interact(GameObject interactor) {
            if (GameManager.Instance.CurrentPlayerState != EnumPlayerState.Walking) return;

            //Disable the box collider
            this.boxCollider.enabled = false;

            // refrence to be used later.
            CinemachineBrain cameraBrain = Camera.main.GetComponent<CinemachineBrain>();

            // Store the stand position.
            this.playerStandPosition = interactor.transform.root.position;
            this.playerRotation = interactor.transform.root.rotation;

            // Starting the camera blend
            this.lockedCamera.gameObject.SetActive(true); // enable the locked sitting camera
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Sitting; // disables the walking camera

            // Wait for the blend to happen
            await Awaitable.NextFrameAsync();

            while (cameraBrain.IsBlending) {
                await Awaitable.NextFrameAsync();
            }

            // Check if the player is still in the sitting state before teleporting
            if (GameManager.Instance.CurrentPlayerState != EnumPlayerState.Sitting) return;

            // Telporting the player
            GameManager.Instance.TeleportPlayer(this.sitPosition.position, this.sitPosition.rotation);

            //Fire event
            onPlayerSatDown?.Invoke();
        }

        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame) {
                if (GameManager.Instance.CurrentPlayerState == EnumPlayerState.Sitting) {
                    StandUp();
                }
            }
        }

        public void StandUp() {
            if (GameManager.Instance.CurrentPlayerState != EnumPlayerState.Sitting) return;

            GameManager.Instance.TeleportPlayer(this.playerStandPosition, this.playerRotation);
            this.lockedCamera.gameObject.SetActive(false);
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Walking;

            //Disable the box collider
            this.boxCollider.enabled = true;
        }
    }
}