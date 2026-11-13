using UnityEngine;

/// <summary>
/// Author: https://github.com/iHateThisName
/// </summary>

public static class Helper {

    public const float InteractionCooldown = 0.5f; // Cooldown duration in seconds
    public static void DisableCameraRotation(Unity.Cinemachine.CinemachineCamera camera) {
        camera.GetComponent<Unity.Cinemachine.CinemachineInputAxisController>().enabled = false;
    }

    public static void EnableCameraRotation(Unity.Cinemachine.CinemachineCamera camera) {
        camera.GetComponent<Unity.Cinemachine.CinemachineInputAxisController>().enabled = true;
    }
}
