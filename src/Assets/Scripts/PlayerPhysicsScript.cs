using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerPhysicsScript : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputScript = GetComponent<PlayerInputScript>();
    }
    private void FixedUpdate()
    {
        Vector3 walkingVector = playerInputScript.WalkingVector * 100;

        rb.AddForce(walkingVector, ForceMode.Impulse);
    }
}
