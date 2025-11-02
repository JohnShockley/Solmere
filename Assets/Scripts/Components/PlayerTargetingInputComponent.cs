// using UnityEngine;
// [RequireComponent(typeof(TargetingComponent))]
// public class PlayerTargetingInputComponent : MonoBehaviour
// {
//     private TargetingComponent targetingComponent;
//     private Camera mainCamera;

//     [SerializeField] private GameObject currentTarget;        // Actual selected target
//     private GameObject currentlyHovered;     // Temp hover target

//     void Awake()
//     {
//         targetingComponent = GetComponent<TargetingComponent>();
//         mainCamera = Camera.main;
//     }

//     void Update()
//     {
//         HandleMouseHover();
//         HandleMouseClick();
//     }
//     void HandleMouseClick()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 var targetable = hit.collider.GetComponentInParent<TargetableComponent>();
//                 if (targetable != null)
//                 {
//                     SetTarget(targetable.gameObject);
//                 }
//                 else
//                 {
//                     ClearTarget();
//                 }
//             }
//             else
//             {
//                 ClearTarget();
//             }
//         }
//     }
//     void OutlineTarget(GameObject newTarget)
//     {
//         if (currentlyHovered != null && currentlyHovered != currentTarget)
//         {
//             currentlyHovered.GetComponent<Outline>().enabled = false;
//         }
//         if (newTarget != null && newTarget != currentTarget)
//         {
//             newTarget.GetComponent<Outline>().enabled = true;
//         }
//         currentlyHovered = newTarget;
//     }
//     void HandleMouseHover()
//     {
//         Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
//         if (Physics.Raycast(ray, out RaycastHit hit))
//         {
//             var targetable = hit.collider.GetComponentInParent<TargetableComponent>();
//             if (targetable != null)
//             {
//                 OutlineTarget(targetable.gameObject);
//             }
//             else
//             {
//                 OutlineTarget(null);
//             }
//         }
//         else
//         {
//             OutlineTarget(null);
//         }
//     }

//     void SetTarget(GameObject newTarget)
//     {
//         if (currentTarget != null)
//         {
//             currentTarget.GetComponent<Outline>().enabled = false;
//         }

//         currentTarget = newTarget;
//         targetingComponent.SetTarget(newTarget);

//         newTarget.GetComponent<Outline>().enabled = true;
//     }

//     void ClearTarget()
//     {
//         if (currentTarget != null)
//         {
//             currentTarget.GetComponent<Outline>().enabled = false;
//             currentTarget = null;
//             targetingComponent.ClearTarget();
//         }
//     }
//     public GameObject GetCurrentTarget() => currentTarget;
// }
