using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
[RequireComponent(typeof(EntityComponent))]

public class MoveComponent : MonoBehaviour
{
    private Vector3 moveDirection;
    private CharacterController controller;

    public StatType speed;
    public StatType rotationSpeed;
    private float gravity = -9.81f;
    private float gravityMultiplier = 1f;
    private float downwardVelocity;
    private EntityComponent entityComponent;


    void Start()
    {
        entityComponent = GetComponent<EntityComponent>();

        controller = GetComponent<CharacterController>();
    }

    // Call this method each frame to apply movement
    public void Apply(Vector2 input)
    {
        moveDirection = new Vector3(input.x, 0f, input.y);
        ApplyGravity();
        ApplyRotation(input);
        ApplyMovement(input);
    }

    public void ApplyRotation(Vector2 input)
    {
        var rotationSpeed = entityComponent.statManager.GetValue(StatType.RotationSpeed);
        if (input.sqrMagnitude == 0)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }

    public void ApplyMovement(Vector2 input)
    {
         var speed = entityComponent.statManager.GetValue(StatType.MovementSpeed);
        controller.Move(speed * Time.deltaTime * moveDirection);
    }

    public void ApplyGravity()
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

    public void Jump()
    {
        var jumpPower = entityComponent.statManager.GetValue(StatType.JumpPower);
        if (!IsGrounded())
        {
            return;
        }
        downwardVelocity += jumpPower;
    }

    private bool IsGrounded() => controller.isGrounded;
}
