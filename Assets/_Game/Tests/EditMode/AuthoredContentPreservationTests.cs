using System.IO;
using System.Linq;
using Avoidance.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Avoidance.Tests.EditMode
{
    public sealed class AuthoredContentPreservationTests
    {
        [Test]
        public void StartupPreservesAuthoredAncientAbyssIncludingDesignerEdits()
        {
            const string modulePath = "Assets/_Game/Levels/Resources/Modules/Module_003_FlowError.asset";
            const string biomePath = "Assets/_Game/Worlds/Resources/EnvironmentBiomes/Biome_AncientAbyss.asset";
            var paths = Directory.GetFiles(Phase074WorldSystemRecovery.LandmarkPrefabRoot, "*.prefab")
                .Concat(new[] { modulePath, biomePath }).ToArray();
            var before = paths.ToDictionary(path => path, File.ReadAllBytes);
            var module = AssetDatabase.LoadMainAssetAtPath(modulePath);
            string originalName = module.name;
            try
            {
                // A legitimate serialized authoring edit must survive routine project setup.
                module.name = "Authored preservation sentinel";
                EditorUtility.SetDirty(module);AssetDatabase.SaveAssets();
                var editedModuleBytes = File.ReadAllBytes(modulePath);
                FoundationProjectSetup.Apply();
                Assert.That(File.ReadAllBytes(modulePath), Is.EqualTo(editedModuleBytes));
                foreach(var path in paths.Where(path => path != modulePath))
                    Assert.That(File.ReadAllBytes(path), Is.EqualTo(before[path]), path);
            }
            finally
            {
                module.name = originalName;EditorUtility.SetDirty(module);AssetDatabase.SaveAssets();
                // Restore the fixture's exact pre-test document even if the assertion failed.
                File.WriteAllBytes(modulePath, before[modulePath]);
                AssetDatabase.ImportAsset(modulePath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
