using Avoidance.Gameplay.Audio;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public static class ContactAudioAuthoring
    {
        [MenuItem("RYDERS BLOCK/Audio/Apply Contact Foley Candidate")]
        public static void Apply()
        {
            AssetDatabase.Refresh();
            var profile = AssetDatabase.LoadAssetAtPath<GameplayAudioProfile>(
                "Assets/_Game/Audio/Resources/GameplayAudioProfile.asset");
            var serialized = new SerializedObject(profile);
            var entries = serialized.FindProperty("_entries");
            Add(GameplayAudioCue.Jump, "footstep_concrete_001", .12f);
            Add(GameplayAudioCue.Landing, "footstep_concrete_000", .35f);
            Add(GameplayAudioCue.HardLanding, "impactSoft_heavy_000", .45f);
            Add(GameplayAudioCue.CrumbleWarning, "impactMining_002", .22f);
            Add(GameplayAudioCue.CrumbleCollapse, "impactMining_000", .36f);
            Add(GameplayAudioCue.Ui, "click_003", .25f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            void Add(GameplayAudioCue cue, string file, float volume)
            {
                string path = "Assets/_Game/Audio/Clips/Contact097/" + file + ".ogg";
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                importer.forceToMono = true;
                var settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                int index = -1;
                for (int i = 0; i < entries.arraySize; i++)
                    if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("Cue").intValue == (int)cue)
                        index = i;
                if (index < 0) { index = entries.arraySize; entries.InsertArrayElementAtIndex(index); }
                var entry = entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("Cue").intValue = (int)cue;
                entry.FindPropertyRelative("Clip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                entry.FindPropertyRelative("Volume").floatValue = volume;
            }
        }
    }
}
