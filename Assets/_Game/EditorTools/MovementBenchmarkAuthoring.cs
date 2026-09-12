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
            // Four movement ideas. Width catches lateral error; depth and spacing
            // determine which beats ordinary and carried-speed players use.
            P("arrival",-24,0,12,8,14);
            P("approach.a",-24,0,22.5f,9,2);
            P("approach.b",-24,0,26.5f,10,3);
            P("approach.c",-24,0,31,11,3);
            P("west.restore",-24,.2f,39,14,8);
            // A sustained left sweep, never alternating corrections.
            P("arc.01",-24,.2f,47,10,3);P("arc.02",-25,.2f,52,10,4);
            P("arc.03",-27,.2f,58,11,5);P("arc.04",-30,.2f,64,12,3);
            P("arc.05",-34,.2f,70,12,4);P("arc.06",-38,.2f,76.5f,12,6);
            P("east.restore",-43,.4f,88,16,11);
            // The diagonal continues the preceding heading, with shallow rises.
            P("ascent.a",-47,.6f,97.5f,14,3);P("ascent.b",-50,.8f,102.5f,14,3.5f);
            P("ascent.c",-53,1,108,14,4);
            P("lens.restore",-58,1.2f,117,20,10);
            // Straighten on the recovery terrace, then carry toward the lens.
            P("lens.a",-58,1.2f,126,10,3);P("lens.b",-58,1.2f,131,11,4);
            P("lens.c",-58,1.2f,137,12,4);P("patch.base",-58,1.2f,146,14,10);
            var so=new SerializedObject(module);
            var main=so.FindProperty("_blocks");
            for(int i=main.arraySize-1;i>=0;i--)
                if(!plan.ContainsKey(main.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue))
                    main.DeleteArrayElementAtIndex(i);
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
                string supportId=e.FindPropertyRelative("_supportBlockStableId").stringValue;
                e.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=plan[supportId].p;
                e.FindPropertyRelative("_pose").FindPropertyRelative("_eulerAngles").vector3Value=new Vector3(0,supportId=="m05.east.restore"?-25:0,0);
            }
            so.FindProperty("_startPoint").FindPropertyRelative("_position").vector3Value=new Vector3(-24,.35f,8);
            so.FindProperty("_patchBlock").FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=plan["m05.patch.base"].p+Vector3.up*.3f;
            var cut=so.FindProperty("_optionalShortcuts").GetArrayElementAtIndex(0);
            cut.FindPropertyRelative("_displayName").stringValue="Pressure fin carry";
            cut.FindPropertyRelative("_entryPosition").vector3Value=plan["m05.arc.03"].p;
            cut.FindPropertyRelative("_exitPosition").vector3Value=plan["m05.arc.05"].p;
            so.FindProperty("_contentVersion").intValue=6;
            so.FindProperty("_developerNotes").stringValue="0.16.2 flow sequences: acceleration, sustained sweep, rising diagonal, final carry. Wide lateral catches; held-hop omissions and recovery terraces. Movement unchanged; ranks provisional.";
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

            }
            var support=bake.Save("RouteKeels",true);
            var biome=new SerializedObject(module.EnvironmentBiomeProfile);var worlds=biome.FindProperty("_worldObjects");
            for(int i=0;i<worlds.arraySize;i++)
            {
                var e=worlds.GetArrayElementAtIndex(i);string id=e.FindPropertyRelative("_stableId").stringValue;
                if(id=="biome.windward.route-structure")e.FindPropertyRelative("_prefab").objectReferenceValue=support;
                // Telescope follows the finish; instrument moves below and away from chord.
                if(id=="biome.windward.telescope")e.FindPropertyRelative("_position").vector3Value=new Vector3(-116,-14,112);
                if(id=="biome.windward.west-sail")e.FindPropertyRelative("_position").vector3Value=new Vector3(-75,-20,51);
                if(id=="biome.windward.north-sail")e.FindPropertyRelative("_position").vector3Value=new Vector3(10,-32,103);
                if(id=="biome.windward.east-sail")e.FindPropertyRelative("_position").vector3Value=new Vector3(-96,-24,139);
                if(id=="biome.windward.wind-instrument")e.FindPropertyRelative("_position").vector3Value=new Vector3(-14,-15,20);
            }
            biome.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module.EnvironmentBiomeProfile);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
    }
}
