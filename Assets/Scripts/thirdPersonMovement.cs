using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class thirdPersonMovement : MonoBehaviour
{
    #region Movement Variables
    private Vector2 input;
    public CharacterController controller;
    private Vector3 moveDirection;
    public float speed = 6f;
    public InputActionReference move;
    #endregion

    #region Rotation Variables
    public float rotationSpeed = 1f;
    #endregion
    #region Gravity Variables
    private float gravity = -9.81f;
    public float gravityMultiplier = 1f;
    private float downwardVelocity;
    #endregion

    #region Jump Variables
    [SerializeField] private float jumpPower;
    #endregion

    // Update is called once per frame
    void Update()
    {

        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
    }

    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        moveDirection = new Vector3(input.x, 0f, input.y);
    }

    void ApplyRotation()
    {
        if (input.sqrMagnitude == 0)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }

    void ApplyMovement()
    {
        controller.Move(speed * Time.deltaTime * moveDirection);
    }

    void ApplyGravity()
    {
        if (IsGrounded() && downwardVelocity < 0.0f)
        {
            downwardVelocity = -1f;
        }
        else
        {
            downwardVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        moveDirection.y = downwardVelocity;
    }

    public void Jump(InputAction.CallbackContext context)
    {

        if (!context.started)
        {
            return;
        }
        if (!IsGrounded())
        {
            return;
        }
        downwardVelocity += jumpPower;
    }

    private bool IsGrounded() => controller.isGrounded;
}
