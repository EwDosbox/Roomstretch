using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    private bool goLeft = false, goRight = false, goForward = false, goBack = false;
    private Vector2 lookInput = Vector2.zero;
    private bool shouldBeInMenu = false;
    private bool shouldTeleport = false;

    public Vector2 LookInput => lookInput;
    public bool ShouldWalk => goLeft || goRight || goForward || goBack;
    public Vector3 WalkingVector
    {
        get
        {
            Vector3 walkingVector = Vector3.zero;
            if (goRight) walkingVector.x = 1;
            else if (goLeft) walkingVector.x = -1;

            if (goForward) walkingVector.z = 1;
            else if (goBack) walkingVector.z = -1;

            return walkingVector.normalized; // Normalize to ensure consistent speed in diagonal movement
        }
    }
    public bool ShouldBeInMenu => shouldBeInMenu;
    public bool ShouldTeleport => shouldTeleport;

    private void Awake()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();

        // Enable both "Movement" and "UI" action maps
        playerInput.actions.FindActionMap("Movement").Enable();
        playerInput.actions.FindActionMap("UI").Enable();
    }
    #region InputAction Methods
    public void Forward(InputAction.CallbackContext context)
    {
        if (context.started) goForward = true;
        else if (context.canceled) goForward = false;
    }
    public void Backward(InputAction.CallbackContext context)
    {
        if (context.started) goBack = true;
        else if (context.canceled) goBack = false;
    }
    public void Left(InputAction.CallbackContext context)
    {
        if (context.started) goLeft = true;
        else if (context.canceled) goLeft = false;
    }
    public void Right(InputAction.CallbackContext context)
    {
        if (context.started) goRight = true;
        else if (context.canceled) goRight = false;
    }
    public void Teleport(InputAction.CallbackContext context)
    {
        if (context.started) shouldTeleport = true;
        else if (context.canceled) shouldTeleport = false;
    }
    #endregion
    #region Camera
    public void Look(InputAction.CallbackContext context)
    {
        if (context.started) lookInput = context.ReadValue<Vector2>();
    }
    #endregion
    #region UI
    public void Menu(InputAction.CallbackContext context)
    {
        if (context.started) shouldBeInMenu = !shouldBeInMenu;
    }
    #endregion
}