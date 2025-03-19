using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera camComponent;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private DNDFileData save;

    [SerializeField]
    private float baseHorizontalSensitivity = 1f;
    [SerializeField]
    private float baseVerticalSensitivity = 1f;

    [SerializeField]
    private float inputThreshold = 0.1f;

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();
        playerInputScript = player.GetComponent<PlayerInputScript>();

        if (save != null && save.Settings != null)
        {
            camComponent.fieldOfView = save.Settings.FOV;
        }
    }

private void LateUpdate()
{
    if (player == null || playerInputScript == null || save == null || save.Settings == null)
        return;

    transform.position = player.transform.position + new Vector3(0f, 1f, 0f);

    // Calculate sensitivities
    float horizontalSensitivity = baseHorizontalSensitivity * save.Settings.Sensitivity;
    float verticalSensitivity = baseVerticalSensitivity * save.Settings.Sensitivity;

    // Clamp sensitivities
    horizontalSensitivity = Mathf.Clamp(horizontalSensitivity, 0.1f, 10f);
    verticalSensitivity = Mathf.Clamp(verticalSensitivity, 0.1f, 10f);

    // Get input
    Vector2 lookInput = playerInputScript.LookInput;

    // Apply a deadzone to prevent drifting
    if (Mathf.Abs(lookInput.x) < inputThreshold) lookInput.x = 0f;
    if (Mathf.Abs(lookInput.y) < inputThreshold) lookInput.y = 0f;

    float mouseX = lookInput.x * horizontalSensitivity * Time.deltaTime;
    float mouseY = lookInput.y * verticalSensitivity * Time.deltaTime;

    // Apply rotation
    verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -89f, 89f);
    horizontalRotation += mouseX;

    transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    camComponent.fieldOfView = save.Settings.FOV;
}

}
