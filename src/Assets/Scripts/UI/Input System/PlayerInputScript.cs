using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    private Rigidbody rb;

    private Dictionary<string, float> Speeds = new Dictionary<string, float>();

    private bool goLeft = false, goRight = false, goForward = false, goBack = false;
    private void Awake()
    {
        Speeds.Add("Forward", 10);
        Speeds.Add("Back", -5);
        Speeds.Add("Left",-8);
        Speeds.Add("Right", 8);

        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 walkingVector = Vector3.zero;

        if (goLeft)
        {
            walkingVector.x = Speeds["Left"];
            Debug.Log("Left");
        }
        else if (goRight)
        {
            walkingVector.x = Speeds["Right"];
            Debug.Log("Right");
        }

        if (goForward)
        {
            walkingVector.y = Speeds["Forward"];
            Debug.Log("Forward");
        }
        else if (goBack)
        {
            walkingVector.y = Speeds["Back"];
            Debug.Log("Back");
        }

        rb.AddForce(walkingVector, ForceMode.Impulse);
    }

    //InputAction Methods

    public void Forward(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            goForward = true;
        }
        else if (context.canceled)
        {
            goForward = false;
        }
    }
    public void Backward(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            goBack = true;
        }
        else if (context.canceled)
        {
            goBack = false;
        }
    }
    public void Left(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            goLeft = true;
        }
        else if (context.canceled)
        {
            goLeft = false;
        }
    }
    public void Right(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            goRight = true;
        }
        else if (context.canceled)
        {
            goRight = false;
        }
    }
}
