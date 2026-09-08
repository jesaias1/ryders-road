using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Avoidance.EditorTools
{
    // Explicit content authoring. Recoverable 0.12.0 assets remain in Git.
    public static class GameplayQualityAuthoring
    {
        public const string Root = "Assets/_Game/Art/Environment/Quality130";
        public static void Apply()
        {
            Directory.CreateDirectory(Root+"/Meshes");
            var windward=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            var env=windward.EnvironmentProfile;
            WindwardAuthoring.Build();
            Set(windward,"_environmentProfile",env);
            // Bake the prior Foundry place with a longer service route. Scale all
            // authored positions, including mechanisms and world, through one map.
            var foundry=Resources.Load<ModuleDefinition>("Modules/Module_004_SolarFoundry");
            var foundryEnv=foundry.EnvironmentProfile;
            string savedEnvironment=EditorJsonUtility.ToJson(foundryEnv);
            SolarFoundryAuthoring.Build();
            EditorJsonUtility.FromJsonOverwrite(savedEnvironment,foundryEnv);
            EditorUtility.SetDirty(foundryEnv);
            Set(foundry,"_environmentProfile",foundryEnv);
            var so=new SerializedObject(foundry);
            foreach(string field in new[]{"_blocks","_movingBlocks","_crumblingBlocks","_restorePoints"})
            {
                var array=so.FindProperty(field);
                for(int i=0;i<array.arraySize;i++)
                {
                    var e=array.GetArrayElementAtIndex(i);
                    ScaleZ(e.FindPropertyRelative("_pose").FindPropertyRelative("_position"));
                    var path=e.FindPropertyRelative("_pathPoints");
                    if(path!=null) for(int j=0;j<path.arraySize;j++)ScaleZ(path.GetArrayElementAtIndex(j));
                    var id=e.FindPropertyRelative("_stableId").stringValue;
                    var size=e.FindPropertyRelative("_size");
                    if(size!=null && id=="m04.cooling.safe")size.vector3Value=new Vector3(6,.6f,5.4f);
                    if(size!=null && id=="m04.intake.start")size.vector3Value=new Vector3(7,.6f,4);
                    if(size!=null && id.StartsWith("m04.intake.pipe"))size.vector3Value=new Vector3(4,.6f,3);
                    if(size!=null && id.StartsWith("m04.crown.beam"))size.vector3Value=new Vector3(5,.6f,3);
                    if(size!=null && id.StartsWith("m04.mastery"))size.vector3Value=new Vector3(3,.6f,3);
                }
            }
            ScaleZ(so.FindProperty("_patchBlock").FindPropertyRelative("_pose").FindPropertyRelative("_position"));
            var cuts=so.FindProperty("_optionalShortcuts");
            for(int i=0;i<cuts.arraySize;i++)
            {
                var e=cuts.GetArrayElementAtIndex(i);
                ScaleZ(e.FindPropertyRelative("_entryPosition"));ScaleZ(e.FindPropertyRelative("_exitPosition"));
            }
            so.FindProperty("_contentVersion").intValue=3;
            so.FindProperty("_expectedCleanTime").floatValue=26;
            so.FindProperty("_estimatedCasualTime").floatValue=70;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(foundry);
            foundry.ConfigureRankThresholds(new ModuleRankThresholds(38,26,20,
                Avoidance.Gameplay.Ranking.RankCalibrationState.Uncalibrated,2,
                "0.13.0 candidate: continuous scripted fast line ~17s; old S23 PB 28.421s is a different layout. Human calibration pending; Bronze unlimited."));
            var biome=new SerializedObject(foundry.EnvironmentBiomeProfile);
            var worlds=biome.FindProperty("_worldObjects");
            for(int i=0;i<worlds.arraySize;i++)
            {
                var e=worlds.GetArrayElementAtIndex(i);ScaleZ(e.FindPropertyRelative("_position"));
                if(e.FindPropertyRelative("_hasPlayableArchitecture").boolValue)
                    e.FindPropertyRelative("_prefab").objectReferenceValue=RemapWorld((GameObject)e.FindPropertyRelative("_prefab").objectReferenceValue);
            }
            biome.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(foundry.EnvironmentBiomeProfile);
            var flow=Resources.Load<FlowChallengeProfile>("FlowChallenges/Module004_FlowChallenge");
            for(int i=0;i<flow.Shards.Length;i++){var shard=flow.Shards[i];var p=shard.Position;p.z=RouteZ(p.z);shard.Position=p;flow.Shards[i]=shard;}
            EditorUtility.SetDirty(flow);
            var ivory=Material("Ivory",new Color(.88f,.92f,.94f),.25f);
            var ink=Material("Ink",new Color(.075f,.16f,.21f),.22f);
            var brass=Material("Brass",new Color(.64f,.40f,.13f),.26f);
            var cyan=Material("Cyan",new Color(.12f,.63f,.69f),.22f);
            var deck=Deck("GalleryDeck",ivory,ink,brass,false);
            var cut=Deck("FastLineDeck",ivory,ink,cyan,false);

            var library=Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName);
            ReplaceRole(library,ModuleMaterialRole.Restore,RestoreMarker(cyan));
            // Shared fixes: broad safe courts become one coherent slab instead of a
            // field of tiled pads. The Boost uses a readable shallow launch skin.

            var libSo=new SerializedObject(library);var roles=libSo.FindProperty("_rolePrefabs");
            for(int i=0;i<roles.arraySize;i++)
            {
                var e=roles.GetArrayElementAtIndex(i);
                if(e.FindPropertyRelative("_role").intValue==(int)ModuleMaterialRole.Boost)
                    ConfigureEntry(e,(GameObject)e.FindPropertyRelative("_prefab").objectReferenceValue);
                if(e.FindPropertyRelative("_fitMode").intValue==(int)VisualFitMode.ModularTile)
                    ConfigureEntry(e,deck);
            }
            libSo.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(library);
            var local=AssetDatabase.LoadAssetAtPath<ModuleVisualPrefabLibrary>(Root+"/GalleryLibrary.asset");
            if(local==null){local=Object.Instantiate(library);AssetDatabase.CreateAsset(local,Root+"/GalleryLibrary.asset");}
            else EditorUtility.CopySerialized(library,local);
            ReplaceRole(local,ModuleMaterialRole.Normal,deck);
            ReplaceRole(local,ModuleMaterialRole.Precision,cut);
            ReplaceRole(local,ModuleMaterialRole.Moving,cut);
            foreach(var module in new[]{windward,foundry})
            {
                string path=Root+"/Visuals_"+module.ContentVersion+"_"+module.StableModuleId+".asset";
                var profile=AssetDatabase.LoadAssetAtPath<ModuleVisualProfile>(path);
                if(profile==null){profile=Object.Instantiate(module.VisualProfile);AssetDatabase.CreateAsset(profile,path);}
                Set(profile,"_prefabLibrary",local);Set(module,"_visualProfile",profile);
            }
            foreach(var module in new[]{windward,foundry,Resources.Load<ModuleDefinition>("Modules/Module_003_FlowError")})
            {
                var sky=module.EnvironmentProfile.SkyboxMaterial;
                sky.SetFloat("_CloudOpacity",.82f);sky.SetFloat("_CloudCeiling",.86f);sky.SetFloat("_CloudAmount",.78f);
                EditorUtility.SetDirty(sky);
            }
            FoundationProjectSetup.Apply();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        private static float RouteZ(float z) => z <= 29 ? z * 1.22f : z + 6.38f;
        private static GameObject RemapWorld(GameObject source)
        {
            var root=Object.Instantiate(source);root.name=source.name+"130";
            foreach(var f in root.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=Object.Instantiate(f.sharedMesh);mesh.name=root.name+"_"+f.name;
                var vertices=mesh.vertices;
                for(int i=0;i<vertices.Length;i++){var p=vertices[i];p.z=RouteZ(p.z);vertices[i]=p;}
                mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                string path=Root+"/Meshes/"+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}
                f.sharedMesh=mesh;f.GetComponent<MeshCollider>().sharedMesh=mesh;
                f.GetComponent<Avoidance.Gameplay.Worlds.AuthoredSurface>().SetSourceMesh(mesh);
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+root.name+".prefab");Object.DestroyImmediate(root);return prefab;
        }
        private static void ScaleZ(SerializedProperty p){if(p==null)return;var v=p.vector3Value;v.z=RouteZ(v.z);p.vector3Value=v;}
        private static void Set(Object o,string field,Object value){var so=new SerializedObject(o);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
        private static Material Material(string name,Color c,float smooth)
        {
            var path=Root+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);EditorUtility.SetDirty(m);return m;
        }
        private static void ReplaceRole(ModuleVisualPrefabLibrary library,ModuleMaterialRole role,GameObject prefab)
        {
            var so=new SerializedObject(library);var array=so.FindProperty("_rolePrefabs");
            for(int i=0;i<array.arraySize;i++)
                if(array.GetArrayElementAtIndex(i).FindPropertyRelative("_role").intValue==(int)role)
                    ConfigureEntry(array.GetArrayElementAtIndex(i),prefab);
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(library);
        }
        private static void ConfigureEntry(SerializedProperty e,GameObject prefab)
        {
            e.FindPropertyRelative("_prefab").objectReferenceValue=prefab;
            e.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;
            e.FindPropertyRelative("_localScale").vector3Value=Vector3.one;
            e.FindPropertyRelative("_fitMode").intValue=(int)VisualFitMode.ExactFootprint;
        }
        private static GameObject RestoreMarker(Material material)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            void Rect(float x,float z,float w,float d)
            {
                int n=vertices.Count;vertices.AddRange(new[]{new Vector3(x,0,z),new Vector3(x,0,z+d),new Vector3(x+w,0,z+d),new Vector3(x+w,0,z)});
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            foreach(float x in new[]{-.46f,.28f})foreach(float z in new[]{-.46f,.28f})
            {Rect(x,z<0?-.46f:.448f,.18f,.012f);Rect(x<0?-.46f:.448f,z,.012f,.18f);}
            var mesh=new Mesh{name="RestoreCorners"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var path=Root+"/Meshes/RestoreCorners.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}
            var go=new GameObject("RestoreSurfaceSignal",typeof(MeshFilter),typeof(MeshRenderer),typeof(RestoreSurfaceMarker));
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/RestoreSurfaceSignal.prefab");Object.DestroyImmediate(go);return prefab;
        }
        private static GameObject Deck(string name,Material top,Material side,Material accent,bool launch)
        {
            var go=new GameObject(name);
            // Eight-sided rim, broad clean deck, dark tapered belly. Entire mesh is
            // the skin of the collider, not a second plate on an existing model.
            var verts=new List<Vector3>();var indices=new List<int>();
            var outline=new[]{new Vector2(-.44f,-.5f),new Vector2(.44f,-.5f),new Vector2(.5f,-.44f),new Vector2(.5f,.44f),new Vector2(.44f,.5f),new Vector2(-.44f,.5f),new Vector2(-.5f,.44f),new Vector2(-.5f,-.44f)};
            void Tri(Vector3 a,Vector3 b,Vector3 c){int n=verts.Count;verts.AddRange(new[]{a,b,c});indices.AddRange(new[]{n,n+1,n+2});}
            void SaveMesh(string part,Material material)
            {
                var mesh=new Mesh{name=name+part};mesh.SetVertices(verts);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                string path=Root+"/Meshes/"+mesh.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}
                var child=new GameObject(part,typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(go.transform,false);
                child.GetComponent<MeshFilter>().sharedMesh=mesh;child.GetComponent<MeshRenderer>().sharedMaterial=material;verts.Clear();indices.Clear();
            }
            foreach(float x in new[]{-.237f,.237f})
            foreach(float z in new[]{-.237f,.237f})
            {
                var panel=outline.Select(v=>new Vector3(x+v.x*.46f,.5f,z+v.y*.46f)).ToArray();
                var center=new Vector3(x,.5f,z);
                for(int i=0;i<8;i++)
                {
                    var at=panel[i];var bt=panel[(i+1)%8];Tri(center,bt,at);
                    var ab=center+(at-center)*1.03f;ab.y=.41f;
                    var bb=center+(bt-center)*1.03f;bb.y=.41f;
                    Tri(at,bt,ab);Tri(ab,bt,bb);
                }
            }
            for(int i=0;i<8;i++)
            {
                var a=outline[i];var b=outline[(i+1)%8];
                var at=new Vector3(a.x,.40f,a.y);var bt=new Vector3(b.x,.40f,b.y);
                var ab=new Vector3(a.x,.2f,a.y);var bb=new Vector3(b.x,.2f,b.y);
                Tri(new Vector3(0,.40f,0),bt,at);Tri(at,bt,ab);Tri(ab,bt,bb);
            }
            SaveMesh("Porcelain",top);
            for(int i=0;i<8;i++)
            {
                var a=outline[i];var b=outline[(i+1)%8];
                var at=new Vector3(a.x,.2f,a.y);var bt=new Vector3(b.x,.2f,b.y);
                var ab=new Vector3(a.x*.85f,-.5f,a.y*.85f);var bb=new Vector3(b.x*.85f,-.5f,b.y*.85f);
                Tri(at,bt,ab);Tri(ab,bt,bb);Tri(Vector3.down*.5f,ab,bb);
            }
            SaveMesh("Structure",side);
            // Painted marks remain almost flush; no vertical energy bars.
            if(launch)
            {
                foreach(float z in new[]{-.22f,0,.22f})
                {
                    Tri(new Vector3(-.23f,.501f,z-.06f),new Vector3(0,.501f,z+.13f),new Vector3(0,.501f,z+.02f));
                    Tri(new Vector3(0,.501f,z+.02f),new Vector3(0,.501f,z+.13f),new Vector3(.23f,.501f,z-.06f));
                }
            }
            else
            {
                foreach(float x in new[]{-.41f,.41f})
                {
                    Tri(new Vector3(x-.006f,.501f,-.34f),new Vector3(x+.006f,.501f,.34f),new Vector3(x+.006f,.501f,-.34f));
                    Tri(new Vector3(x-.006f,.501f,-.34f),new Vector3(x-.006f,.501f,.34f),new Vector3(x+.006f,.501f,.34f));
                }
            }
            SaveMesh("Inlay",accent);
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"/"+name+".prefab");Object.DestroyImmediate(go);return prefab;
        }
    }
}
