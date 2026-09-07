using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Avoidance.Gameplay.Levels;

namespace Avoidance.EditorTools
{
    public static class ProductionTruthAudit
    {
        public static void Run()
        {
            var report = new StringBuilder("# Module 003 source surface audit\n\n");
            foreach (var module in Resources.LoadAll<ModuleDefinition>("Modules"))
            {
                if (module.StableModuleId != "module.003.flow-error") continue;
                var serialized = new SerializedObject(module);
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Game/Art/Environment" }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.Contains("M03_") && !path.Contains("PF_RR_FloatingArch") && !path.Contains("PF_RR_RuinedTower")) continue;
                    var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                    report.AppendLine("\n## " + path);
                    foreach (var mesh in instance.GetComponentsInChildren<MeshFilter>(true))
                    {
                        var renderer = mesh.GetComponent<Renderer>();
                        report.AppendLine($"{mesh.transform.parent?.name}/{mesh.name}: mesh={AssetDatabase.GetAssetPath(mesh.sharedMesh)} vertices={mesh.sharedMesh.vertexCount} bounds={renderer.bounds} colliders={mesh.GetComponents<Collider>().Length}");
                    }
                    Object.DestroyImmediate(instance);
                }
            }
            Directory.CreateDirectory("Logs/Phase091VisualQA");
            File.WriteAllText("Logs/Phase091VisualQA/source-surface-audit.md", report.ToString());
        }
    }
}
