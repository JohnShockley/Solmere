using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class thirdPersonMovement : MonoBehaviour
{

    public CharacterController controller;
    public float speed = 6f;
    public float lookSpeed = 1f;

    public InputActionReference move;

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>().normalized;
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed);
        }
    }
}
