using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameObject cam;
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private float sensitivity = 5f; // Adjust sensitivity for mouse input

    private float pitch = 0f; // Up/down rotation
    private float yaw = 0f;   // Left/right rotation
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        cam = this.gameObject;
        playerInputScript = player.GetComponent<PlayerInputScript>();
    }

    private void LateUpdate()
    {
        // Follow player position
        cam.transform.position = player.transform.position;

        // Rotate camera based on input
        Vector2 lookInput = playerInputScript.LookInput;
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch - mouseY, -90f, 90f); // Clamp vertical rotation
        yaw += mouseX; // Accumulate horizontal rotation

        // Apply rotations to the camera
        cam.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
