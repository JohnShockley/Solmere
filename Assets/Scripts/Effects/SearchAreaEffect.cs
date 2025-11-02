// using UnityEngine;

// [CreateAssetMenu(menuName = "Effects/SearchArea")]
// public class SearchAreaEffect : Effect
// {
//     public float radius;
//     public LayerMask targetMask;
//     public Effect effect;

//     public override void Apply(EffectContext context)
//     {
//         var targets = Physics.OverlapSphere((Vector3)context.Position, radius, targetMask);
//         foreach (var target in targets)
//         {
//             context.Target = target.gameObject;
//             effect.Apply(context);
//         }
//     }


// }
