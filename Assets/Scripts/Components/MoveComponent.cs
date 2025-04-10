using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


[RequireComponent(typeof(PropertyRegistryComponent))]
public class MoveComponent : MonoBehaviour
{
    private Vector3 moveDirection;
    private CharacterController controller;

    public Property speed;
    public Property rotationSpeed;
    public Property gravityMultiplier;
    public Property jumpPower;
    private float gravity = -9.81f;
    private float downwardVelocity;

    private PropertyRegistryComponent propertyRegistry;
    void Awake()
    {
        controller = GetComponent<CharacterController>();

        propertyRegistry = GetComponent<PropertyRegistryComponent>();
        propertyRegistry.Register("speed", speed);
        propertyRegistry.Register("rotationSpeed", rotationSpeed);
        propertyRegistry.Register("gravityMultiplier", gravityMultiplier);
        propertyRegistry.Register("jumpPower", jumpPower);
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
        if (input.sqrMagnitude == 0)
        {
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(input.x, 0f, input.y));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed.Value);
    }

    public void ApplyMovement(Vector2 input)
    {
        controller.Move(speed.Value * Time.deltaTime * moveDirection);
    }

    public void ApplyGravity()
    {
        if (IsGrounded() && downwardVelocity < 0.0f)
        {
            downwardVelocity = -1f;
        }
        else
        {
            downwardVelocity += gravity * gravityMultiplier.Value * Time.deltaTime;
        }
        moveDirection.y = downwardVelocity;
    }

    public void Jump()
    {
        if (!IsGrounded())
        {
            return;
        }
        downwardVelocity += jumpPower.Value;
    }

    private bool IsGrounded() => controller.isGrounded;
}
