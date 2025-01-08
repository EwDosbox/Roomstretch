using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    private bool goLeft = false, goRight = false, goForward = false, goBack = false;
    private Vector2 lookInput;

    public Vector2 LookInput
    {
        get
        {
            return lookInput;
        }
    }

    public bool ShouldWalk
    {
        get
        {
            return goLeft || goRight || goForward || goBack;
        }
    }

    public Vector3 WalkingVector
    {
        get
        {
            Vector3 walkingVector = Vector3.zero;
            if (goRight)
            {
                walkingVector.x = 1;
            }
            else if (goLeft)
            {
                walkingVector.x = -1;
            }

            if (goForward)
            {
                walkingVector.z = 1;
            }
            else if (goBack)
            {
                walkingVector.z = -1;
            }

            return walkingVector.normalized; // Normalize to ensure consistent speed in diagonal movement
        }
    }


#region InputAction Methods

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
    #endregion
    #region Camera
    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    #endregion

}
