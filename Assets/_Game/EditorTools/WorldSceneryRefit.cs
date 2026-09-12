using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Avoidance.EditorTools
{
    internal static class WorldSceneryRefit
    {
        // Warp existing near architecture along the authored centreline. Its original
        // silhouette, materials and truthful fatal mesh collision remain paired.
        public static GameObject Bake(WorldFlowAuthoring.World world,WorldFlowAuthoring.Scenery item)
        {
            string root="Assets/_Game/Art/Environment/World170/Refit/"+world.world;
            Directory.CreateDirectory(root);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(item.sourceGuid));
            var clone=Object.Instantiate(source);clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);clone.transform.localScale=Vector3.one;
            var original=Matrix4x4.TRS(item.sourcePosition,Quaternion.Euler(item.sourceRotation),item.sourceScale);
            var route=world.entries.Where(e=>e.field=="_blocks"&&!e.id.Contains("mastery")&&!e.id.Contains("risky")&&e.id!="m03.sanctuary.bridge").OrderBy(e=>e.old.z).ToArray();
            float lateralShift=0;float verticalShift=0;
            Vector3 Map(Vector3 p)
            {
                int index=1;while(index<route.Length-1&&route[index].old.z<p.z)index++;
                var a=route[index-1];var b=route[index];float t=Mathf.InverseLerp(a.old.z,b.old.z,p.z);
                var old=Vector3.Lerp(a.old,b.old,t);var next=Vector3.Lerp(a.position,b.position,t);
                return p+next-old+Vector3.down*1.2f+Vector3.right*lateralShift+Vector3.up*verticalShift;
            }
            // Tall retained architecture frames the route instead of crossing the
            // flight corridor. Translate the entire multi-material object together.
            var bounds=new Bounds();bool hasBounds=false;
            foreach(var filter in clone.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.sharedMesh==null)continue;
                var transform=original*filter.transform.localToWorldMatrix;
                foreach(var vertex in filter.sharedMesh.vertices)
                {
                    var point=Map(transform.MultiplyPoint3x4(vertex));
                    if(!hasBounds){bounds=new Bounds(point,Vector3.zero);hasBounds=true;}else bounds.Encapsulate(point);
                }
            }
            bool crosses=false;float left=0,right=0;
            foreach(var deck in world.entries.Where(e=>e.field=="_blocks"))
            {
                var corridor=new Bounds(deck.position+Vector3.up*1.5f,new Vector3(deck.size.x+2,3.6f,deck.size.z+3));
                if(!bounds.Intersects(corridor))continue;
                crosses=true;left=Mathf.Min(left,corridor.min.x-bounds.max.x-1);right=Mathf.Max(right,corridor.max.x-bounds.min.x+1);
            }
            if(item.id.Contains("footing."))
            {
                var deck=world.entries.FirstOrDefault(e=>item.id.EndsWith(e.id));
                if(deck!=null){verticalShift=Mathf.Min(0,deck.position.y-deck.size.y*.5f-.6f-bounds.max.y);crosses=false;}
            }
            if(crosses)lateralShift=Mathf.Abs(left)<right?left:right;
            int serial=0;
            foreach(var filter in clone.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.sharedMesh==null)continue;
                var before=filter.sharedMesh;var mesh=Object.Instantiate(before);
                var toWorld=original*filter.transform.localToWorldMatrix;var fromWorld=filter.transform.worldToLocalMatrix;
                var vertices=mesh.vertices;
                for(int i=0;i<vertices.Length;i++)vertices[i]=fromWorld.MultiplyPoint3x4(Map(toWorld.MultiplyPoint3x4(vertices[i])));
                mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                string path=root+"/"+item.id+"-"+(serial++)+".asset";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing==null){mesh.name=item.id+"-mesh";AssetDatabase.CreateAsset(mesh,path);}
                else{EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}
                filter.sharedMesh=mesh;
                foreach(var collider in filter.GetComponents<MeshCollider>())collider.sharedMesh=mesh;
                var surface=filter.GetComponent<AuthoredSurface>();if(surface!=null)surface.SetSourceMesh(mesh);
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(clone,root+"/"+item.id+".prefab");Object.DestroyImmediate(clone);return prefab;
        }
    }
}
