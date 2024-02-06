using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] ThirdPersonMovement thirdPersonMovement;

    PlayerControls playerControls;
    PlayerControls.PlayerMovementActions playerMovement;

    Vector2 movementInput;

    private void Awake()
    {
        playerControls = new PlayerControls();
        playerMovement = playerControls.PlayerMovement;

        playerMovement.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerMovement.Jump.performed += ctx => thirdPersonMovement.OnJumpPressed();
        playerMovement.Crouch.performed += ctx => thirdPersonMovement.OnCrouchStart();
        playerMovement.Crouch.canceled += ctx => thirdPersonMovement.OnCrouchEnd();
    }

    private void Update()
    {
        thirdPersonMovement.RecieveInput(movementInput);
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
