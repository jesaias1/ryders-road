using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Avoidance.EditorTools
{
    // Explicit, repeatable authoring only. Startup never calls this builder.
    public static class SolarFoundryAuthoring
    {
        public const string ModuleId = "module.004.solar-foundry";
        public const string ModulePath = "Assets/_Game/Levels/Resources/Modules/Module_004_SolarFoundry.asset";
        public const string Root = "Assets/_Game/Art/Environment/SolarFoundry098";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("RYDERS BLOCK/Production/Author Solar Foundry")]
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Meshes");Directory.CreateDirectory(Root + "/Materials");
            Mat("Ceramic", new Color(.72f,.22f,.09f));
            Mat("Copper", new Color(.40f,.12f,.06f), .28f);
            Mat("Steel", new Color(.10f,.20f,.24f), .35f);
            Mat("Ivory", new Color(.82f,.80f,.65f));
            Mat("Gold", new Color(.95f,.63f,.14f), .4f);
            Mat("Glass", new Color(.12f,.47f,.57f), .65f);
            Mat("Light", new Color(.70f,.90f,.91f));
            var module = AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModulePath);
            if(module == null) { module = ScriptableObject.CreateInstance<ModuleDefinition>();AssetDatabase.CreateAsset(module, ModulePath); }
            ConfigureRoute(module);
            var spine = new Bake();
            foreach(var block in module.Blocks)
            {
                var p = block.Pose.Position;var s = block.Size;
                spine.Box("Steel", p + Vector3.down * 1.5f, new Vector3(s.x*.86f,2.2f,s.z*.86f));
                spine.Box("Copper", p + Vector3.down * 2.85f, new Vector3(s.x*.70f,.55f,s.z*.70f));
                if(s.x>=6) spine.Cylinder("Steel", p + Vector3.down * 6f, s.x*.20f, .55f, 6f);
                else
                {
                    spine.Beam("Steel",p+new Vector3(-s.x*.32f,-2.5f,0),p+new Vector3(s.x*.32f,-4.1f,0),new Vector2(.28f,.45f));
                    spine.Beam("Steel",p+new Vector3(s.x*.32f,-2.5f,0),p+new Vector3(-s.x*.32f,-4.1f,0),new Vector2(.28f,.45f));
                }
                // Under-route braces belong to the structures, never bridge the intended gaps.
                foreach(float x in new[]{-.36f,.36f})
                    spine.Box("Gold",p + new Vector3(x*s.x,-1.1f,0),new Vector3(.09f,.7f,s.z*.70f));
            }
            // Three industrial places, with the traversal lane kept on their open east side.
            var intake = new Bake();Tank(intake,new Vector3(-19,-13,15),7,18);
            for(int i=0;i<5;i++)
            {
                float z=7+i*3.6f;
                intake.Pipe(new Vector3(-13,-2,z),new Vector3(-8,-2,z),.7f);
                intake.Pipe(new Vector3(-8,-2,z),new Vector3(-8,-7,z),.7f);
            }
            intake.Pipe(new Vector3(-19,-9,6),new Vector3(-3,-9,6),1.1f);
            intake.Pipe(new Vector3(-3,-9,6),new Vector3(-3,-3,6),1.1f);
            var reactor = new Bake();Tank(reactor,new Vector3(-12,-18,55),10,48);
            reactor.Pipe(new Vector3(-3,-8,55),new Vector3(19,-8,55),1.5f);
            reactor.Pipe(new Vector3(19,-8,55),new Vector3(19,2,55),1.5f);
            var crown = new Bake();
            foreach(float x in new[]{-27f,-18f})
            {
                crown.Cylinder("Steel",new Vector3(x,-2,99),3.5f,2.3f,40);
                for(int fin=0;fin<11;fin++)
                    crown.Cylinder(fin%3==0?"Gold":"Ivory",new Vector3(x,-12+fin*2.7f,99),5,4.5f,.75f);
                crown.Cylinder("Copper",new Vector3(x,20,99),2.4f,1.2f,5);
            }
            crown.Beam("Steel",new Vector3(-27,13,99),new Vector3(-18,13,99),new Vector2(1,1.5f));
            crown.Pipe(new Vector3(-12,-5,103),new Vector3(-5,-5,103),1.25f);
            crown.Pipe(new Vector3(-5,-5,103),new Vector3(-5,7,103),1.25f);
            // Transfer machine: pylons sit beside the entire moving/capsule sweep.
            var crane = new Bake();
            foreach(var p in new[]{new Vector3(34,-5,39),new Vector3(28,-5,37)})
            {
                crane.Box("Steel",p,new Vector3(1.2f,25,1.2f));
                crane.Box("Ceramic",p+Vector3.up*5,new Vector3(2,7,2));
                for(int j=0;j<4;j++) crane.Box("Gold",p+new Vector3(0,j*2,0),new Vector3(2.12f,.18f,2.12f));
            }
            // Side-mounted davit: the transfer lane has open sky throughout the jump.
            crane.Beam("Steel",new Vector3(28,7.5f,37),new Vector3(34,7.5f,39),new Vector2(.65f,.8f));
            // A protective service house sits behind the final approach, never across it.
            crown.Box("Steel",new Vector3(-5,10.5f,108),new Vector3(8,3,2));
            crown.Box("Ivory",new Vector3(-5,12.2f,108),new Vector3(9,.35f,3));
            crown.Box("Glass",new Vector3(-5,11,106.98f),new Vector3(6,1.2f,.08f));
            var collector = Collector();
            var world = new[]
            {
                Place("service-spine",spine.Save("ServiceSpine",true),Vector3.zero,BiomeDepthBand.NearEnvironment),
                Place("intake-vessel",intake.Save("IntakeVessel",true),Vector3.zero,BiomeDepthBand.NearEnvironment),
                Place("reactor-vessel",reactor.Save("ReactorVessel",true),Vector3.zero,BiomeDepthBand.NearEnvironment),
                Place("crown-vessel",crown.Save("CrownVessel",true),Vector3.zero,BiomeDepthBand.NearEnvironment),
                Place("transfer-crane",crane.Save("TransferCrane",true),Vector3.zero,BiomeDepthBand.NearEnvironment),
                Place("heliostat.hero",collector,new Vector3(52,-17,120),BiomeDepthBand.MidWorld,1,-25),
                Place("heliostat.west",collector,new Vector3(-140,-44,175),BiomeDepthBand.FarWorld,.65f,35),
                Place("heliostat.north",collector,new Vector3(70,-55,230),BiomeDepthBand.FarWorld,.9f,-25),
                Place("lower.heat-exchanger",DistantWorks(),new Vector3(55,-85,15),BiomeDepthBand.LowerAbyss,1.1f,35)
            };
            var biomePath = "Assets/_Game/Worlds/Resources/EnvironmentBiomes/Biome_SolarFoundry.asset";
            var biome = AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(biomePath);
            if(biome == null) { biome = ScriptableObject.CreateInstance<EnvironmentBiomeProfile>();AssetDatabase.CreateAsset(biome,biomePath); }
            biome.Configure("biome.solar-foundry","Solar Foundry",EnvironmentBiomeKind.SolarFoundry,
                new Color(.73f,.79f,.83f),new Color(.75f,.82f,.85f),new Color(.52f,.67f,.74f),-65,
                Array.Empty<BiomeMistLayer>(),world);
            var so = new SerializedObject(biome);so.FindProperty("_combineStaticGeometry").boolValue = true;so.ApplyModifiedPropertiesWithoutUndo();
            module.ConfigureEnvironmentBiome(biome);ConfigureEnvironment(module);
            var flowPath = "Assets/_Game/Levels/Resources/FlowChallenges/Module004_FlowChallenge.asset";
            var flow = AssetDatabase.LoadAssetAtPath<FlowChallengeProfile>(flowPath);
            if(flow == null) { flow = ScriptableObject.CreateInstance<FlowChallengeProfile>();AssetDatabase.CreateAsset(flow,flowPath); }
            flow.ModuleId = ModuleId;
            flow.Shards = new[]{new FlowChallengeProfile.Shard{StableId="shard.m04.transfer-cut",Position=new Vector3(17.5f,5,30.5f)},
                new FlowChallengeProfile.Shard{StableId="shard.m04.transfer-exit",Position=new Vector3(23,5.4f,38.1f)}};
            EditorUtility.SetDirty(flow);EditorUtility.SetDirty(biome);EditorUtility.SetDirty(module);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }

        private static void ConfigureRoute(ModuleDefinition m)
        {
            ModuleBlockDefinition B(string id,float x,float y,float z,float w=3.6f,float d=3.6f,bool mastery=false)
                => new ModuleBlockDefinition("m04."+id,new Vector3(x,y,z),new Vector3(w,.6f,d),mastery?ModuleMaterialRole.Precision:ModuleMaterialRole.Normal);
            var blocks = new[]{
                B("intake.start",0,0,0,7,6),B("intake.pipe-a",0,.5f,5),B("intake.pipe-b",0,1,9.5f),
                B("intake.gantry",2,1.5f,14,7,4),B("inspection.a",5,2,19,4,4),B("inspection.b",8,2.5f,24,4,4),
                B("transfer.dock",11,3,29,6,5),B("transfer.exit",19,4,42,6,5),
                B("outer.a",22,4.5f,47),B("outer.b",24,5,51.5f),B("outer.c",24,5.5f,56),
                B("furnace.court",22,6,61,7,5),B("cooling.safe",11,7,73,6,5),
                B("crown.run-a",8,7.5f,78,4,4),B("crown.run-b",5,8,83,4,4),B("crown.restore",2,8.5f,88,7,5),
                B("crown.beam-a",0,9,93,4,4),B("crown.beam-b",-3,9.5f,98,4,4),B("patch.base",-5,10,103,8,6),
                B("mastery.transfer-a",17.5f,3.4f,30.5f,2.1f,2.1f,true),B("mastery.transfer-b",22.5f,3.6f,33,2.1f,2.1f,true),
                B("mastery.transfer-c",23,3.8f,38.1f,2.1f,2.1f,true)
            };
            var restores = new[]{
                new ModuleRestorePointDefinition("restore.module-004.transfer",0,new Vector3(11,3,29),new Vector3(0,.35f,0),supportBlockStableId:"m04.transfer.dock"),
                new ModuleRestorePointDefinition("restore.module-004.furnace",1,new Vector3(22,6,61),new Vector3(0,.35f,0),supportBlockStableId:"m04.furnace.court"),
                new ModuleRestorePointDefinition("restore.module-004.crown",2,new Vector3(2,8.5f,88),new Vector3(0,.35f,0),supportBlockStableId:"m04.crown.restore")
            };
            var moving = new[]{new ModuleMovingBlockDefinition("moving.m04.transfer",new Vector3(11,3.1f,34),new Vector3(4,.6f,3.6f),
                new[]{new Vector3(11,3.1f,34),new Vector3(19,3.6f,37)},3,.65f)};
            var crumble = new[]{new ModuleCrumblingBlockDefinition("crumble.m04.cooling-a",new Vector3(19,6.3f,66),new Vector3(3.6f,.6f,3.6f),.03f,1,4,.025f),
                new ModuleCrumblingBlockDefinition("crumble.m04.cooling-b",new Vector3(15,6.6f,69.5f),new Vector3(3.6f,.6f,3.6f),.03f,1,4,.025f)};
            m.Configure(ModuleId,"Campaign 04 - Solar Foundry","module_004_solar_foundry","project.ryders-block","world.solar-foundry",2,
                ModuleSelectionState.ModuleRunnerSceneName,ModuleDifficulty.Medium,new ModulePose(new Vector3(0,.35f,-1),Vector3.zero),
                new ModulePatchBlockDefinition("patch.module-004.solar-foundry",new Vector3(-5,10.3f,103),new Vector3(2.2f,2.2f,2.2f),supportBlockStableId:"m04.patch.base"),
                restores,48,110,new[]{ModuleMechanic.StandardBlock,ModuleMechanic.MovingBlock,ModuleMechanic.CrumblingBlock,ModuleMechanic.RestorePoint,ModuleMechanic.PatchBlock,ModuleMechanic.Shortcut},
                new[]{new ModuleShortcutDefinition("shortcut.module-004.transfer","Outer maintenance brackets",new Vector3(17.5f,3.4f,30.5f),new Vector3(19,4,42),4,new[]{ModuleMechanic.PrecisionBlock})},
                Resources.Load<ModuleEnvironmentProfile>("ModuleEnvironmentProfile"),Resources.Load<ModuleVisualProfile>("ModuleVisualProfile"),
                "Reconnect the suspended solar works.","Intake / Inspection Gantry / Transfer / Cooling Fins / Crown. Explicitly authored; no startup regeneration. Physical rank calibration pending.",
                blocks,moving,null,null,crumble,null,null);
            m.ConfigureRankThresholds(new ModuleRankThresholds(75,52,38,RankCalibrationState.Uncalibrated,1,"Provisional until physical S23 timing; Bronze always completes."));
            m.ConfigurePlayability("start.module-004.solar-foundry","m04.intake.start",-18,true);
        }

        private static void ConfigureEnvironment(ModuleDefinition module)
        {
            const string path="Assets/_Game/Visuals/Resources/ModuleEnvironment_SolarFoundry.asset";
            var env = AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(path);
            if(env == null) { env = Object.Instantiate(module.EnvironmentProfile);env.name="ModuleEnvironment_SolarFoundry";AssetDatabase.CreateAsset(env,path); }
            var skyPath=Root+"/Materials/FoundrySky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if(sky==null) { sky=new Material(AssetDatabase.LoadAssetAtPath<Material>(Phase074WorldSystemRecovery.AncientAbyssSkyPath));AssetDatabase.CreateAsset(sky,skyPath); }
            sky.SetFloat("_Exposure",.6f);sky.SetFloat("_Rotation",32);sky.SetColor("_Tint",new Color(1,.93f,.83f));EditorUtility.SetDirty(sky);
            var so = new SerializedObject(env);
            so.FindProperty("_skyboxMaterial").objectReferenceValue=sky;so.FindProperty("_overrideBiomeFog").boolValue=true;
            so.FindProperty("_fogDensity").floatValue=.0032f;so.FindProperty("_fogColor").colorValue=new Color(.65f,.73f,.77f);
            so.FindProperty("_sunIntensity").floatValue=1.25f;so.FindProperty("_ambientLight").colorValue=new Color(.70f,.76f,.79f);
            so.FindProperty("_sunLight").colorValue=new Color(1,.89f,.72f);so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(env);
            so=new SerializedObject(module);so.FindProperty("_environmentProfile").objectReferenceValue=env;so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static BiomeWorldObject Place(string id,GameObject prefab,Vector3 p,BiomeDepthBand band,float scale=1,float yaw=0)
            => new BiomeWorldObject("biome.solar-foundry."+id,prefab,p,new Vector3(0,yaw,0),Vector3.one*scale,true,band,.75f,band==BiomeDepthBand.NearEnvironment);
        private static void Tank(Bake b,Vector3 basePoint,float radius,float height)
        {
            b.Cylinder("Copper",basePoint+Vector3.up*height*.5f,radius*.65f,radius,height);
            b.Cylinder("Ceramic",basePoint+Vector3.up*(height*.57f),radius,radius*.92f,height*.72f);
            for(int i=0;i<5;i++)
            {
                float y=height*(.23f+i*.15f);
                b.Cylinder(i==4?"Ivory":"Steel",basePoint+Vector3.up*y,radius*1.025f,radius*1.025f,.55f);
            }
            b.Cylinder("Ivory",basePoint+Vector3.up*height,radius*.95f,radius*.65f,1.5f);
            b.Cylinder("Steel",basePoint+Vector3.up*(height+2),radius*.45f,radius*.4f,3);
            b.Cylinder("Gold",basePoint+Vector3.up*(height+3.65f),radius*.43f,radius*.43f,.3f);
            for(int i=0;i<12;i++)
            {
                float a=i*Mathf.PI/6;var p=basePoint+new Vector3(Mathf.Cos(a)*radius*.99f,height*.60f,Mathf.Sin(a)*radius*.99f);
                b.Box("Steel",p,new Vector3(.28f,height*.62f,.28f));
                b.Box("Gold",p+Vector3.up*height*.16f,new Vector3(.42f,height*.12f,.42f));
            }
        }
        private static GameObject Collector()
        {
            var b = new Bake();
            b.Cylinder("Steel",new Vector3(0,9,7),3,2,42);
            // Original concave heliostat: broad blue reflector with visible radial construction.
            var center=new Vector3(0,36,0);
            b.Dish("Glass",center,28,5);
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI/8;
                b.Beam("Ivory",center+new Vector3(0,0,5.05f),center+new Vector3(Mathf.Cos(a)*28,Mathf.Sin(a)*28,0),new Vector2(.28f,.4f));
            }
            b.Ring("Gold",center,28,.45f);b.Ring("Steel",center+Vector3.forward*2.5f,19,.35f);
            b.Cylinder("Copper",new Vector3(0,-12,7),9,4,5);
            return b.Save("SolarCollector",false);
        }
        private static GameObject DistantWorks()
        {
            var b=new Bake();Tank(b,new Vector3(-10,0,0),6,28);Tank(b,new Vector3(8,-7,7),9,37);
            b.Pipe(new Vector3(-10,5,0),new Vector3(8,5,7),2);
            return b.Save("LowerHeatExchanger",false);
        }
        private static void Mat(string name,Color color,float smooth=.15f)
        {
            var path=Root+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path); }
            material.SetColor("_BaseColor",color);material.SetFloat("_Smoothness",smooth);Materials[name]=material;EditorUtility.SetDirty(material);
        }

        private sealed class Bake
        {
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
            public void Pipe(Vector3 a,Vector3 b,float radius)
            {
                var scratch=new Bake();scratch.Cylinder("Steel",Vector3.zero,radius,radius,Vector3.Distance(a,b));
                var mesh=scratch.temporary[0];Add("Steel",mesh,(a+b)*.5f,Quaternion.FromToRotation(Vector3.up,b-a),Vector3.one);
            }
            public void Ring(string material,Vector3 center,float radius,float width)
            {
                for(int i=0;i<48;i++)
                {
                    float a=i*Mathf.PI/24,z=(i+1)*Mathf.PI/24;
                    Beam(material,center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),center+new Vector3(Mathf.Cos(z)*radius,Mathf.Sin(z)*radius,0),new Vector2(width,width));
                }
            }
            public void Dish(string material,Vector3 p,float radius,float depth)
            {
                var v=new List<Vector3>();var t=new List<int>();
                for(int ring=0;ring<4;ring++)for(int i=0;i<48;i++)
                {
                    Vector3 P(int r,int a) {float f=r/4f,angle=a*Mathf.PI/24;return new Vector3(Mathf.Cos(angle)*radius*f,Mathf.Sin(angle)*radius*f,depth*(1-f*f));}
                    int n=v.Count;v.AddRange(new[]{P(ring,i),P(ring,i+1),P(ring+1,i+1),P(ring+1,i)});
                    t.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
                }
                Add(material,Mesh(v,t),p,Quaternion.identity,Vector3.one);
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
}
