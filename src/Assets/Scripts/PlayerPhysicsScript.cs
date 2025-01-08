using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsScript : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputScript playerInputScript;
    [SerializeField]
    private Transform cameraTransform; // Reference to the camera's transform

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputScript = GetComponent<PlayerInputScript>();
    }

    private void FixedUpdate()
    {
        if (playerInputScript.ShouldWalk)
        {
            // Get the input direction from PlayerInputScript
            Vector3 inputDirection = playerInputScript.WalkingVector;

            // Adjust inputDirection to be relative to the camera's orientation
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Flatten the vectors to ignore vertical components
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // Normalize the vectors
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate the final movement direction
            Vector3 moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

            // Apply movement while preserving vertical velocity (e.g., gravity, jumps)
            Vector3 finalVelocity = moveDirection * 10f; // Adjust speed as necessary
            finalVelocity.y = rb.velocity.y;

            rb.velocity = finalVelocity; // Set velocity directly
        }
    }
}
