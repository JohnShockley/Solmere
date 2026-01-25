// using UnityEngine;

// public class MissileEffect : MonoBehaviour
// {
//     private GameObject target;
//     private Vector3 destination;
//     private bool trackingTarget;
//     private Effect onImpact;
//     private EffectContext context;
//     private float speed = 10f; // default missile speed, could expose as public

//     public void Initialize(GameObject target, Vector3 destination, bool trackingTarget, Effect onImpact, EffectContext context)
//     {
//         this.target = target;
//         this.destination = destination;
//         this.trackingTarget = trackingTarget;
//         this.onImpact = onImpact;
//         this.context = context;
//     }

// void Update()
// {
//     Vector3 currentDestination = trackingTarget && target != null ? target.transform.position : destination;

//     Vector3 direction = (currentDestination - transform.position);
//     if (direction != Vector3.zero)
//     {
//         transform.rotation = Quaternion.LookRotation(direction.normalized); // 🧠 face the movement direction
//     }

//     transform.position += direction.normalized * speed * Time.deltaTime;

//     if (Vector3.Distance(transform.position, currentDestination) < 0.2f)
//     {
//         Impact();
//     }
// }


//     private void Impact()
//     {
//         if (onImpact != null)
//         {
//             onImpact.Apply(context);
//         }
//         Destroy(gameObject);
//     }
// }
