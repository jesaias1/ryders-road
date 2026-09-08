using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public static class Production110Authoring
    {
        [MenuItem("RYDERS BLOCK/Production/Apply 0.11 World Atmospheres")]
        public static void Apply()
        {
            WindwardAuthoring.Build();
            string[] names={"SkyCity","Mountain","AncientAbyss","SolarFoundry","Windward"};
            Color C(float r,float g,float b)=>new Color(r,g,b);
            Color[] zenith={C(.065f,.29f,.60f),C(.16f,.30f,.47f),C(.10f,.15f,.32f),C(.10f,.24f,.40f),C(.16f,.22f,.40f)};
            Color[] horizon={C(.62f,.80f,.90f),C(.68f,.76f,.81f),C(.53f,.61f,.74f),C(.86f,.64f,.43f),C(.76f,.70f,.76f)};
            Color[] nadir={C(.18f,.37f,.56f),C(.29f,.38f,.45f),C(.12f,.19f,.31f),C(.34f,.29f,.27f),C(.22f,.28f,.43f)};
            Color[] cloud={C(.90f,.93f,.96f),C(.89f,.86f,.80f),C(.63f,.69f,.78f),C(.90f,.77f,.61f),C(.85f,.79f,.84f)};
            Color[] sun={C(1,.94f,.84f),C(1,.83f,.64f),C(1,.85f,.68f),C(1,.76f,.48f),C(1,.86f,.76f)};
            float[] intensity={1.24f,1.06f,1.2f,1.08f,1.1f};
            float[] fog={.0028f,.0042f,.0058f,.0038f,.0035f};
            float[] elevation={48,26,32,19,23};
            var modules=Resources.LoadAll<ModuleDefinition>("Modules");
            var ids=ModuleSelectionState.GetCampaignModuleIds();
            Directory.CreateDirectory("Assets/_Game/Visuals/WorldAtmospheres");
            for(int i=0;i<ids.Length;i++)
            {
                var module=modules.Single(m=>m.StableModuleId==ids[i]);
                var path="Assets/_Game/Visuals/WorldAtmospheres/"+names[i]+".mat";
                var sky=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(sky==null){sky=new Material(Shader.Find("RydersRoad/World Sky"));AssetDatabase.CreateAsset(sky,path);}
                sky.SetColor("_Zenith",zenith[i]);sky.SetColor("_Horizon",horizon[i]);sky.SetColor("_Nadir",nadir[i]);sky.SetColor("_Cloud",cloud[i]);
                sky.SetFloat("_CloudAmount",i==0?.38f:.62f);sky.SetColor("_SunColor",sun[i]);
                var rotation=Quaternion.Euler(elevation[i],-32,0);
                sky.SetVector("_SunDirection",-(rotation*Vector3.forward));EditorUtility.SetDirty(sky);
                var envPath="Assets/_Game/Visuals/Resources/ModuleEnvironment_"+names[i]+".asset";
                var env=AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(envPath);
                if(env==null){env=Object.Instantiate(module.EnvironmentProfile);AssetDatabase.CreateAsset(env,envPath);}
                var so=new SerializedObject(env);
                so.FindProperty("_ambienceClip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/Clips/Wind110.wav");
                so.FindProperty("_ambienceGain").floatValue=i==4?.09f:.045f;
                so.FindProperty("_skyboxMaterial").objectReferenceValue=sky;
                so.FindProperty("_overrideBiomeFog").boolValue=true;
                so.FindProperty("_authoredLighting").boolValue=true;
                so.FindProperty("_sunEuler").vector3Value=new Vector3(elevation[i],-32,0);
                so.FindProperty("_sunIntensity").floatValue=intensity[i];
                so.FindProperty("_sunLight").colorValue=sun[i];
                so.FindProperty("_ambientLight").colorValue=Color.Lerp(horizon[i],Color.white,.12f)*.65f;
                so.FindProperty("_fogColor").colorValue=Color.Lerp(horizon[i],nadir[i],.2f);
                so.FindProperty("_fogDensity").floatValue=fog[i];
                so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(env);
                so=new SerializedObject(module);so.FindProperty("_environmentProfile").objectReferenceValue=env;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module);
            }
            // Broad matte geology: retain authored silhouettes, all route geometry and collision.
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/_Game/Art/Environment/Mountain094"}))
            {
                var material=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.14f);
                EditorUtility.SetDirty(material);
            }
            var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Branding/Resources/Branding/Jesaias_Emblem.png");
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var audio=AssetDatabase.LoadAssetAtPath<Avoidance.Gameplay.Audio.GameplayAudioProfile>("Assets/_Game/Audio/Resources/GameplayAudioProfile.asset");
            var audioSo=new SerializedObject(audio);var entries=audioSo.FindProperty("_entries");
            void Cue(Avoidance.Gameplay.Audio.GameplayAudioCue cue,string clip,float volume)
            {
                int index=-1;for(int j=0;j<entries.arraySize;j++)if(entries.GetArrayElementAtIndex(j).FindPropertyRelative("Cue").intValue==(int)cue)index=j;
                if(index<0){index=entries.arraySize;entries.InsertArrayElementAtIndex(index);}
                var e=entries.GetArrayElementAtIndex(index);e.FindPropertyRelative("Cue").intValue=(int)cue;
                e.FindPropertyRelative("Clip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(clip);e.FindPropertyRelative("Volume").floatValue=volume;
            }
            Cue(Avoidance.Gameplay.Audio.GameplayAudioCue.Footstep,"Assets/_Game/Audio/Clips/Contact097/footstep_concrete_001.ogg",.13f);
            Cue(Avoidance.Gameplay.Audio.GameplayAudioCue.RestorePoint,"Assets/_Game/Audio/Clips/Contact097/click_003.ogg",.18f);
            Cue(Avoidance.Gameplay.Audio.GameplayAudioCue.Restore,"Assets/_Game/Audio/Clips/Contact097/impactSoft_heavy_000.ogg",.16f);
            audioSo.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(audio);
            FoundationProjectSetup.Apply();AssetDatabase.SaveAssets();
        }
    }
}
