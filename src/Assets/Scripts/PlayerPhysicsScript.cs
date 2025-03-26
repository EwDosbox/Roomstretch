using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsScript : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;
    
    private Rigidbody rb;
    private PlayerInputScript playerInputScript;

    private Vector3 currentVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputScript = GetComponent<PlayerInputScript>();
    }

    private void FixedUpdate()
    {
        Vector3 inputDirection = playerInputScript.ShouldWalk ? playerInputScript.WalkingVector : Vector3.zero;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        Vector3 moveDirection = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

        Vector3 targetVelocity = moveDirection * moveSpeed;

        if (moveDirection.magnitude > 0)
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        else
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);

        currentVelocity.y = rb.velocity.y;

        rb.velocity = currentVelocity;
    }
}
