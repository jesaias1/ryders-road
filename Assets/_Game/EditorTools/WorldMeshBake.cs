using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Avoidance.Gameplay.Worlds;
using Object=UnityEngine.Object;
namespace Avoidance.EditorTools
{
        internal sealed class WorldMeshBake
        {
            private readonly string Root;
            private readonly Dictionary<string, Material> Materials;
            public WorldMeshBake(string root, Dictionary<string,Material> materials) { Root=root; Materials=materials; }
            private readonly Dictionary<string,List<CombineInstance>> parts=new Dictionary<string,List<CombineInstance>>();
            private readonly List<Mesh> temporary=new List<Mesh>();
            private void Add(string material,Mesh mesh,Vector3 p,Quaternion rotation,Vector3 scale)
            {
                if(!parts.ContainsKey(material))parts[material]=new List<CombineInstance>();
                parts[material].Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,rotation,scale)});temporary.Add(mesh);
            }
            public void Box(string material,Vector3 p,Vector3 size) => Beam(material,p-Vector3.up*size.y*.5f,p+Vector3.up*size.y*.5f,new Vector2(size.x,size.z));
            public void Beam(string material,Vector3 a,Vector3 b,Vector2 width)
            {
                var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var mesh=Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);Object.DestroyImmediate(primitive);
                Add(material,mesh,(a+b)*.5f,Quaternion.FromToRotation(Vector3.up,b-a),new Vector3(width.x,Vector3.Distance(a,b),width.y));
            }
            public void Cylinder(string material,Vector3 p,float bottomRadius,float topRadius,float height)
            {
                var v=new List<Vector3>();var t=new List<int>();
                void Tri(Vector3 a,Vector3 b,Vector3 c) {int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});}
                for(int i=0;i<24;i++)
                {
                    float a=i*Mathf.PI/12,z=(i+1)*Mathf.PI/12;
                    var ab=new Vector3(Mathf.Cos(a)*bottomRadius,-height*.5f,Mathf.Sin(a)*bottomRadius);
                    var bb=new Vector3(Mathf.Cos(z)*bottomRadius,-height*.5f,Mathf.Sin(z)*bottomRadius);
                    var at=new Vector3(Mathf.Cos(a)*topRadius,height*.5f,Mathf.Sin(a)*topRadius);
                    var bt=new Vector3(Mathf.Cos(z)*topRadius,height*.5f,Mathf.Sin(z)*topRadius);
                    Tri(ab,at,bb);Tri(bb,at,bt);Tri(Vector3.up*height*.5f,bt,at);Tri(Vector3.down*height*.5f,ab,bb);
                }
                Add(material,Mesh(v,t),p,Quaternion.identity,Vector3.one);
            }
            public void Ring(string material,Vector3 center,float radius,float width)
            {
                for(int i=0;i<48;i++)
                {
                    float a=i*Mathf.PI/24,z=(i+1)*Mathf.PI/24;
                    Beam(material,center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),center+new Vector3(Mathf.Cos(z)*radius,Mathf.Sin(z)*radius,0),new Vector2(width,width));
                }
            }
            private static Mesh Mesh(List<Vector3> v,List<int> t)
            {var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
            public GameObject Save(string name,bool collide)
            {
                var root=new GameObject(name);
                foreach(var part in parts)
                {
                    var mesh=new Mesh{name=name+"_"+part.Key,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(part.Value.ToArray(),true,true);
                    string path=Root+"/Meshes/"+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(existing==null)AssetDatabase.CreateAsset(mesh,path);
                    else {EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(existing);}
                    var go=new GameObject(part.Key,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);
                    go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=Materials[part.Key];r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
                    if(collide)
                    {
                        go.AddComponent<MeshCollider>().sharedMesh=mesh;
                        var surface=go.AddComponent<AuthoredSurface>();surface.SetSourceMesh(mesh);
                        surface.SetRestoreOnLanding(true);
                    }
                }
                foreach(var mesh in temporary)Object.DestroyImmediate(mesh);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+name+".prefab");Object.DestroyImmediate(root);return prefab;
            }
        }
}
