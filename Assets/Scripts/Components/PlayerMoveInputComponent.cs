// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UIElements;


// [RequireComponent(typeof(MoveComponent))]
// public class PlayerMoveInputComponent : MonoBehaviour
// {
//     private MoveComponent moveComponent;
//     private Vector2 input;

//     void Awake()
//     {
//         moveComponent = GetComponent<MoveComponent>();
//     }
//     void Update()
//     {
//         moveComponent.Apply(input);
//     }

//     public void Move(InputAction.CallbackContext context)
//     {
//         input = context.ReadValue<Vector2>();
//     }

//     public void Jump(InputAction.CallbackContext context)
//     {
//         if (!context.started)
//         {
//             return;
//         }
//         moveComponent.Jump();
//     }
// }
