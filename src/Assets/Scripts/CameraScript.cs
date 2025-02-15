using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera camComponent;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private DNDFileData save;

    [SerializeField]
    private float baseHorizontalSensitivity;
    [SerializeField]
    private float baseVerticalSensitivity;

    [SerializeField]
    private float inputThreshold;

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        camComponent = GetComponent<Camera>();
        playerInputScript = player.GetComponent<PlayerInputScript>();
        camComponent.fieldOfView = save.Settings.FOV;
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(0f, 1.2f, 0f);

        float fovScale = save.Settings.FOV / 60f;
        float horizontalSensitivity = baseHorizontalSensitivity * fovScale;
        float verticalSensitivity = baseVerticalSensitivity * fovScale;

        Vector2 lookInput = playerInputScript.LookInput;

        if (Mathf.Abs(lookInput.x) > inputThreshold || Mathf.Abs(lookInput.y) > inputThreshold)
        {
            float mouseX = lookInput.x * horizontalSensitivity * Time.deltaTime;
            float mouseY = lookInput.y * verticalSensitivity * Time.deltaTime;

            verticalRotation = Mathf.Clamp(verticalRotation - mouseY, -50f, 50f);
            horizontalRotation += mouseX;

            transform.localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
        }

        camComponent.fieldOfView = save.Settings.FOV;
    }
}
