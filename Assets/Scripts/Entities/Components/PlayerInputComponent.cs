using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MoveComponent))]
public class PlayerInputComponent : MonoBehaviour
{
    private MoveComponent mover;

    private void Awake()
    {
        mover = GetComponent<MoveComponent>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        Vector3 dir = new Vector3(input.x, 0f, input.y);

        mover.SetMoveDirection(dir.normalized);
        Debug.Log("PlayerInputComponent: OnMove - " + dir);
    }
}
