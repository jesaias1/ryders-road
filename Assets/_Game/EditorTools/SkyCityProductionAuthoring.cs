using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Avoidance.EditorTools
{
    // Editor-authored, baked architecture. Runtime consumes ordinary profile/prefab assets.
    public static class SkyCityProductionAuthoring
    {
        public const string Root = "Assets/_Game/Art/Environment/SkyCity093";
        private const string ModulePath = "Assets/_Game/Levels/Resources/Modules/Module_001_FirstSteps.asset";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("RYDERS BLOCK/Production/Author Module 001 Sky City")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Meshes"); Directory.CreateDirectory(Root + "/Materials");
            Mat("Porcelain", new Color(.84f,.83f,.72f));
            Mat("Limestone", new Color(.57f,.64f,.65f));
            Mat("Ocean", new Color(.035f,.21f,.29f), .3f);
            Mat("Glass", new Color(.07f,.43f,.51f), .65f);
            Mat("Brass", new Color(.83f,.5f,.14f), .45f);
            Mat("Light", new Color(.45f,.9f,.93f), .2f);
            Mat("Leaf", new Color(.18f,.42f,.12f));
            Mat("LeafSun", new Color(.48f,.65f,.15f));
            Mat("Bark", new Color(.28f,.19f,.11f));
            var arrival = Arrival(); var pier = BridgePier(); var garden = Garden();
            var gate = Gateway(); var overlook = Overlook();
            var city = District(); var hero = Observatory(); var bridge = Aqueduct();
            ConfigureModule();
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModulePath);
            var world = new List<BiomeWorldObject>();
            void Place(string id, GameObject prefab, Vector3 p, BiomeDepthBand band, float scale = 1, float yaw = 0)
                => world.Add(new BiomeWorldObject("biome.sky-city." + id, prefab, p, new Vector3(0,yaw,0), Vector3.one * scale, true, band, .7f, band == BiomeDepthBand.NearEnvironment));
            Place("arrival.terrace", arrival, Vector3.zero, BiomeDepthBand.NearEnvironment);
            foreach (var id in new[]{"m01.step.01","m01.step.02","m01.step.03","m01.turn.01","m01.turn.02","m01.flow.01","m01.flow.02","m01.flow.03","m01.final-runup"})
            {
                var block = module.Blocks.Single(b => b.StableId == id);
                Place("bridge-pier." + id, pier, block.Pose.Position + Vector3.down * .3f, BiomeDepthBand.NearEnvironment);
            }
            Place("garden.restore-court", garden, new Vector3(5.8f,.45f,25), BiomeDepthBand.NearEnvironment);
            Place("gateway.sun-portal", gate, new Vector3(7,1.15f,46.5f), BiomeDepthBand.NearEnvironment);
            Place("overlook.patch-terrace", overlook, new Vector3(7,1.15f,46.5f), BiomeDepthBand.NearEnvironment);
            Place("district.west-gardens", city, new Vector3(-31,-21,28), BiomeDepthBand.MidWorld, 1, 18);
            Place("district.east-marina", city, new Vector3(36,-28,51), BiomeDepthBand.MidWorld, 1.2f, -35);
            Place("landmark.sun-observatory", hero, new Vector3(-20,-19,79), BiomeDepthBand.MidWorld, 1, 12);
            Place("bridge.west-aqueduct", bridge, new Vector3(-36,-13,58), BiomeDepthBand.MidWorld, 1, -20);
            Place("district.north-crown", city, new Vector3(32,-30,130), BiomeDepthBand.FarWorld, 1.5f, -10);
            Place("district.west-horizon", city, new Vector3(-75,-38,114), BiomeDepthBand.FarWorld, 1.7f, 25);
            Place("district.east-horizon", hero, new Vector3(85,-48,118), BiomeDepthBand.FarWorld, 1.35f, -30);
            Place("district.lower-port", city, new Vector3(-25,-75,3), BiomeDepthBand.LowerAbyss, 1.3f, 65);
            var biome = AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>("Assets/_Game/Worlds/Resources/EnvironmentBiomes/Biome_SkyCity.asset");
            biome.Configure("biome.sky-city", "Sky City", EnvironmentBiomeKind.SkyCity,
                new Color(.76f,.86f,.92f), new Color(.78f,.9f,1,.2f), new Color(.5f,.7f,.83f), -58,
                Array.Empty<BiomeMistLayer>(), world.ToArray());
            EditorUtility.SetDirty(biome); ConfigureEnvironment(module);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("Sky City 0.9.3 authored: five places, " + world.Count + " world placements; route v2.");
        }

        private static void ConfigureModule()
        {
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModulePath);
            var serialized = new SerializedObject(module);
            serialized.FindProperty("_contentVersion").intValue = 2;
            serialized.FindProperty("_displayName").stringValue = "Campaign 01 - First Steps";
            serialized.FindProperty("_developerNotes").stringValue = "Sky City: Arrival Terrace, Broken Sky Bridge, Suspended Garden, Sun Gateway, Patch Overlook. Welcoming roof widths with original centers and jump cadence. V2 rank times require physical calibration.";
            serialized.FindProperty("_loreBugDescription").stringValue = "Restore the first broken connection through the sunlit rooftops of Sky City.";
            var blocks = serialized.FindProperty("_blocks");
            for (var i=0;i<blocks.arraySize;i++)
            {
                var b=blocks.GetArrayElementAtIndex(i); var id=b.FindPropertyRelative("_stableId").stringValue;
                var size = new Vector3(2.5f,.6f,2.5f);
                if (id=="m01.start") size=new Vector3(6,.6f,4);
                if (id=="m01.restore.approach") size=new Vector3(5,.6f,3.4f);
                if (id=="m01.final-runup") size=new Vector3(3.4f,.6f,2.2f);
                if (id=="m01.patch.base") size=new Vector3(6,.6f,4);
                b.FindPropertyRelative("_size").vector3Value=size;
                b.FindPropertyRelative("_editorLabel").boolValue=false;
            }
            serialized.FindProperty("_decorations").arraySize=0;
            serialized.FindProperty("_disableAutomaticDistantFragments").boolValue=true;
            serialized.FindProperty("_startAnchorStableId").stringValue="start.module-001.arrival";
            serialized.FindProperty("_startSupportBlockStableId").stringValue="m01.start";
            serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(module);
        }
        private static void ConfigureEnvironment(ModuleDefinition module)
        {
            var path="Assets/_Game/Visuals/Resources/ModuleEnvironment_SkyCity.asset";
            var env=AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(path);
            if(env==null) { env=Object.Instantiate(module.EnvironmentProfile); env.name="ModuleEnvironment_SkyCity"; AssetDatabase.CreateAsset(env,path); }
            var skyPath=Root+"/Materials/SkyCity_CloudOcean.mat";
            var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if(sky==null) { sky=new Material(AssetDatabase.LoadAssetAtPath<Material>(Phase074WorldSystemRecovery.AncientAbyssSkyPath)); AssetDatabase.CreateAsset(sky,skyPath); }
            sky.SetFloat("_Exposure",.58f); sky.SetFloat("_Rotation",32); sky.SetColor("_Tint",new Color(.92f,.97f,1)); EditorUtility.SetDirty(sky);
            var so=new SerializedObject(env);
            so.FindProperty("_skyboxMaterial").objectReferenceValue=sky;
            so.FindProperty("_overrideBiomeFog").boolValue=true;
            so.FindProperty("_fogDensity").floatValue=.004f;
            so.FindProperty("_fogColor").colorValue=new Color(.55f,.72f,.82f);
            so.FindProperty("_sunIntensity").floatValue=1.35f;
            so.FindProperty("_ambientLight").colorValue=new Color(.62f,.74f,.83f);
            so.FindProperty("_sunLight").colorValue=new Color(1,.93f,.8f);
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(env);
            var m=new SerializedObject(module); m.FindProperty("_environmentProfile").objectReferenceValue=env; m.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(module);
        }

        private static GameObject Arrival()
        {
            var b=new Sculpt();
            b.Prism("Ocean",new Vector3(0,-2.3f,0),new Vector3(5.8f,3.8f,3.8f),.6f);
            b.Prism("Limestone",new Vector3(0,-4.9f,0),new Vector3(3.7f,2,2.4f),.18f);
            b.Prism("Brass",new Vector3(0,-.55f,0),new Vector3(6,.16f,4),1);
            // Side garden ledges are real, colliding terraces; the departure lane stays clear.
            foreach(var x in new[]{-4.15f,4.15f})
            {
                b.Prism("Porcelain",new Vector3(x,-.14f,-.6f),new Vector3(2.3f,.85f,4.2f),.85f);
                b.Box("Ocean",new Vector3(x,0,-1.2f),new Vector3(1.5f,.7f,1.5f));
                b.Tree(new Vector3(x,.35f,-1.2f),.9f);
                foreach(var z in new[]{-.5f,.4f,1.3f})
                    b.Box("Brass",new Vector3(x+Mathf.Sign(x)*.7f,.65f,z),new Vector3(.08f,.9f,.08f));
                b.Box("Brass",new Vector3(x+Mathf.Sign(x)*.7f,1.1f,.4f),new Vector3(.08f,.08f,2));
            }
            b.Box("Limestone",new Vector3(0,-.15f,-2.7f),new Vector3(10.5f,.9f,1.4f));
            return b.Save("ArrivalTerrace",true);
        }
        private static GameObject BridgePier()
        {
            var b=new Sculpt();
            b.Prism("Porcelain",new Vector3(0,-.25f,0),new Vector3(2.4f,.5f,2.4f),.86f);
            b.Prism("Brass",new Vector3(0,-.65f,0),new Vector3(2.15f,.14f,2.15f),1);
            b.Prism("Ocean",new Vector3(0,-1.45f,0),new Vector3(2,1.5f,2),.64f);
            b.Prism("Limestone",new Vector3(0,-2.6f,0),new Vector3(1.3f,.8f,1.3f),.2f);
            foreach(var x in new[]{-.92f,.92f}) b.Box("Light",new Vector3(x,-1.4f,0),new Vector3(.1f,.9f,.2f));
            return b.Save("BrokenBridgePier",true);
        }
        private static GameObject Garden()
        {
            var b=new Sculpt();
            b.Prism("Ocean",new Vector3(0,-2.4f,0),new Vector3(5.2f,4.2f,3.5f),.4f);
            b.Prism("Brass",new Vector3(0,-.47f,0),new Vector3(5.3f,.2f,3.6f),1);
            foreach(var side in new[]{-1,1})
            {
                float x=side*3.65f;
                b.Prism("Porcelain",new Vector3(x,-.25f,0),new Vector3(2.5f,1.1f,3.4f),.78f);
                b.Prism("Limestone",new Vector3(x,-2.2f,0),new Vector3(2.2f,3,2.8f),.2f);
                b.Prism("Ocean",new Vector3(x,.36f,.25f),new Vector3(1.8f,.35f,2.2f),1);
                b.Tree(new Vector3(x,.55f,.7f),.9f);
                b.Box("Porcelain",new Vector3(x,.45f,-1.25f),new Vector3(1.9f,.3f,.35f));
                foreach(var z in new[]{-1.2f,0,1.2f})
                    b.Box("Brass",new Vector3(x+side*.95f,.7f,z),new Vector3(.08f,.8f,.08f));
                b.Box("Brass",new Vector3(x+side*.95f,1.1f,0),new Vector3(.08f,.08f,2.5f));
            }
            return b.Save("SuspendedGarden",true);
        }
        private static GameObject Gateway()
        {
            var b=new Sculpt();
            foreach(var x in new[]{-4.35f,4.35f})
            {
                b.Prism("Porcelain",new Vector3(x,2.5f,1.15f),new Vector3(1,6.3f,1.3f),1.15f);
                b.Box("Ocean",new Vector3(x,2.7f,.46f),new Vector3(.55f,3.5f,.1f));
                b.Box("Brass",new Vector3(x,4.5f,.38f),new Vector3(.72f,.12f,.15f));
                b.Prism("Brass",new Vector3(x,5.7f,1.15f),new Vector3(1.2f,.22f,1.5f),1);
            }
            b.Arc("Porcelain",new Vector3(0,5.4f,1.15f),4.35f,.42f,.8f,0,180);
            b.Arc("Brass",new Vector3(0,5.4f,.69f),4.35f,.12f,.14f,0,180);
            b.Arc("Light",new Vector3(0,5.4f,.66f),3.96f,.055f,.07f,10,170);
            b.Prism("Brass",new Vector3(0,10.1f,1.15f),new Vector3(.4f,.75f,.4f),.2f);
            return b.Save("SunGateway",true);
        }
        private static GameObject Overlook()
        {
            var b=new Sculpt();
            b.Prism("Ocean",new Vector3(0,-3,0),new Vector3(6,5.4f,4),.5f);
            b.Prism("Limestone",new Vector3(0,-6.6f,0),new Vector3(3,2,2),.08f);
            b.Prism("Brass",new Vector3(0,-.45f,0),new Vector3(6.2f,.2f,4.2f),1);
            foreach(var x in new[]{-4f,4f})
            {
                b.Prism("Porcelain",new Vector3(x,-.2f,.4f),new Vector3(2.2f,1,4.7f),.8f);
                b.Prism("Limestone",new Vector3(x,-2.4f,.4f),new Vector3(1.7f,3.4f,3.5f),.35f);
                b.Tree(new Vector3(x,.3f,-1.3f),.8f);
            }
            b.Box("Porcelain",new Vector3(0,-.1f,2.65f),new Vector3(9.7f,.8f,1.3f));
            b.Box("Brass",new Vector3(0,1,3.16f),new Vector3(9.7f,.12f,.1f));
            for(var x=-4;x<=4;x+=2) b.Box("Porcelain",new Vector3(x,.5f,3.16f),new Vector3(.16f,1,.16f));
            return b.Save("PatchOverlook",true);
        }
        private static GameObject District()
        {
            var b=new Sculpt();
            b.Prism("Limestone",new Vector3(0,-5,0),new Vector3(23,9,16),.28f);
            b.Prism("Porcelain",new Vector3(0,-.45f,0),new Vector3(23,.9f,16),1);
            b.Prism("Brass",new Vector3(0,-1.1f,0),new Vector3(23.1f,.2f,16.1f),1);
            b.Tower(new Vector3(-6,0,1),6,18,5);
            b.Tower(new Vector3(4,0,3),5,25,5);
            b.Tower(new Vector3(7,0,-4),5,10,4);
            b.Tower(new Vector3(-3,0,-4),5,7,4);
            b.Box("Porcelain",new Vector3(0,9,2),new Vector3(10,.6f,2));
            b.Box("Brass",new Vector3(0,9.65f,1),new Vector3(10,.1f,.1f));
            foreach(var p in new[]{new Vector3(-9,0,-4),new Vector3(0,0,-5),new Vector3(9,0,5)}) b.Tree(p,1.8f);
            return b.Save("TerracedSkyDistrict",false);
        }
        private static GameObject Observatory()
        {
            var b=new Sculpt();
            b.Prism("Limestone",new Vector3(0,-4,0),new Vector3(19,8,14),.22f);
            b.Prism("Porcelain",new Vector3(0,.1f,0),new Vector3(20,1,15),1);
            b.Tower(new Vector3(-5,0,0),5,33,5);
            b.Tower(new Vector3(5,0,0),5,26,5);
            b.Box("Porcelain",new Vector3(0,19,0),new Vector3(13,1,3.5f));
            b.Arc("Brass",new Vector3(0,30,0),7,.35f,.8f,0,360);
            b.Arc("Porcelain",new Vector3(0,30,.15f),7.5f,.24f,.5f,12,168);
            b.Tower(new Vector3(-4,0,-5),5,7,3);
            foreach(var x in new[]{-8f,8f}) b.Tree(new Vector3(x,.5f,-3),2);
            return b.Save("SunObservatory",false);
        }
        private static GameObject Aqueduct()
        {
            var b=new Sculpt();
            b.Box("Porcelain",new Vector3(0,0,0),new Vector3(28,.8f,3));
            b.Box("Brass",new Vector3(0,.5f,-1.5f),new Vector3(28,.12f,.1f));
            foreach(var x in new[]{-10f,0f,10f})
            {
                b.Prism("Limestone",new Vector3(x,-9,0),new Vector3(2.2f,18,2.2f),.6f);
                b.Arc("Porcelain",new Vector3(x+5,-4,0),4.5f,.5f,1.8f,0,180);
            }
            return b.Save("CloudAqueduct",false);
        }
        private static void Mat(string name,Color color,float smooth=0)
        {
            var path=Root+"/Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path); }
            mat.SetColor("_BaseColor",color); mat.SetFloat("_Smoothness",smooth); mat.SetFloat("_Metallic",name=="Brass"?.35f:0);
            Materials[name]=mat; EditorUtility.SetDirty(mat);
        }

        private sealed class Sculpt
        {
            private readonly Dictionary<string,List<CombineInstance>> _parts=new Dictionary<string,List<CombineInstance>>();
            private readonly List<Mesh> _temporary=new List<Mesh>();
            private void Add(string material,Mesh mesh,Vector3 p,Quaternion rotation,Vector3 scale)
            {
                if(!_parts.ContainsKey(material)) _parts[material]=new List<CombineInstance>();
                _parts[material].Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,rotation,scale)}); _temporary.Add(mesh);
            }
            public void Box(string material,Vector3 p,Vector3 size) => Prism(material,p,size,1,.001f);
            public void Prism(string material,Vector3 p,Vector3 size,float bottomScale,float bevel=.16f)
            {
                var points=new[]{new Vector2(-.5f+bevel,-.5f),new Vector2(.5f-bevel,-.5f),new Vector2(.5f,-.5f+bevel),new Vector2(.5f,.5f-bevel),new Vector2(.5f-bevel,.5f),new Vector2(-.5f+bevel,.5f),new Vector2(-.5f,.5f-bevel),new Vector2(-.5f,-.5f+bevel)};
                var v=new List<Vector3>(); var t=new List<int>();
                void Tri(Vector3 a,Vector3 b,Vector3 c) { int n=v.Count; v.Add(a);v.Add(b);v.Add(c);t.Add(n);t.Add(n+1);t.Add(n+2); }
                for(int i=0;i<8;i++)
                {
                    var a=points[i];var z=points[(i+1)%8];
                    var at=new Vector3(a.x,.5f,a.y);var bt=new Vector3(z.x,.5f,z.y);
                    var ab=new Vector3(a.x*bottomScale,-.5f,a.y*bottomScale);var bb=new Vector3(z.x*bottomScale,-.5f,z.y*bottomScale);
                    Tri(at,bt,ab);Tri(bt,bb,ab);Tri(Vector3.up*.5f,bt,at);Tri(Vector3.down*.5f,ab,bb);
                }
                var m=new Mesh();m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds(); Add(material,m,p,Quaternion.identity,size);
            }
            public void Arc(string material,Vector3 center,float radius,float thickness,float depth,float from,float to)
            {
                // Solid annular extrusion with exact visible collision for near architecture.
                var verts=new List<Vector3>();var tris=new List<int>();int count=Mathf.CeilToInt((to-from)/7.5f);
                void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=verts.Count;verts.AddRange(new[]{a,b,c,d});tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
                for(int i=0;i<count;i++)
                {
                    float a=Mathf.Lerp(from,to,i/(float)count)*Mathf.Deg2Rad,z=Mathf.Lerp(from,to,(i+1)/(float)count)*Mathf.Deg2Rad;
                    Vector3 P(float angle,float r,float dz)=>new Vector3(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r,dz);
                    float outer=radius+thickness,inner=radius-thickness,h=depth*.5f;
                    var af=P(a,outer,-h);var bf=P(z,outer,-h);var cf=P(z,inner,-h);var df=P(a,inner,-h);
                    var ab=P(a,outer,h);var bb=P(z,outer,h);var cb=P(z,inner,h);var db=P(a,inner,h);
                    Quad(df,cf,bf,af);Quad(ab,bb,cb,db);Quad(af,bf,bb,ab);Quad(db,cb,cf,df);
                    if(i==0)Quad(af,ab,db,df);if(i==count-1)Quad(cf,cb,bb,bf);
                }
                var m=new Mesh();m.SetVertices(verts);m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateBounds();Add(material,m,center,Quaternion.identity,Vector3.one);
            }
            public void Tree(Vector3 p,float scale)
            {
                if (scale <= 1.4f) { ApprovedGardenTree(p, scale); return; }
                Prism("Bark",p+Vector3.up*scale,new Vector3(.23f,2*scale,.23f),1);
                for(var i=0;i<3;i++)
                {
                    var primitive=GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var mesh=Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);Object.DestroyImmediate(primitive);
                    var offset=new Vector3(i==0?-.45f:i==1?.5f:0,2.1f+i*.35f,i==1?.25f:0)*scale;
                    Add(i==1?"Leaf":"LeafSun",mesh,p+offset,Quaternion.identity,new Vector3(1.9f,.8f,1.65f)*scale);
                }
            }
            private void ApprovedGardenTree(Vector3 p,float scale)
            {
                // Reuse the approved textured bonsai crown; its floating-rock base is not part of a city planter.
                var source=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Art/Environment/Nature/PF_RR_Tree_Floating.prefab"));
                var renderers=source.GetComponentsInChildren<MeshRenderer>();
                var bounds=renderers[0].bounds; foreach(var r in renderers) bounds.Encapsulate(r.bounds);
                var cut=bounds.min.y+bounds.size.y*.49f;
                float resize=3.3f*scale/(bounds.max.y-cut);
                foreach(var filter in source.GetComponentsInChildren<MeshFilter>())
                {
                    var original=filter.sharedMesh; var vertices=original.vertices;
                    var transformed=vertices.Select(v => filter.transform.TransformPoint(v)).ToArray();
                    var kept=new List<int>(); var triangles=original.triangles;
                    for(var i=0;i<triangles.Length;i+=3)
                    {
                        if(transformed[triangles[i]].y<cut || transformed[triangles[i+1]].y<cut || transformed[triangles[i+2]].y<cut) continue;
                        kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);
                    }
                    var mesh=new Mesh{indexFormat=IndexFormat.UInt32};
                    mesh.vertices=transformed.Select(v => (v-new Vector3(bounds.center.x,cut,bounds.center.z))*resize).ToArray();
                    mesh.uv=original.uv;mesh.triangles=kept.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
                    Materials["Leaf_ApprovedTree"]=filter.GetComponent<MeshRenderer>().sharedMaterial;
                    Add("Leaf_ApprovedTree",mesh,p,Quaternion.identity,Vector3.one);
                }
                Object.DestroyImmediate(source);
            }
            public void Tower(Vector3 p,float width,float height,float depth)
            {
                Prism("Ocean",p+Vector3.up*(height*.5f),new Vector3(width,height,depth),1);
                // Continuous recessed glazing is broken by readable floor cornices and ivory ribs.
                for(float y=2;y<height;y+=3)
                {
                    Prism("Glass",p+Vector3.up*y,new Vector3(width+.03f,1.8f,depth+.03f),1);
                    Prism("Porcelain",p+Vector3.up*(y+1.05f),new Vector3(width+.25f,.28f,depth+.25f),1);
                    foreach(var x in new[]{-width*.22f,0,width*.22f})
                    {
                        Box("Limestone",p+new Vector3(x,y,-depth*.505f),new Vector3(.09f,1.8f,.09f));
                        Box("Limestone",p+new Vector3(x,y,depth*.505f),new Vector3(.09f,1.8f,.09f));
                    }
                    foreach(var z in new[]{-depth*.22f,0,depth*.22f})
                    {
                        Box("Limestone",p+new Vector3(-width*.505f,y,z),new Vector3(.09f,1.8f,.09f));
                        Box("Limestone",p+new Vector3(width*.505f,y,z),new Vector3(.09f,1.8f,.09f));
                    }
                }
                foreach(var x in new[]{-width*.42f,width*.42f})
                    foreach(var z in new[]{-depth*.42f,depth*.42f}) Box("Porcelain",p+new Vector3(x,height*.5f,z),new Vector3(.35f,height+.8f,.35f));
                Prism("Porcelain",p+Vector3.up*(height+.1f),new Vector3(width+1,.65f,depth+1),1);
                Prism("Brass",p+Vector3.up*(height+.54f),new Vector3(width+.8f,.18f,depth+.8f),1);
                Prism("Ocean",p+Vector3.up*(height+1.2f),new Vector3(width*.6f,1.2f,depth*.6f),1.2f);
                Prism("Porcelain",p+Vector3.up*(height+2),new Vector3(width*.6f,.5f,depth*.6f),1);
            }
            public GameObject Save(string name,bool collide)
            {
                var root=new GameObject(name);
                foreach(var part in _parts)
                {
                    var mesh=new Mesh { name=name+"_"+part.Key,indexFormat=IndexFormat.UInt32 };
                    mesh.CombineMeshes(part.Value.ToArray(),true,true); mesh.RecalculateBounds();
                    var meshPath=Root+"/Meshes/"+mesh.name+".asset";
                    var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    if(existing==null) AssetDatabase.CreateAsset(mesh,meshPath);
                    else { EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(existing); }
                    var obj=new GameObject(part.Key,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(root.transform,false);
                    obj.GetComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=Materials[part.Key];renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                    if(collide && !part.Key.StartsWith("Leaf"))
                    {
                        obj.AddComponent<MeshCollider>().sharedMesh=mesh; obj.AddComponent<AuthoredSurface>().SetSourceMesh(mesh);
                    }
                }
                foreach(var mesh in _temporary) Object.DestroyImmediate(mesh);
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+name+".prefab");Object.DestroyImmediate(root);return prefab;
            }
        }
    }
}
