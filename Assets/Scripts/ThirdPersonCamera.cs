using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Splines.Interpolators;
public class ThirdPersonCamera : MonoBehaviour
{
    public Camera camera;
    public CinemachineFollow cFollow;
    public Transform player;

    private bool inCombat;
    [Min(0.01f)]
    public float lerpSmoothness;
    [Min(0.01f)]
    public float lerpDistance;

    Vector3 defaultOffset;
    Vector3 targetOffset;
    void Start()
    {
        defaultOffset = cFollow.FollowOffset;

    }
    void Update()
    {
        targetOffset = defaultOffset;
        if (inCombat)
        {
            Debug.Log("combat");
            Vector2 mousePos = Input.mousePosition;
            float normalizedX = (mousePos.x / Screen.width) * 2 - 1; // -1 to 1 range
            float normalizedY = (mousePos.y / Screen.height) * 2 - 1; // -1 to 1 range

            Vector3 mouseOffset = new Vector3(normalizedX, 0f, normalizedY);

            targetOffset = defaultOffset + mouseOffset * lerpDistance;
        }

        // Calculate distance between current and target offsets
        float distance = Vector3.Distance(cFollow.FollowOffset, targetOffset);

        // Scale lerp speed based on distance (ensuring a minimum speed)
        float dynamicLerpSpeed = Mathf.Max(lerpSmoothness, Mathf.Sqrt(distance * lerpSmoothness));

        cFollow.FollowOffset = Vector3.Lerp(cFollow.FollowOffset, targetOffset, dynamicLerpSpeed * Time.deltaTime);

    }
    public void Combat(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        inCombat = !inCombat;
    }
}
