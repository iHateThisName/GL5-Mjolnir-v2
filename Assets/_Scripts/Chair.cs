using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

namespace Assets._Scripts {
    public class Chair : MonoBehaviour, IInteractable {
        [SerializeField] private Transform sitPosition;
        [SerializeField] private CinemachineCamera lockedCamera;
        public Vector3 playerStandPosition;
        public Quaternion playerRotation;

        public UnityEvent onPlayerSatDown;
        public async void Interact(GameObject interactor) {
            if (GameManager.Instance.CurrentPlayerState == EnumPlayerState.Sitting) {
                Debug.Log("Already sitting. Cannot sit down.");
                return;
            }

            // refrence to be used later.
            CinemachineBrain cameraBrain = Camera.main.GetComponent<CinemachineBrain>();

            // Store the stand position.
            this.playerStandPosition = interactor.transform.root.position;
            this.playerRotation = interactor.transform.root.rotation;

            // Starting the camera blend
            lockedCamera.gameObject.SetActive(true); // enable the locked sitting camera
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Sitting; // disables the walking camera

            // Wait for the blend to happen
            await Awaitable.NextFrameAsync();

            while (cameraBrain.IsBlending) {
                await Awaitable.NextFrameAsync();
            }

            // Telporting the player
            GameManager.Instance.TeleportPlayer(this.sitPosition.position, this.sitPosition.rotation);

            //Fire event
            onPlayerSatDown?.Invoke();
        }

        public void StandUp() {
            GameManager.Instance.TeleportPlayer(this.playerStandPosition, this.playerRotation);
            lockedCamera.gameObject.SetActive(false);
            GameManager.Instance.CurrentPlayerState = EnumPlayerState.Walking;
        }
    }
}