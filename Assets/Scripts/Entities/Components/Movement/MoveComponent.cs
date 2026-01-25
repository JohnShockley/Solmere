using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EntityComponent))]
public class MoveComponent : MonoBehaviour
{
    [Header("Movement Settings")]
    private float _moveSpeed = 0f;
    private readonly StatType movementSpeed = StatType.MovementSpeed;
    private float _rotationSpeed = 0f;
    private readonly StatType rotationSpeed = StatType.RotationSpeed;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 moveDirection;

    private float verticalVelocity;
    private EntityComponent entity;


    private void Awake()
    {
        entity = GetComponent<EntityComponent>();
        controller = GetComponent<CharacterController>();
    }
    private void Start()
    {
        _moveSpeed = entity.statManager.GetValue(movementSpeed);
        entity.statManager.SubscribeTrue(movementSpeed, (value) =>
        {
            _moveSpeed = value;
        });
        _rotationSpeed = entity.statManager.GetValue(rotationSpeed);
        entity.statManager.SubscribeTrue(rotationSpeed, (value) =>
        {
            _rotationSpeed = value;
        });

    }

    private void Update()
    {
        ApplyGravity();
        ApplyMovement();
    }
    public void SetMoveDirection(Vector3 worldDir)
    {
        moveDirection = worldDir;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        Vector3 horizontalVelocity = moveDirection * _moveSpeed;
        horizontalVelocity.y = verticalVelocity;

        controller.Move(horizontalVelocity * Time.deltaTime);

        // Rotate toward movement direction
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }
    }
}
