using System;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    public static class WorldFlowAuthoring
    {
        [Serializable] public sealed class Plan {public World[] worlds;}
        [Serializable] public sealed class World {public string resource,world;public int version;public Entry[] entries;public Scenery[] scenery,distant;public Anchor[] anchors;public string[] retired;public Metadata[] metadata;public Shard[] shards;}
        [Serializable] public sealed class Metadata {public string field,id,property;public Vector3 value;}
        [Serializable] public sealed class Shard {public string resource,id;public Vector3 value;}
        [Serializable] public sealed class Anchor {public string field,id,support;public Vector3 position;}
        [Serializable] public sealed class Scenery {public string id,sourceGuid;public Vector3 position,sourcePosition,sourceRotation,sourceScale;}
        [Serializable] public sealed class Entry {public string id,field;public Vector3 old,position,size,rotation,direction;public Vector3[] path;public float pause,vertical,horizontal,cooldown;}
        public static void Apply()
        {
            var plan=JsonUtility.FromJson<Plan>(File.ReadAllText("Assets/_Game/EditorTools/Data/WorldFlow170.json"));
            foreach(var world in plan.worlds)
            {
                var module=Resources.Load<ModuleDefinition>(world.resource);
                var so=new SerializedObject(module);
                var blocks=so.FindProperty("_blocks");
                if(world.retired!=null)
                    for(int i=blocks.arraySize-1;i>=0;i--)
                        if(world.retired.Contains(blocks.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue))blocks.DeleteArrayElementAtIndex(i);
                foreach(var entry in world.entries)
                {
                    var array=so.FindProperty(entry.field);
                    bool found=false;
                    for(int i=0;i<array.arraySize;i++)if(array.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue==entry.id)found=true;
                    if(!found)
                    {
                        array.InsertArrayElementAtIndex(array.arraySize);
                        var added=array.GetArrayElementAtIndex(array.arraySize-1);
                        added.FindPropertyRelative("_stableId").stringValue=entry.id;
                        var role=added.FindPropertyRelative("_visualRole");if(role!=null)role.intValue=0;
                        added.FindPropertyRelative("_pose").FindPropertyRelative("_eulerAngles").vector3Value=Vector3.zero;
                    }
                    for(int i=0;i<array.arraySize;i++)
                    {
                        var e=array.GetArrayElementAtIndex(i);
                        if(e.FindPropertyRelative("_stableId").stringValue!=entry.id)continue;
                        e.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=entry.position;
                        e.FindPropertyRelative("_size").vector3Value=entry.size;
                        if(entry.vertical>0)
                        {
                            e.FindPropertyRelative("_launchDirection").vector3Value=entry.direction;
                            e.FindPropertyRelative("_verticalStrength").floatValue=entry.vertical;
                            e.FindPropertyRelative("_horizontalStrength").floatValue=entry.horizontal;
                            e.FindPropertyRelative("_cooldown").floatValue=entry.cooldown;
                        }
                        if(entry.field=="_surfSurfaces")
                        {
                            e.FindPropertyRelative("_flowDirection").vector3Value=entry.direction;
                            e.FindPropertyRelative("_pose").FindPropertyRelative("_eulerAngles").vector3Value=entry.rotation;
                        }
                        if(entry.pause>0)e.FindPropertyRelative("_pauseAtEndpoints").floatValue=entry.pause;
                        if(entry.path!=null)
                        {
                            var path=e.FindPropertyRelative("_pathPoints");path.arraySize=entry.path.Length;
                            for(int j=0;j<entry.path.Length;j++)path.GetArrayElementAtIndex(j).vector3Value=entry.path[j];
                        }
                    }
                }
                // Existing anchor identities survive; only their physical support pose changes.
                if(world.world!="Spiral")
                {
                    int order=0;
                    foreach(var entry in world.entries.Where(e=>e.field=="_blocks"&&!e.id.Contains("mastery")&&!e.id.Contains("risky")).OrderBy(e=>e.position.z))
                    {
                        for(int i=order;i<blocks.arraySize;i++)
                            if(blocks.GetArrayElementAtIndex(i).FindPropertyRelative("_stableId").stringValue==entry.id){blocks.MoveArrayElement(i,order++);break;}
                    }
                }
                foreach(var metadata in world.metadata)
                {
                    var array=so.FindProperty(metadata.field);
                    for(int i=0;i<array.arraySize;i++)
                    {
                        var item=array.GetArrayElementAtIndex(i);
                        if(item.FindPropertyRelative("_stableId").stringValue==metadata.id)item.FindPropertyRelative(metadata.property).vector3Value=metadata.value;
                    }
                }
                foreach(var shard in world.shards)
                {
                    var profile=Resources.Load<FlowChallengeProfile>(shard.resource);
                    for(int i=0;i<profile.Shards.Length;i++)
                    {
                        if(profile.Shards[i].StableId!=shard.id)continue;
                        var item=profile.Shards[i];item.Position=shard.value;profile.Shards[i]=item;
                    }
                    EditorUtility.SetDirty(profile);
                }
                foreach(var anchor in world.anchors)
                {
                    var target=so.FindProperty(anchor.field);
                    if(anchor.field=="_restorePoints")
                    {
                        for(int i=0;i<target.arraySize;i++)
                        {
                            var item=target.GetArrayElementAtIndex(i);
                            if(item.FindPropertyRelative("_stableId").stringValue==anchor.id)
                            {
                                item.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=anchor.position;
                                if(!string.IsNullOrEmpty(anchor.support))item.FindPropertyRelative("_supportBlockStableId").stringValue=anchor.support;
                            }
                        }
                    }
                    else if(anchor.field=="_startPoint")target.FindPropertyRelative("_position").vector3Value=anchor.position;
                    else
                    {
                        target.FindPropertyRelative("_pose").FindPropertyRelative("_position").vector3Value=anchor.position;
                        if(!string.IsNullOrEmpty(anchor.support))target.FindPropertyRelative("_supportBlockStableId").stringValue=anchor.support;
                    }
                }
                if(world.world=="SolarFoundry")
                {
                    var mechanics=so.FindProperty("_mechanicsUsed");
                    foreach(int value in new[]{5,10})
                    {
                        bool present=false;for(int i=0;i<mechanics.arraySize;i++)present|=mechanics.GetArrayElementAtIndex(i).intValue==value;
                        if(!present){mechanics.InsertArrayElementAtIndex(mechanics.arraySize);mechanics.GetArrayElementAtIndex(mechanics.arraySize-1).intValue=value;}
                    }
                }
                so.FindProperty("_contentVersion").intValue=world.version;
                so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module);
                if(world.scenery!=null)
                {
                    var biome=new SerializedObject(module.EnvironmentBiomeProfile);
                    var objects=biome.FindProperty("_worldObjects");
                    foreach(var item in world.scenery)
                    for(int i=0;i<objects.arraySize;i++)
                    {
                        var e=objects.GetArrayElementAtIndex(i);
                        if(e.FindPropertyRelative("_stableId").stringValue==item.id)
                        {
                            e.FindPropertyRelative("_prefab").objectReferenceValue=WorldSceneryRefit.Bake(world,item);
                            e.FindPropertyRelative("_position").vector3Value=Vector3.zero;
                            e.FindPropertyRelative("_eulerAngles").vector3Value=Vector3.zero;
                            e.FindPropertyRelative("_scale").vector3Value=Vector3.one;
                        }
                    }
                    if(world.distant!=null)
                    foreach(var item in world.distant)
                    for(int i=0;i<objects.arraySize;i++)
                    {
                        var e=objects.GetArrayElementAtIndex(i);
                        if(e.FindPropertyRelative("_stableId").stringValue!=item.id)continue;
                        // Far dressing keeps its source transform unless the longer route
                        // approaches it; move the complete object beyond the route envelope.
                        var prefab=(GameObject)e.FindPropertyRelative("_prefab").objectReferenceValue;
                        var instance=UnityEngine.Object.Instantiate(prefab,item.sourcePosition,Quaternion.Euler(item.sourceRotation));
                        instance.transform.localScale=item.sourceScale;
                        try
                        {
                            var renderers=instance.GetComponentsInChildren<Renderer>();
                            var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                            var decks=world.entries.Where(a=>a.field=="_blocks").ToArray();
                            bool close=decks.Any(a=>{var delta=bounds.ClosestPoint(a.position)-a.position;delta.y=0;return delta.magnitude<=8.5f;});
                            var position=item.sourcePosition;
                            if(close)
                            {
                                float left=decks.Min(a=>a.position.x-a.size.x*.5f)-9-bounds.max.x;
                                float right=decks.Max(a=>a.position.x+a.size.x*.5f)+9-bounds.min.x;
                                position.x+=Mathf.Abs(left)<Mathf.Abs(right)?left:right;
                            }
                            e.FindPropertyRelative("_position").vector3Value=position;
                        }
                        finally{UnityEngine.Object.DestroyImmediate(instance);}
                    }
                    biome.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(module.EnvironmentBiomeProfile);
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}
