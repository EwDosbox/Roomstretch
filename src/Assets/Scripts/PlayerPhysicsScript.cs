using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsScript : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputScript playerInputScript;
    [SerializeField]
    private Transform cameraTransform; // Reference to the camera's transform

    [SerializeField]
    private float moveSpeed = 10f; // Max movement speed
    [SerializeField]
    private float acceleration = 20f; // Speed gain per second
    [SerializeField]
    private float deceleration = 30f; // Speed loss when stopping

    private Vector3 currentVelocity; // Stores velocity for smoothing

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputScript = GetComponent<PlayerInputScript>();
    }

    private void FixedUpdate()
    {
        // Get movement input
        Vector3 inputDirection = playerInputScript.ShouldWalk ? playerInputScript.WalkingVector : Vector3.zero;

        // Convert to camera-relative movement
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        Vector3 moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        // Calculate target velocity
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // Smooth movement using acceleration/deceleration
        if (moveDirection.magnitude > 0)
        {
            // Accelerate to target speed
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate smoothly when no input
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        // Preserve vertical velocity (gravity, jumping)
        currentVelocity.y = rb.velocity.y;

        // Apply velocity
        rb.velocity = currentVelocity;
    }
}
