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
            P("arrival",-24,0,12,8,14);
            P("approach.a",-24,.2f,23.5f,5,3);
            P("approach.b",-22,.4f,30,7,4);
            P("approach.c",-24,.6f,37,5,4);
            P("west.restore",-24,1,47,10,10);
            P("arc.01",-21,1.2f,58,7,6);P("arc.02",-16,1.4f,67,8,6);
            P("arc.03",-6,1.6f,71,8,7);P("arc.04",2.5f,1.6f,72,3,5);
            P("arc.05",9.5f,1.6f,70,5,6);P("arc.06",19,2,68,8,8);
            P("arc.07",28.5f,2.2f,61,7,7);P("arc.08",32,2.4f,51,7,7);
            P("east.restore",34,2.8f,39,11,11);
            P("ascent.a",45,3.1f,31,7,7);P("ascent.b",50,3.3f,23,5,4);
            P("ascent.c",56,3.6f,16,8,6);
            P("lens.restore",68,4,9,12,10);
            P("lens.a",72,4.2f,21,7,8);P("lens.b",76,4.4f,30,5,4);
            P("lens.c",75,4.6f,37.5f,8,6);P("patch.base",70,5,49.5f,12,12);
            var so=new SerializedObject(module);
            var main=so.FindProperty("_blocks");
            for(int i=main.arraySize-1;i>=0;i--)
                if(main.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue=="m05.ascent.d" || main.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue.StartsWith("m05.skill."))main.DeleteArrayElementAtIndex(i);
            plan.Remove("m05.ascent.d");
            foreach(string field in new[]{"_blocks","_crumblingBlocks"})
            {
                var array=so.FindProperty(field);
                for(int i=0;i<array.arraySize;i++)
                {
                    var e=array.GetArrayElementAtIndex(i);var item=plan[e.FindPropertyRelative("_stableId").stringValue];
                    e.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=item.p;
                    e.FindPropertyRelative("_size").vector3Value=item.size;
                    if(field=="_crumblingBlocks")e.FindPropertyRelative("_fallDelay").floatValue=1.8f;
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
            cut.FindPropertyRelative("_displayName").stringValue="Pressure fin carry";
            cut.FindPropertyRelative("_entryPosition").vector3Value=plan["m05.arc.03"].p;
            cut.FindPropertyRelative("_exitPosition").vector3Value=plan["m05.arc.05"].p;
            so.FindProperty("_contentVersion").intValue=5;
            so.FindProperty("_developerNotes").stringValue="0.16.1 cadence: one shared sweep, committed gaps, sparse anchors, optional same-route skips and two pressure fins. Movement unchanged; ranks provisional.";
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module);

            Directory.CreateDirectory(Root+"/Meshes");
            var mats=new Dictionary<string,Material>();
            foreach(string name in new[]{"Ink","Brass"})mats[name]=AssetDatabase.LoadAssetAtPath<Material>(WindwardAuthoring.Root+"/Materials/"+name+".mat");
            var bake=new WorldMeshBake(Root,mats);
            foreach(var entry in plan)
            {
                var item=entry.Value;
                // Only anchors have deep observatory foundations. Intermediate decks float
                // on compact tapered housings; no repeated scaffold forest.
                bool anchor=entry.Key.Contains("restore")||entry.Key=="m05.arrival"||entry.Key=="m05.patch.base";
                if(anchor)
                {
                    bake.Cylinder("Ink",item.p+Vector3.down*3.1f,1.2f,3.1f,4.8f);
                    bake.Cylinder("Brass",item.p+Vector3.down*.9f,3.15f,3.15f,.18f);
                }
                else
                    bake.Cylinder("Ink",item.p+Vector3.down*.85f,.6f,1.35f,.9f);
            }
            var support=bake.Save("RouteKeels",true);
            var biome=new SerializedObject(module.EnvironmentBiomeProfile);var worlds=biome.FindProperty("_worldObjects");
            for(int i=0;i<worlds.arraySize;i++)
            {
                var e=worlds.GetArrayElementAtIndex(i);string id=e.FindPropertyRelative("_stableId").stringValue;
                if(id=="biome.windward.route-structure")e.FindPropertyRelative("_prefab").objectReferenceValue=support;
                // Telescope follows the finish; instrument moves below and away from chord.
                if(id=="biome.windward.telescope")e.FindPropertyRelative("_position").vector3Value=new Vector3(15,-9,10);
                if(id=="biome.windward.wind-instrument")e.FindPropertyRelative("_position").vector3Value=new Vector3(6,-7,4);
            }
            biome.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module.EnvironmentBiomeProfile);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
    }
}
