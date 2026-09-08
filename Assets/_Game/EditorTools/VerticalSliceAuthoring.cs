using System.Linq;
using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Levels;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    // Explicit production pass. Does not run at startup or rewrite existing routes.
    public static class VerticalSliceAuthoring
    {
        public static void Apply()
        {
            var windward=AssetDatabase.LoadAssetAtPath<ModuleDefinition>(WindwardAuthoring.ModulePath);
            var environment=windward.EnvironmentProfile;
            WindwardAuthoring.Build();
            var moduleSo=new SerializedObject(windward);
            moduleSo.FindProperty("_environmentProfile").objectReferenceValue=environment;
            moduleSo.ApplyModifiedPropertiesWithoutUndo();
            MountainWorldProductionAuthoring.RebuildWaterfall();
            foreach(var name in new[]{"Mountain","AncientAbyss","SolarFoundry","Windward"})
            {
                var env=AssetDatabase.LoadAssetAtPath<Object>("Assets/_Game/Visuals/Resources/ModuleEnvironment_"+name+".asset");
                var so=new SerializedObject(env);
                // Surface modelling comes from warm direct light against cooler, quieter fill.
                var sky=so.FindProperty("_skyboxMaterial").objectReferenceValue as Material;
                so.FindProperty("_ambientLight").colorValue=Color.Lerp(sky.GetColor("_Horizon"),Color.white,.12f)*.43f;
                so.FindProperty("_nearArchitectureShadows").boolValue=true;
                so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(env);
            }
            var audio=Resources.Load<GameplayAudioProfile>("GameplayAudioProfile");
            var audioSo=new SerializedObject(audio);var entries=audioSo.FindProperty("_entries");
            void Cue(GameplayAudioCue cue,string file,float gain)
            {
                int index=-1;for(int i=0;i<entries.arraySize;i++)if(entries.GetArrayElementAtIndex(i).FindPropertyRelative("Cue").intValue==(int)cue)index=i;
                if(index<0){index=entries.arraySize;entries.InsertArrayElementAtIndex(index);}
                var e=entries.GetArrayElementAtIndex(index);e.FindPropertyRelative("Cue").intValue=(int)cue;
                e.FindPropertyRelative("Clip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/"+file);
                e.FindPropertyRelative("Volume").floatValue=gain;
            }
            Cue(GameplayAudioCue.Patch,"Patch120.wav",.32f);
            Cue(GameplayAudioCue.Boost,"Boost120.wav",.22f);
            audioSo.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(audio);
            FoundationProjectSetup.Apply();AssetDatabase.SaveAssets();
        }
    }
}
