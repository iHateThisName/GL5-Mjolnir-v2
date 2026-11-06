using Assets._Scripts;
using Unity.Cinemachine;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    protected override bool IsPersistent => true;

    [SerializeField] private EnumPlayerState startingPlayerState = EnumPlayerState.Intro;
    private EnumPlayerState currentPlayerState = EnumPlayerState.None;

    public EnumPlayerState CurrentPlayerState
    {
        get => this.currentPlayerState;
        set => SetPlayerState(value);
    }

    [SerializeField, Range(1, 50)] private float lookSensitivity = 2f;
    public float LookSensitivity
    {
        get => this.lookSensitivity;
        set => SetLookSensitivity(value);
    }

    private void Start()
    {
        this.CurrentPlayerState = this.startingPlayerState;
        SetLookSensitivity(this.LookSensitivity);
        Cursor.visible = false;
    }

    public void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        // Get the player's CharacterController refrence.
        CharacterController playerController = PlayerRefrenceProvider.Instance.PlayerCharacterController;

        // Disable the CharacterController to avoid collision issues during teleportation.
        playerController.enabled = false;

        // Set the player's position and rotation to the specified values.
        playerController.transform.SetPositionAndRotation(position, rotation);

        // Re-enable the CharacterController after teleportation.
        playerController.enabled = true;
    }

    public void SetPlayerState(EnumPlayerState newState)
    {
        if (newState == this.currentPlayerState) return;

        switch (newState)
        {
            case EnumPlayerState.None:
                return;

            case EnumPlayerState.Walking:
                Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(true);
                Helper.EnableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = true;
                break;

            case EnumPlayerState.Sitting:
                Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(false);
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                break;

            case EnumPlayerState.Dialogue:
                Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                Helper.DisableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);
                break;

            case EnumPlayerState.Cutscene:
                Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                Helper.DisableCameraRotation(PlayerRefrenceProvider.Instance.PlayerWalkCamera);
                break;

            case EnumPlayerState.Intro:
                Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                PlayerRefrenceProvider.Instance.PlayerWalkCamera.gameObject.SetActive(true);
                PlayerRefrenceProvider.Instance.PlayerCrosshair.enabled = false;
                break;

            default:
                Debug.LogError($"Unknown player state: {newState}");
                break;

        }

        this.currentPlayerState = newState;
    }

    public void SetPlayerStateToPreviouseState()
    {
        // By bypassing state history entirely, we prevent spam-clicking bugs.
        // We use physical reality as the source of truth for our base gameplay states:
        if (Chair.ActiveChair != null)
        {
            this.SetPlayerState(EnumPlayerState.Sitting);
        }
        else
        {
            this.SetPlayerState(EnumPlayerState.Walking);
        }
    }

    public void SetLookSensitivity(float newSensitivity)
    {
        CinemachineInputAxisController axisController = PlayerRefrenceProvider.instance.PlayerWalkCamera.GetComponent<CinemachineInputAxisController>();
        string xLookName = "Look X (Pan)";
        string yLookName = "Look Y (Tilt)";

        foreach (var componment in axisController.Controllers)
        {
            if (componment.Name == xLookName)
            {
                componment.Input.Gain = newSensitivity;

            }
            else if (componment.Name == yLookName)
            {
                componment.Input.Gain = -newSensitivity;

            }
        }
        this.lookSensitivity = newSensitivity;
    }
}

public enum EnumPlayerState { None = 0, Walking = 1, Sitting = 2, Intro = 3, Dialogue = 4, Cutscene = 5 }
public enum EnumDifficulty { None = 0, Easy = 1, Medium = 2, Hard = 3, };
[System.Serializable] public enum CommunicationTypeEnum : int { None = 0, Company = 1, Personal = 2, Spam = 3, Scam = 4, }