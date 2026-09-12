using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Avoidance.EditorTools
{
    public static class WorldKitAuthoring
    {
        public const string Root="Assets/_Game/Art/Environment/World170";
        public static void Apply()
        {
            Directory.CreateDirectory(Root+"/Meshes");
            var materials=new Dictionary<string,Material>();
            void Mat(string name,Color color,float smooth)
            {
                string path=Root+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
                m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",name=="Metal"?.35f:0);
                m.enableInstancing=true;EditorUtility.SetDirty(m);materials[name]=m;
            }
            Mat("Ivory",new Color(.86f,.88f,.85f),.24f);Mat("Structure",new Color(.055f,.12f,.17f),.22f);
            Mat("Cyan",new Color(.08f,.58f,.65f),.3f);Mat("Warm",new Color(.70f,.39f,.13f),.32f);
            Mat("Rock",new Color(.40f,.47f,.47f),.12f);Mat("Stone",new Color(.73f,.75f,.69f),.15f);
            Mat("Metal",new Color(.28f,.36f,.40f),.35f);
            string[] names={"SkyCity","Mountain","AncientAbyss","SolarFoundry","Windward","Spiral"};
            string[] resources={"Module_001_FirstSteps","Module_002_MovingParts","Module_003_FlowError","Module_004_SolarFoundry","Module_005_Windward","Module_004_TheSpiral"};
            for(int world=0;world<names.Length;world++)
            {
                string name=names[world];var bake=new WorldMeshBake(Root,materials);
                // Complete exact-footprint shell; all structural detail remains inside its bounds.
                bake.BeveledBox("Structure",new Vector3(0,-.18f,0),new Vector3(.98f,.64f,.98f),.06f);
                if(world==0||world==4)
                {
                    // Architectural terrace: two long stone panels, recessed navy joint and paired ribs.
                    foreach(float x in new[]{-.251f,.251f})bake.BeveledBox("Ivory",new Vector3(x,.37f,0),new Vector3(.498f,.26f,1),.018f);
                    foreach(float x in new[]{-.40f,.40f})foreach(float z in new[]{-.38f,.38f})
                        bake.Box("Warm",new Vector3(x,.499f,z),new Vector3(.045f,.002f,.09f));
                    foreach(float x in new[]{-.38f,.38f})bake.Box("Warm",new Vector3(x,.06f,0),new Vector3(.025f,.06f,.92f));
                    if(world==4)foreach(float z in new[]{-.3f,.3f})bake.Box("Metal",new Vector3(0,-.34f,z),new Vector3(.84f,.22f,.07f));
                }
                else if(world==1)
                {
                    // Cut cliff shelf above a staggered geological belly.
                    bake.BeveledBox("Stone",new Vector3(0,.34f,0),new Vector3(1,.32f,1),.04f);
                    foreach(float x in new[]{-.38f,.38f})bake.Box("Ivory",new Vector3(x,.499f,0),new Vector3(.025f,.002f,.74f));
                    foreach(float x in new[]{-.3f,0,.3f})bake.BeveledBox("Rock",new Vector3(x,-.1f,.02f),new Vector3(.31f,.65f,.90f-Mathf.Abs(x)*.5f),.06f);
                }
                else if(world==2)
                {
                    // Ruined span: broad rim and three transverse worn masonry courses.
                    for(int j=0;j<3;j++)bake.BeveledBox("Stone",new Vector3(0,.36f,(j-1)*.334f),new Vector3(1,.28f,.332f),.035f);
                    foreach(float x in new[]{-.4f,.4f})bake.Box("Cyan",new Vector3(x,.1f,0),new Vector3(.035f,.06f,.84f));
                }
                else if(world==3)
                {
                    // Gantry plates sit on deep transverse metal stiffeners.
                    foreach(float x in new[]{-.251f,.251f})bake.BeveledBox("Metal",new Vector3(x,.36f,0),new Vector3(.498f,.28f,1),.025f);
                    foreach(float z in new[]{-.36f,0,.36f})bake.Box("Warm",new Vector3(0,.08f,z),new Vector3(.98f,.08f,.055f));
                    foreach(float x in new[]{-.42f,.42f})bake.Box("Ivory",new Vector3(x,.492f,0),new Vector3(.035f,.016f,.8f));
                }
                else
                {
                    bake.BeveledBox("Ivory",new Vector3(0,.35f,0),new Vector3(1,.3f,1),.065f);
                    bake.Box("Cyan",new Vector3(0,.16f,0),new Vector3(.96f,.07f,.96f));
                }
                var deck=bake.Save(name+"Deck",false);
                var module=Resources.Load<ModuleDefinition>("Modules/"+resources[world]);
                if(world<4)BuildFoundations(module,world,materials,name);
                var library=Copy(Resources.Load<ModuleVisualPrefabLibrary>(ModuleVisualPrefabLibrary.ResourceName),Root+"/"+name+"Library.asset");
                var so=new SerializedObject(library);var array=so.FindProperty("_rolePrefabs");
                for(int i=0;i<array.arraySize;i++)
                {
                    var entry=array.GetArrayElementAtIndex(i);
                    if(entry.FindPropertyRelative("_role").intValue!=(int)ModuleMaterialRole.Normal)continue;
                    entry.FindPropertyRelative("_prefab").objectReferenceValue=deck;
                    entry.FindPropertyRelative("_fitMode").intValue=(int)VisualFitMode.ExactFootprint;
                    entry.FindPropertyRelative("_localPosition").vector3Value=Vector3.zero;
                    entry.FindPropertyRelative("_localScale").vector3Value=Vector3.one;
                }
                so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(library);
                var profile=Copy(module.VisualProfile,Root+"/"+name+"Visual.asset");Set(profile,"_prefabLibrary",library);Set(module,"_visualProfile",profile);
                if(world==5)
                {
                    var environment=Copy(Resources.Load<ModuleEnvironmentProfile>("ModuleEnvironment_AncientAbyss"),Root+"/SpiralEnvironment.asset");
                    var env=new SerializedObject(environment);env.FindProperty("_ambientLight").colorValue=new Color(.31f,.31f,.49f);
                    env.FindProperty("_fogColor").colorValue=new Color(.35f,.35f,.57f);env.FindProperty("_sunIntensity").floatValue=1.25f;
                    env.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(environment);Set(module,"_environmentProfile",environment);
                }
            }
            AssetDatabase.SaveAssets();
        }
        private static T Copy<T>(T source,string path) where T:Object
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path);if(asset!=null)return asset;
            asset=Object.Instantiate(source);AssetDatabase.CreateAsset(asset,path);return asset;
        }
        private static void Set(Object target,string property,Object value)
        {var so=new SerializedObject(target);so.FindProperty(property).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);}
        private static void BuildFoundations(ModuleDefinition module,int world,Dictionary<string,Material> materials,string name)
        {
            var ids=module.RestorePoints.Select(r=>r.SupportBlockStableId).Concat(new[]{module.StartSupportBlockStableId,module.PatchBlock.SupportBlockStableId}).Distinct();
            var bake=new WorldMeshBake(Root,materials);
            foreach(string id in ids)
            {
                var deck=module.Blocks.FirstOrDefault(b=>b.StableId==id);if(deck==null)continue;
                var p=deck.Pose.Position;float radius=Mathf.Min(deck.Size.x,deck.Size.z)*.36f;
                if(world==1)
                {
                    bake.Cylinder("Rock",p+Vector3.down*4,1,radius,7.3f);
                    bake.Cylinder("Stone",p+new Vector3(-radius*.55f,-2.2f,0),.6f,radius*.55f,3.7f);
                }
                else if(world==3)
                {
                    bake.BeveledBox("Structure",p+Vector3.down*.7f,new Vector3(deck.Size.x*.8f,.7f,deck.Size.z*.8f),.2f);
                    foreach(float x in new[]{-1f,1f})
                        bake.Beam("Metal",p+new Vector3(x*radius*.35f,-5,0),p+new Vector3(x*radius,-1,0),new Vector2(.5f,.7f));
                    bake.Box("Warm",p+Vector3.down*4.8f,new Vector3(radius,.3f,1));
                }
                else
                {
                    bake.BeveledBox("Structure",p+Vector3.down*1.15f,new Vector3(deck.Size.x*.8f,1.6f,deck.Size.z*.8f),.3f);
                    foreach(float x in new[]{-1f,1f})
                    {
                        bake.Cylinder(world==0?"Ivory":"Stone",p+new Vector3(x*radius*.65f,-4,0),.65f,.85f,4.5f);
                        bake.Cylinder("Warm",p+new Vector3(x*radius*.65f,-2,0),.9f,.9f,.16f);
                    }
                }
            }
            var prefab=bake.Save(name+"Foundations",true);
            var so=new SerializedObject(module.EnvironmentBiomeProfile);var objects=so.FindProperty("_worldObjects");
            string stable="biome.flow-foundations."+module.StableModuleId;int at=-1;
            for(int i=0;i<objects.arraySize;i++)if(objects.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue==stable)at=i;
            if(at<0){at=objects.arraySize;objects.InsertArrayElementAtIndex(at);}
            var item=objects.GetArrayElementAtIndex(at);item.FindPropertyRelative("_stableId").stringValue=stable;
            item.FindPropertyRelative("_prefab").objectReferenceValue=prefab;
            item.FindPropertyRelative("_position").vector3Value=Vector3.zero;item.FindPropertyRelative("_eulerAngles").vector3Value=Vector3.zero;
            item.FindPropertyRelative("_scale").vector3Value=Vector3.one;item.FindPropertyRelative("_hasPlayableArchitecture").boolValue=true;
            item.FindPropertyRelative("_traversableArchitecture").boolValue=false;item.FindPropertyRelative("_staticBatch").boolValue=true;
            item.FindPropertyRelative("_depthBand").intValue=1;item.FindPropertyRelative("_depthFadeStrength").floatValue=.3f;
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module.EnvironmentBiomeProfile);
        }
    }
}
