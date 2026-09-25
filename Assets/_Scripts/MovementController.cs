using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour {
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerVisualTransform;
    [SerializeField] private Transform playerHeadVisualTransform;
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference interactActionReference;
    [SerializeField] private InputActionReference attackActionReference;
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 moveInput;

    private void Start() {
        if (this.characterController == null) {
            this.characterController = this.GetComponentInParent<CharacterController>();
        }

    }
    private void OnEnable() {
        this.moveActionReference.action.Enable();
        this.moveActionReference.action.performed += OnMove;
        this.moveActionReference.action.canceled += OnMove;

        this.attackActionReference.action.Enable();
        this.attackActionReference.action.performed += OnInteract;
    }

    private void OnDisable() {
        this.moveActionReference.action.performed -= OnMove;
        this.moveActionReference.action.canceled -= OnMove;
        this.attackActionReference.action.performed -= OnInteract;
    }


    private void Update() {
        // rotate the charater controller y-axis based on the camera's y rotation
        this.characterController.transform.rotation = Quaternion.Euler(
            0f,
            this.playerCamera.transform.rotation.eulerAngles.y,
            0f
        );

        this.playerHeadVisualTransform.rotation = Quaternion.Euler(
            this.playerCamera.transform.rotation.eulerAngles.x,
            this.playerHeadVisualTransform.rotation.eulerAngles.y,
            this.playerHeadVisualTransform.rotation.eulerAngles.z
        );

        if (GameManager.Instance.CurrentPlayerState == EnumPlayerState.Walking) {
            HandleMove(this.moveInput);
        }
    }
    private void OnInteract(InputAction.CallbackContext context) {
        Debug.Log("Interact action performed");

        // Check if pointer is over a UI element. If so, ignore the interaction.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f)) {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable)) {
                interactable.Interact(gameObject);
            }
        }

        Debug.DrawRay(ray.origin, ray.direction * 3f, Color.red, 5f);
    }
    private void OnMove(InputAction.CallbackContext context) => this.moveInput = context.ReadValue<Vector2>();
    private void HandleMove(Vector2 moveInput) {
        Vector3 moveDirection =
            transform.forward * moveInput.y +
            transform.right * moveInput.x;

        this.characterController.Move(
            this.moveSpeed * Time.deltaTime * moveDirection
        );
    }
}
