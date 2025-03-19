// using UnityEngine;
// using UnityEditor;
// [CustomEditor(typeof(VoronoiGenerator))]
// public class VoronoiGeneratorEditor :Editor
// {
//     public override void OnInspectorGUI()
//     {
//         Voronoi voronoiGenerator = (VoronoiGenerator)target;

//         if (DrawDefaultInspector())
//         {
//             if (voronoiGenerator.autoUpdate) {
//                  voronoiGenerator.CreateVoronoi();
//             }
//         }
//         if (GUILayout.Button("Generate"))
//         {
//             voronoiGenerator.CreateVoronoi();
//         }
//     }
// }
