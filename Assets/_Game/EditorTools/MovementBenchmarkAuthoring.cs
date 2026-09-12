using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    // Explicit, idempotent asset authoring; never called by gameplay startup.
    public static class MovementBenchmarkAuthoring
    {
        public const string Root="Assets/_Game/Art/Environment/Benchmark160";
        public static void Apply()
        {
            var module=Resources.Load<ModuleDefinition>("Modules/Module_005_Windward");
            var plan=new Dictionary<string,(Vector3 p,Vector3 size)>();
            void P(string id,float x,float y,float z,float width,float length)
                =>plan["m05."+id]=(new Vector3(x,y,z),new Vector3(width,.6f,length));
            P("arrival",-24,0,12,10,14);
            P("approach.a",-24,.3f,23,7,7);P("approach.b",-22,.6f,32,9,10);
            P("approach.c",-24,1,41,9,7);P("west.restore",-24,1.6f,50,12,12);
            P("arc.01",-22,1.8f,62,9,10);P("arc.02",-16,2,72,10,10);
            P("arc.03",-5,2.2f,77,11,9);P("arc.04",7,2.2f,77,10,8);
            P("arc.05",19,2.2f,76,10,8);P("arc.06",30,2.6f,72,11,10);
            P("arc.07",39,3,63,10,11);P("arc.08",42,3.6f,51,10,11);
            P("east.restore",42,4.6f,39,14,12);
            P("ascent.a",51,5,29,10,10);P("ascent.b",60,5.4f,20,11,10);
            P("ascent.c",69,5.4f,12,12,11);P("ascent.d",79,6.2f,5,11,10);
            P("lens.restore",90,6.6f,4,28,14); // One recovery terrace absorbs the redundant last ascent pad.
            P("lens.a",97,7,16,12,11);P("lens.b",101,7.4f,26,12,12);
            P("lens.c",99,7.8f,36,12,12);P("patch.base",94,8.2f,50,16,14);
            P("skill.chord.1",-8,2.3f,47,14,9);P("skill.chord.2",9,3.1f,44,14,9);
            P("skill.chord.3",26,3.8f,41,14,9);
            var so=new SerializedObject(module);
            var main=so.FindProperty("_blocks");
            for(int i=main.arraySize-1;i>=0;i--)
                if(main.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue=="m05.ascent.d")main.DeleteArrayElementAtIndex(i);
            plan.Remove("m05.ascent.d");
            foreach(string field in new[]{"_blocks","_crumblingBlocks"})
            {
                var array=so.FindProperty(field);
                for(int i=0;i<array.arraySize;i++)
                {
                    var e=array.GetArrayElementAtIndex(i);var item=plan[e.FindPropertyRelative("_stableId").stringValue];
                    e.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=item.p;
                    e.FindPropertyRelative("_size").vector3Value=item.size;
                    if(field=="_crumblingBlocks")e.FindPropertyRelative("_fallDelay").floatValue=3;
                }
            }
            var restores=so.FindProperty("_restorePoints");
            for(int i=0;i<restores.arraySize;i++)
            {
                var e=restores.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=plan[e.FindPropertyRelative("_supportBlockStableId").stringValue].p;
            }
            so.FindProperty("_startPoint").FindPropertyRelative("_position").vector3Value=new Vector3(-24,.35f,8);
            so.FindProperty("_patchBlock").FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=plan["m05.patch.base"].p+Vector3.up*.3f;
            var cut=so.FindProperty("_optionalShortcuts").GetArrayElementAtIndex(0);
            cut.FindPropertyRelative("_entryPosition").vector3Value=plan["m05.skill.chord.1"].p;
            cut.FindPropertyRelative("_exitPosition").vector3Value=plan["m05.east.restore"].p;
            so.FindProperty("_contentVersion").intValue=4;
            so.FindProperty("_developerNotes").stringValue="0.16.0 movement benchmark: long arrival runway, offset safe stepping courts, broad north sweep, direct chord, diagonal ascent and recovery terraces. Same shared physics; rank thresholds unchanged and provisional.";
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module);

            Directory.CreateDirectory(Root+"/Meshes");
            var mats=new Dictionary<string,Material>();
            foreach(string name in new[]{"Ink","Brass"})mats[name]=AssetDatabase.LoadAssetAtPath<Material>(WindwardAuthoring.Root+"/Materials/"+name+".mat");
            var bake=new WorldMeshBake(Root,mats);
            foreach(var item in plan.Values)
            {
                // Narrow underside keels, not attractive secondary landing slabs.
                bake.Beam("Ink",item.p+Vector3.down*.8f,item.p+Vector3.down*5,new Vector2(.7f,.7f));
                bake.Beam("Brass",item.p+new Vector3(-item.size.x*.3f,-.75f,0),item.p+Vector3.down*4,new Vector2(.16f,.16f));
                bake.Beam("Brass",item.p+new Vector3(item.size.x*.3f,-.75f,0),item.p+Vector3.down*4,new Vector2(.16f,.16f));
            }
            var support=bake.Save("RouteKeels",true);
            var biome=new SerializedObject(module.EnvironmentBiomeProfile);var worlds=biome.FindProperty("_worldObjects");
            for(int i=0;i<worlds.arraySize;i++)
            {
                var e=worlds.GetArrayElementAtIndex(i);string id=e.FindPropertyRelative("_stableId").stringValue;
                if(id=="biome.windward.route-structure")e.FindPropertyRelative("_prefab").objectReferenceValue=support;
                // Telescope follows the finish; instrument moves below and away from chord.
                if(id=="biome.windward.telescope")e.FindPropertyRelative("_position").vector3Value=new Vector3(39,-6,10);
                if(id=="biome.windward.wind-instrument")e.FindPropertyRelative("_position").vector3Value=new Vector3(6,-7,4);
            }
            biome.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module.EnvironmentBiomeProfile);
            // The old garden arch crosses the existing valid jump's head clearance.
            // Relocate the scenery as a side landmark; do not make its roof traversable.
            var mountain=Resources.Load<ModuleDefinition>("Modules/Module_002_MovingParts").EnvironmentBiomeProfile;
            var mountainSo=new SerializedObject(mountain);var places=mountainSo.FindProperty("_worldObjects");
            for(int i=0;i<places.arraySize;i++)
            {
                var e=places.GetArrayElementAtIndex(i);
                if(e.FindPropertyRelative("_stableId").stringValue=="biome.mountain-sky.garden.waterfall-shrine")
                    e.FindPropertyRelative("_position").vector3Value=new Vector3(18,3,26);
            }
            mountainSo.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(mountain);
            // Fatal collision changes record compatibility even where route positions stay fixed.
            foreach(var m in Resources.LoadAll<ModuleDefinition>("Modules").Where(m=>m.EnvironmentBiomeProfile!=null && m!=module))
            {
                int version=m.StableModuleId=="module.001.first-steps"?3:m.StableModuleId=="module.002.moving-parts"?4:
                    m.StableModuleId=="module.003.flow-error"?8:m.StableModuleId=="module.004.solar-foundry"?4:m.ContentVersion;
                var ms=new SerializedObject(m);ms.FindProperty("_contentVersion").intValue=version;ms.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(m);
            }
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
    }
}
