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
    // Bakes original fractured geology and small mountain structures; no runtime generation.
    public static class MountainWorldProductionAuthoring
    {
        public const string Root = "Assets/_Game/Art/Environment/Mountain094";
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        [MenuItem("RYDERS BLOCK/Production/Author Module 002 Mountain World")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Meshes"); Directory.CreateDirectory(Root+"/Materials");
            CreateStoneTexture();
            Mat("Rock",new Color(.47f,.48f,.43f)); Mat("Strata",new Color(.58f,.56f,.49f));
            Mat("Stone",new Color(.77f,.74f,.61f)); Mat("Snow",new Color(.83f,.91f,.92f));
            Mat("Moss",new Color(.31f,.42f,.17f)); Mat("Leaf",new Color(.15f,.32f,.16f));
            Mat("LeafSun",new Color(.36f,.5f,.19f)); Mat("Bark",new Color(.29f,.23f,.16f));
            Mat("Metal",new Color(.21f,.3f,.32f)); Mat("Gold",new Color(.84f,.49f,.16f));
            Mat("Water",new Color(.17f,.67f,.74f)); Mat("Foam",new Color(.69f,.91f,.92f));
            var module=Resources.Load<ModuleDefinition>("Modules/Module_002_MovingParts");
            ConfigureRoute(module); ConfigureEnvironment(module);
            var world=new List<BiomeWorldObject>();
            void Place(string id,GameObject prefab,Vector3 p,BiomeDepthBand band,float scale=1,float yaw=0)
                => world.Add(new BiomeWorldObject("biome.mountain-sky."+id,prefab,p,new Vector3(0,yaw,0),Vector3.one*scale,true,band,.8f,band==BiomeDepthBand.NearEnvironment));
            // Actual footings are narrowly cut to preserve every intended gap.
            foreach(var block in module.Blocks)
            {
                var b=new Sculpt(); var size=block.Size; var isCourt=size.x>=5;
                var seed=block.StableId.Aggregate(17,(a,c)=>unchecked(a*31+c)) & 0x7fffffff;
                float depth=5+seed%8;
                b.Rock(new Vector3(0,-depth-.3f,0),new Vector3(size.x*.49f,depth,size.z*.49f),seed, false, false);
                if(isCourt)
                {
                    b.Rock(new Vector3(-size.x*.48f,-3.6f,-.4f),new Vector3(2.4f,3,2.2f),37,false,false);

                }
                Place("footing."+block.StableId,b.Save("Footing_"+block.StableId,true),block.Pose.Position,BiomeDepthBand.NearEnvironment);
            }
            Place("arrival.cliff",Cliffside(),Vector3.zero,BiomeDepthBand.NearEnvironment);
            Place("pass.fractured-abutment",Pass(),new Vector3(0,1.2f,8.5f),BiomeDepthBand.NearEnvironment);
            Place("garden.waterfall-shrine",Garden(),new Vector3(4,3,26),BiomeDepthBand.NearEnvironment);
            Place("ridge.old-lift",Lift(),new Vector3(6,7.2f,58),BiomeDepthBand.NearEnvironment);
            Place("summit.stone-crown",Summit(),new Vector3(8,12.4f,93),BiomeDepthBand.NearEnvironment);
            var shelf=new Sculpt();
            shelf.Rock(new Vector3(-8,-10,6),new Vector3(4,9,10),203,false);
            shelf.Rock(new Vector3(-10,-15,-3),new Vector3(4,13,8),913,false);
            shelf.Pine(new Vector3(-8,-1,6),4.8f);
            Place("garden.fractured-wall",shelf.Save("GardenCliffWall",true),new Vector3(4,3,26),BiomeDepthBand.NearEnvironment);
            var upper=new Sculpt();upper.Rock(new Vector3(8,-12,0),new Vector3(4.5f,11,12),731,false);
            upper.Rock(new Vector3(9,-16,9),new Vector3(5,14,8),613,false);upper.Pine(new Vector3(8,-1,0),3.1f);
            Place("ridge.fractured-wall",upper.Save("WindwardCliffWall",true),new Vector3(12,9.4f,73),BiomeDepthBand.NearEnvironment);
            var valley=new Sculpt();valley.Rock(new Vector3(0,-12,0),new Vector3(7,11,11),411,false,false);
            Place("garden.valley-foundation",valley.Save("GardenValleyFoundation",true),new Vector3(0,0,39),BiomeDepthBand.NearEnvironment);
            var spine=new Sculpt();spine.Rock(new Vector3(0,-14,0),new Vector3(5.5f,12,12),809,false,false);
            Place("ridge.upper-foundation",spine.Save("UpperRidgeFoundation",true),new Vector3(13,9,81),BiomeDepthBand.NearEnvironment);
            var ridge=Mountain("AlpineRidge",31,false); var peak=Mountain("SnowPeak",73,true);
            Place("west.headwall",ridge,new Vector3(-65,-49,19),BiomeDepthBand.MidWorld,1.1f,25);
            Place("east.waterfall-headwall",ridge,new Vector3(78,-42,44),BiomeDepthBand.MidWorld,1.25f,-25);
            Place("west.high-ridge",ridge,new Vector3(-58,-40,124),BiomeDepthBand.MidWorld,1.2f,55);
            Place("landmark.split-peak",SplitPeak(),new Vector3(-48,-15,64),BiomeDepthBand.MidWorld);
            Place("west.valley",peak,new Vector3(-165,-60,180),BiomeDepthBand.FarWorld,2.2f,13);
            Place("north.range",peak,new Vector3(-26,-58,280),BiomeDepthBand.FarWorld,2.6f,-31);
            Place("east.range",peak,new Vector3(195,-65,240),BiomeDepthBand.FarWorld,2.4f,75);
            Place("very-far.west",peak,new Vector3(-270,-80,460),BiomeDepthBand.FarWorld,4.6f,20);
            Place("very-far.east",peak,new Vector3(280,-90,520),BiomeDepthBand.FarWorld,5,55);
            Place("below.valley-spur",ridge,new Vector3(-30,-105,-24),BiomeDepthBand.LowerAbyss,1.8f,10);
            var biome=module.EnvironmentBiomeProfile;
            biome.Configure("biome.mountain-sky","Mountain World",EnvironmentBiomeKind.MountainSky,new Color(.76f,.84f,.91f),new Color(.77f,.88f,.95f,.2f),new Color(.52f,.7f,.81f),-68,Array.Empty<BiomeMistLayer>(),world.ToArray());
            var biomeSettings=new SerializedObject(biome);biomeSettings.FindProperty("_combineStaticGeometry").boolValue=true;biomeSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(biome); EditorUtility.SetDirty(module);
            var flowPath="Assets/_Game/Levels/Resources/FlowChallenges/Module002_FlowChallenge.asset";
            var flow=AssetDatabase.LoadAssetAtPath<FlowChallengeProfile>(flowPath);
            if(flow==null){Directory.CreateDirectory(Path.GetDirectoryName(flowPath));flow=ScriptableObject.CreateInstance<FlowChallengeProfile>();AssetDatabase.CreateAsset(flow,flowPath);}
            flow.ModuleId=module.StableModuleId;
            flow.Shards=new[]{new FlowChallengeProfile.Shard{StableId="shard.m02.inner-ridge",Position=new Vector3(8,5.9f,41)},new FlowChallengeProfile.Shard{StableId="shard.m02.lift-cut",Position=new Vector3(12.2f,9.2f,63.5f)}};
            EditorUtility.SetDirty(flow);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("Mountain 0.9.4 authored: "+world.Count+" world placements, "+module.Blocks.Count+" landings, two optional shard lines.");
        }
        private static void ConfigureRoute(ModuleDefinition m)
        {
            ModuleBlockDefinition B(string id,float x,float y,float z,float w=2.9f,float d=2.9f,bool mastery=false)=>new ModuleBlockDefinition(id,new Vector3(x,y,z),new Vector3(w,.6f,d),mastery?ModuleMaterialRole.Precision:ModuleMaterialRole.Normal);
            var blocks=new[]{
                B("m02.start",0,0,0,6,5), B("m02.rhythm.01",0,.6f,4.5f),B("m02.rhythm.02",0,1.2f,8.5f,4,3),
                B("m02.after-moving.01",3,1.8f,17.5f),B("m02.after-moving.02",4,2.4f,21.5f),B("m02.restore.platform",4,3,26,6,4.5f),
                B("m02.safe.01",1,3.6f,30.5f),B("m02.safe.02",-2,4.2f,34.5f),B("m02.safe.03",-5,4.8f,38.5f),B("m02.safe.04",-6,5.4f,42.5f),
                B("m02.garden.exit",-4,6,46.5f),B("m02.ridge.01",-1,6.4f,50.5f),B("m02.ridge.02",3,6.8f,54.5f),B("m02.rejoin",6,7.2f,58.5f,5,4),
                B("m02.lift.exit",9,8.8f,69),B("m02.ascent.01",12,9.4f,73),B("m02.ascent.02",15,10,77),B("m02.ascent.03",15,10.6f,81),B("m02.ascent.04",13,11.2f,85),
                B("m02.final-runup",10,11.8f,89,3.5f,3),B("m02.patch.base",8,12.4f,93,6,5),
                B("m02.risky.01",7,3.7f,31,1.8f,2,true),B("m02.risky.02",8,4.3f,36,1.8f,2,true),B("m02.risky.03",8,4.9f,41,1.8f,2,true),B("m02.risky.04",7,5.5f,46,1.8f,2,true),B("m02.risky.05",6,6.2f,51,1.8f,2,true),B("m02.risky.06",6,6.7f,54.5f,1.8f,2,true),
                B("m02.mastery.lift-cut",12.2f,8.2f,63.5f,1.9f,2,true)};
            var restore=new[]{new ModuleRestorePointDefinition("restore.module-002.branch",0,new Vector3(4,3,26),new Vector3(0,.35f,0),supportBlockStableId:"m02.restore.platform"),new ModuleRestorePointDefinition("restore.module-002.ridge",1,new Vector3(6,7.2f,58.5f),new Vector3(0,.35f,0),supportBlockStableId:"m02.rejoin")};
            var moving=new[]{new ModuleMovingBlockDefinition("moving.m02.first",new Vector3(-2,1.3f,13),new Vector3(4,.6f,3),new[]{new Vector3(-2,1.3f,13),new Vector3(2.5f,1.3f,13)},1.8f,.65f),new ModuleMovingBlockDefinition("moving.m02.final",new Vector3(6,7.4f,62.5f),new Vector3(4,.6f,3.5f),new[]{new Vector3(6,7.4f,62.5f),new Vector3(9,8.5f,65.5f)},2,.65f)};
            var cuts=new[]{new ModuleShortcutDefinition("shortcut.module-002.risky-inner","Inner rock spine",new Vector3(7,3.7f,31),new Vector3(6,7.2f,58.5f),5,new[]{ModuleMechanic.PrecisionBlock}),new ModuleShortcutDefinition("shortcut.module-002.lift-cut","Windward lift cut",new Vector3(12.2f,8.2f,63.5f),new Vector3(9,8.8f,69),3,new[]{ModuleMechanic.PrecisionBlock})};
            m.Configure(m.StableModuleId,"Campaign 02 - Moving Parts",m.InternalName,m.ProjectId,m.WorldId,3,m.SceneName,m.Difficulty,new ModulePose(new Vector3(0,.35f,-.8f),Vector3.zero),new ModulePatchBlockDefinition("patch.module-002",new Vector3(8,12.7f,93),new Vector3(2.2f,2.2f,2.2f),supportBlockStableId:"m02.patch.base"),restore,m.ExpectedCleanTime,110,m.MechanicsUsed.ToArray(),cuts,m.EnvironmentProfile,m.VisualProfile,"Repair a broken passage through the high mountains.","Cliffside Arrival / Broken Mountain Pass / Waterfall Garden / High Ridge / Summit Patch. Standard route needs no advanced movement. V2 ranks provisional pending physical S23 timing.",blocks,moving,null,null,null,null,null);
            m.ConfigurePlayability("start.module-002.cliffside","m02.start",-14,true);
        }
        private static void ConfigureEnvironment(ModuleDefinition m)
        {
            var path="Assets/_Game/Visuals/Resources/ModuleEnvironment_Mountain.asset";
            var env=AssetDatabase.LoadAssetAtPath<ModuleEnvironmentProfile>(path);
            if(env==null){env=Object.Instantiate(m.EnvironmentProfile);env.name="ModuleEnvironment_Mountain";AssetDatabase.CreateAsset(env,path);}
            var skyPath=Root+"/Materials/Mountain_CloudDepth.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);
            if(sky==null){sky=new Material(AssetDatabase.LoadAssetAtPath<Material>(Phase074WorldSystemRecovery.AncientAbyssSkyPath));AssetDatabase.CreateAsset(sky,skyPath);}
            sky.SetFloat("_Exposure",.57f);sky.SetFloat("_Rotation",32);sky.SetColor("_Tint",new Color(.9f,.97f,1));EditorUtility.SetDirty(sky);
            var s=new SerializedObject(env);s.FindProperty("_skyboxMaterial").objectReferenceValue=sky;s.FindProperty("_overrideBiomeFog").boolValue=true;
            s.FindProperty("_fogDensity").floatValue=.003f;s.FindProperty("_fogColor").colorValue=new Color(.57f,.74f,.84f);
            s.FindProperty("_sunIntensity").floatValue=1.35f;s.FindProperty("_ambientLight").colorValue=new Color(.68f,.76f,.82f);s.FindProperty("_sunLight").colorValue=new Color(1,.94f,.82f);
            s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(env);s=new SerializedObject(m);s.FindProperty("_environmentProfile").objectReferenceValue=env;s.ApplyModifiedPropertiesWithoutUndo();
        }
        private static GameObject Cliffside()
        {
            var b=new Sculpt();b.Rock(new Vector3(-5,-10,-2),new Vector3(6,9,7),19,false);b.Rock(new Vector3(4,-14,-4),new Vector3(5,13,5),48,false);
            b.Rock(new Vector3(-5.8f,-.55f,-1),new Vector3(2,.6f,2.7f),12,false,false);b.Pine(new Vector3(-4,0,-1.5f),5);b.Pine(new Vector3(4,-1,-3),3);
            b.Box("Stone",new Vector3(0,-.35f,-3.2f),new Vector3(9,.6f,1));
            foreach(var x in new[]{-3.9f,3.9f}){b.Box("Stone",new Vector3(x,1,-2.9f),new Vector3(.45f,2.3f,.6f));b.Box("Gold",new Vector3(x,2.2f,-2.9f),new Vector3(.7f,.16f,.8f));}
            return b.Save("CliffsideArrival",true);
        }
        private static GameObject Pass()
        {
            var b=new Sculpt();b.Rock(new Vector3(-5,-8,-1),new Vector3(4,7,4),59,false);b.Rock(new Vector3(6,-12,7),new Vector3(4,11,4),29,false);
            foreach(var x in new[]{-2.7f,2.7f}){b.Box("Stone",new Vector3(x,.15f,-.6f),new Vector3(.5f,.7f,2.8f));b.Box("Gold",new Vector3(x,.55f,-.6f),new Vector3(.55f,.12f,2.8f));
                foreach(var z in new[]{-1.7f,.5f})b.Box("Stone",new Vector3(x,.75f,z),new Vector3(.62f,1,.55f));b.Box("Metal",new Vector3(x,-2,-.3f),new Vector3(.3f,3,.5f));}
            b.Pine(new Vector3(-5,-1.2f,-1),4);
            return b.Save("BrokenMountainPass",true);
        }
        private static GameObject Garden()
        {
            var b=new Sculpt();b.Rock(new Vector3(-5,-10,-2),new Vector3(4.5f,9,5),39,false);b.Rock(new Vector3(5,-12,0),new Vector3(3.5f,11,5),63,false);
            b.Rock(new Vector3(-5.8f,-.5f,-.4f),new Vector3(2.3f,.7f,3),48,false,false);b.Pine(new Vector3(-4,.2f,-1.5f),4.5f);b.Pine(new Vector3(5,-1,-1),3.7f);

            foreach(var x in new[]{-2f,2f}){b.Box("Stone",new Vector3(x,1.8f,-2.7f),new Vector3(.35f,4,.4f));b.Box("Gold",new Vector3(x,3.7f,-2.7f),new Vector3(.5f,.18f,.6f));}
            b.Box("Stone",new Vector3(0,3.8f,-2.7f),new Vector3(5,.35f,1.8f));b.Box("Metal",new Vector3(0,4.08f,-2.7f),new Vector3(5.5f,.2f,2.2f));
            // A planted side pavilion and rough stream ledge establish the calmer garden place.
            b.Rock(new Vector3(-5.8f,-.65f,1.2f),new Vector3(2.4f,.8f,2),852,false,false);
            foreach(var x in new[]{-6.7f,-4.2f})foreach(var z in new[]{.4f,2.4f})b.Box("Bark",new Vector3(x,1.3f,z),new Vector3(.18f,2.7f,.18f));
            b.Box("Stone",new Vector3(-5.4f,2.8f,1.4f),new Vector3(3.5f,.25f,3));
            b.Box("Metal",new Vector3(-5.4f,3,1.4f),new Vector3(3.8f,.15f,3.3f));
            b.Box("Gold",new Vector3(-5.4f,3.16f,1.4f),new Vector3(2.7f,.15f,2.2f));
            foreach(var p in new[]{new Vector3(-5.5f,.1f,-2),new Vector3(-5.7f,.1f,1),new Vector3(4.9f,-.3f,-1)}){b.Rock(p,new Vector3(.6f,.38f,.6f),561,false,false);b.Pine(p,.8f);}
            return b.Save("WaterfallGarden",true);
        }
        private static GameObject Lift()
        {
            var b=new Sculpt();b.Rock(new Vector3(-4,-12,-1),new Vector3(3.5f,11,4),93,false);b.Rock(new Vector3(8,-14,11),new Vector3(4,13,5),102,false);
            foreach(var x in new[]{-3f,3f}){b.Box("Metal",new Vector3(x,2,-.8f),new Vector3(.24f,4.5f,.24f));b.Box("Gold",new Vector3(x,4.3f,-.8f),new Vector3(.42f,.25f,.42f));}
            b.Box("Metal",new Vector3(0,4.1f,-.8f),new Vector3(6.5f,.25f,.25f));b.Pine(new Vector3(-4,-1,-1.5f),2.8f);
            return b.Save("HighRidgeLift",true);
        }
        private static GameObject Summit()
        {
            var b=new Sculpt();b.Rock(new Vector3(0,-14,1),new Vector3(5,13,6),127,false);b.Rock(new Vector3(-4,-6,4),new Vector3(2.5f,6,3),76,false);
            b.Box("Stone",new Vector3(0,-.35f,3.5f),new Vector3(10,.6f,1.5f));
            foreach(var x in new[]{-4f,4f}){b.Box("Stone",new Vector3(x,2,2.7f),new Vector3(.65f,4.6f,.7f));b.Box("Gold",new Vector3(x,4.4f,2.7f),new Vector3(.9f,.15f,1));}
            b.Box("Stone",new Vector3(0,4.6f,2.7f),new Vector3(9.7f,.5f,2.5f));b.Box("Metal",new Vector3(0,4.95f,2.7f),new Vector3(10.5f,.22f,3));
            b.Box("Stone",new Vector3(0,5.15f,2.7f),new Vector3(8,.3f,2.3f));b.Box("Gold",new Vector3(0,5.4f,2.7f),new Vector3(7,.16f,1.8f));
            b.Pine(new Vector3(-4,0,3.2f),3);return b.Save("SummitStoneCrown",true);
        }
        private static GameObject Mountain(string name,int seed,bool snow)
        {
            var b=new Sculpt();b.Peak(Vector3.zero,new Vector3(26,30,27),seed,snow);
            b.Peak(new Vector3(13,-9,7),new Vector3(20,23,22),seed+13,snow);
            b.Peak(new Vector3(-14,-13,-3),new Vector3(20,21,24),seed+29,snow);
            return b.Save(name,false);
        }
        // Rebuild only the rejected landmark. Never reauthor the accepted mountain route.
        public static void RebuildWaterfall()
        {
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/Materials"}))
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));Materials[mat.name]=mat;
            }
            Materials["Water"].SetColor("_BaseColor",new Color(.30f,.62f,.67f));
            Materials["Foam"].SetColor("_BaseColor",new Color(.69f,.80f,.79f));
            foreach(var name in new[]{"Water","Foam"}){Materials[name].SetFloat("_FlowSpeed",1.4f);EditorUtility.SetDirty(Materials[name]);}
            SplitPeak();
            var biome=Resources.Load<ModuleDefinition>("Modules/Module_002_MovingParts").EnvironmentBiomeProfile;
            var so=new SerializedObject(biome);var objects=so.FindProperty("_worldObjects");
            for(int i=0;i<objects.arraySize;i++)
            {
                var item=objects.GetArrayElementAtIndex(i);
                if(item.FindPropertyRelative("_stableId").stringValue=="biome.mountain-sky.landmark.split-peak")
                {
                    item.FindPropertyRelative("_eulerAngles").vector3Value=new Vector3(0,-55,0);
                    item.FindPropertyRelative("_position").vector3Value=new Vector3(-82,-15,64);
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(biome);AssetDatabase.SaveAssets();
        }
        private static GameObject SplitPeak()
        {
            var b=new Sculpt();
            b.Peak(new Vector3(-13,4,8),new Vector3(22,42,25),317,true);
            b.Peak(new Vector3(17,-2,12),new Vector3(22,37,25),438,true);
            b.Rock(new Vector3(1,-25,6),new Vector3(26,26,22),579,false);
            // A snow-fed cleft, two eroded shelves and a widening lower plunge.
            // Ledges sit under each basin; water crosses their lips before falling.
            b.Rock(new Vector3(-3,22,-16),new Vector3(4.5f,5,7),613,false,false,"Stone");
            b.Rock(new Vector3(3,4,-28),new Vector3(5,4,6),741,false,false,"Stone");
            b.Rock(new Vector3(9,-16.7f,-38),new Vector3(5.3f,4,6),813,false,false,"Stone");
            b.Ribbon("Water",new[]{new Vector3(-5,29,-6),new Vector3(-3,28,-13),new Vector3(-3,27.4f,-22)},5.2f);
            b.Ribbon("Water",new[]{new Vector3(-3,27.4f,-22),new Vector3(-2,23,-23),new Vector3(0,10,-26),new Vector3(3,8.5f,-29)},4.6f);
            b.Ribbon("Water",new[]{new Vector3(3,8.5f,-29),new Vector3(5,8.2f,-34)},7.6f);
            b.Ribbon("Water",new[]{new Vector3(5,8.2f,-34),new Vector3(6,3,-35),new Vector3(8,-11,-38),new Vector3(9,-12.6f,-40)},5.8f);
            b.Ribbon("Water",new[]{new Vector3(9,-12.6f,-40),new Vector3(11,-13,-44)},8.4f);
            for(int i=0;i<3;i++)
            {
                float x=8+i*2.6f;
                b.Ribbon("Water",new[]{new Vector3(x,-13,-44),new Vector3(x+1,-23,-45),new Vector3(x+3,-39,-46),new Vector3(x+4,-54,-48)},2.3f-i*.25f);
                b.Ribbon("Foam",new[]{new Vector3(-4+i*1.4f,26,-22.12f),new Vector3(-3+i*1.4f,22,-23.12f),new Vector3(-1+i*1.4f,11,-26.12f)},.22f+i*.09f);
                b.Ribbon("Foam",new[]{new Vector3(3+i*1.5f,7,-34.12f),new Vector3(4+i*1.5f,2,-35.12f),new Vector3(6+i*1.5f,-10,-38.12f)},.28f);
            }
            b.Pool("Water",new Vector3(-3,27.45f,-16),new Vector2(3.6f,5.4f));
            b.Pool("Water",new Vector3(3,8.55f,-30),new Vector2(4.3f,3.6f));
            b.Pool("Water",new Vector3(9,-12.5f,-40),new Vector2(4.6f,3.4f));
            b.Pool("Foam",new Vector3(1,8.58f,-28.5f),new Vector2(1.8f,1.1f));
            b.Pool("Foam",new Vector3(8,-12.47f,-39),new Vector2(2,1.2f));
            // Broken foam fans belong to the impact pools, not floating mist cards.
            b.Ribbon("Foam",new[]{new Vector3(3,8.57f,-29),new Vector3(4,8.4f,-31)},5.6f);
            b.Ribbon("Foam",new[]{new Vector3(9,-12.5f,-40),new Vector3(10,-12.7f,-42)},6.8f);
            b.Rock(new Vector3(-10,25,-14),new Vector3(3,5,7),417,true,false);
            b.Rock(new Vector3(4,24,-12),new Vector3(3,5,7),518,true,false);
            return b.Save("SplitPeakWaterfall",false);
        }
        private static void CreateStoneTexture()
        {
            const int n=256;var t=new Texture2D(n,n,TextureFormat.RGB24,false);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float noise=Mathf.PerlinNoise(x*.09f,y*.18f)*.1f+Mathf.PerlinNoise(x*.028f,y*.19f)*.14f;
                float vein=Mathf.Pow(.5f+.5f*Mathf.Sin(y*.28f+Mathf.PerlinNoise(x*.021f,y*.016f)*8),12)*.075f;
                float v=.77f+noise-vein;t.SetPixel(x,y,new Color(v,v,v));
            }
            t.Apply();var path=Root+"/Materials/MineralGrain.png";File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.SaveAndReimport();
        }
        private static void Mat(string name,Color color)
        {
            var path=Root+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
            if(name=="Water"||name=="Foam"){mat.shader=Shader.Find("RYDERS ROAD/Mountain Waterfall");mat.SetFloat("_FlowSpeed",2);mat.SetFloat("_StreakScale",4);}
            if(name=="Rock"||name=="Strata"||name=="Stone")mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Materials/MineralGrain.png"));
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",name=="Water"?.45f:.05f);Materials[name]=mat;EditorUtility.SetDirty(mat);
        }
        private sealed class Sculpt
        {
            private readonly Dictionary<string,List<Vector3>> vertices=new Dictionary<string,List<Vector3>>();
            private void Tri(string mat,Vector3 a,Vector3 b,Vector3 c){if(!vertices.ContainsKey(mat))vertices[mat]=new List<Vector3>();vertices[mat].AddRange(new[]{a,b,c});}
            private void Quad(string mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(mat,a,c,b);Tri(mat,a,d,c);}
            public void Box(string mat,Vector3 p,Vector3 s)
            {
                var v=new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,-1,1),new Vector3(-1,-1,1),new Vector3(-1,1,-1),new Vector3(1,1,-1),new Vector3(1,1,1),new Vector3(-1,1,1)}.Select(x=>p+Vector3.Scale(x,s)*.5f).ToArray();
                Quad(mat,v[0],v[1],v[5],v[4]);Quad(mat,v[1],v[2],v[6],v[5]);Quad(mat,v[2],v[3],v[7],v[6]);Quad(mat,v[3],v[0],v[4],v[7]);Quad(mat,v[4],v[5],v[6],v[7]);Quad(mat,v[3],v[2],v[1],v[0]);
            }
            public void Rock(Vector3 p,Vector3 radius,int seed,bool snow,bool peak=true,string cap=null)
            {
                var random=new System.Random(seed);const int sides=16,rings=12;var v=new Vector3[rings,sides];
                var rim=new float[sides];for(int i=0;i<sides;i++)rim[i]=.78f+(float)random.NextDouble()*.32f;
                for(int r=0;r<rings;r++)for(int i=0;i<sides;i++)
                {
                    float t=r/(float)(rings-1),angle=(i/(float)sides*360+3*Mathf.Sin(r*1.7f))*Mathf.Deg2Rad;
                    // Faulted ledges and an asymmetric serrated crest replace rotational cones.
                    float width=peak?Mathf.Lerp(1.1f,.52f,t):Mathf.Lerp(.64f,1,t);
                    width*=r%3==1?.89f:r%3==2?1.035f:1;
                    float y=t*2-1;
                    if(peak)y-=(.08f+.3f*(.5f+.5f*Mathf.Sin(i*2.7f+seed)))*t*t;
                    if(r!=rings-1&&r!=0)y+=((float)random.NextDouble()-.5f)*.028f;
                    var local=new Vector3(Mathf.Cos(angle)*width*rim[i],y,Mathf.Sin(angle)*width*rim[i]);
                    if(peak)local.x+=t*.24f;v[r,i]=p+Vector3.Scale(local,radius);
                }
                for(int r=0;r<rings-1;r++)for(int i=0;i<sides;i++)
                {
                    int j=(i+1)%sides;string mat=snow&&r>=9+(i%3==0?1:0)?"Snow":r%5==2?"Strata":"Rock";
                    Tri(mat,v[r,i],v[r+1,i],v[r+1,j]);Tri(mat,v[r,i],v[r+1,j],v[r,j]);
                }
                var top=p+Vector3.up*radius.y*(peak?.82f:1);
                for(int i=0;i<sides;i++){int j=(i+1)%sides;Tri(cap??(snow?"Snow":peak?"Rock":"Moss"),top,v[rings-1,j],v[rings-1,i]);Tri("Rock",p-Vector3.up*radius.y,v[0,i],v[0,j]);}
            }
            public void Peak(Vector3 p,Vector3 size,int seed,bool snow)
            {
                const int n=24;var v=new Vector3[n+1,n+1];
                for(int x=0;x<=n;x++)for(int z=0;z<=n;z++)
                {
                    float u=x/(float)n*2-1,w=z/(float)n*2-1;
                    float edge=Mathf.Max(0,1-Mathf.Max(Mathf.Abs(u),Mathf.Abs(w)));
                    float ridge=Mathf.Max(0,1-Mathf.Abs(u+.18f*Mathf.Sin(w*3+seed)));
                    float h=Mathf.Pow(edge,.5f)*Mathf.Pow(ridge,1.6f)*(1.1f+.25f*Mathf.Sin(w*4+seed));
                    h+=Mathf.PerlinNoise(u*9+seed,w*4+seed)*edge*.19f;
                    v[x,z]=p+Vector3.Scale(new Vector3(u,h*1.8f-1,w),size);
                }
                for(int x=0;x<n;x++)for(int z=0;z<n;z++)
                {
                    var a=v[x,z];var b=v[x,z+1];var c=v[x+1,z+1];var d=v[x+1,z];
                    string mat=snow&&(a.y-p.y)>size.y*.24f?"Snow":(x+z)%7==0?"Strata":"Rock";
                    Tri(mat,a,b,c);Tri(mat,a,c,d);
                }
            }
            public void Pine(Vector3 p,float h)
            {
                float ground=float.NegativeInfinity;
                foreach(var entry in vertices.Where(e=>e.Key=="Rock"||e.Key=="Strata"||e.Key=="Moss"||e.Key=="Snow"))
                for(int i=0;i<entry.Value.Count;i+=3)
                {
                    var a=entry.Value[i];var b=entry.Value[i+1];var c=entry.Value[i+2];
                    float den=(b.z-c.z)*(a.x-c.x)+(c.x-b.x)*(a.z-c.z);if(Mathf.Abs(den)<.0001f)continue;
                    float u=((b.z-c.z)*(p.x-c.x)+(c.x-b.x)*(p.z-c.z))/den;
                    float v=((c.z-a.z)*(p.x-c.x)+(a.x-c.x)*(p.z-c.z))/den;
                    if(u>=0&&v>=0&&u+v<=1)ground=Mathf.Max(ground,u*a.y+v*b.y+(1-u-v)*c.y);
                }
                if(!float.IsNegativeInfinity(ground))p.y=ground;
                Box("Bark",p+Vector3.up*h*.45f,new Vector3(.17f,h*.9f,.17f));
                for(int level=0;level<6;level++)for(int branch=0;branch<5;branch++)
                {
                    float a=(branch*72+level*39)*Mathf.Deg2Rad,spread=h*(.23f-level*.029f);
                    var center=p+new Vector3(Mathf.Cos(a)*spread*.5f,h*(.28f+level*.115f),Mathf.Sin(a)*spread*.5f);
                    for(int i=0;i<7;i++)
                    {
                        float f=i*Mathf.PI*2/7,g=(i+1)*Mathf.PI*2/7;
                        var v=center+new Vector3(Mathf.Cos(f)*spread*.7f,0,Mathf.Sin(f)*spread*.7f);
                        var w=center+new Vector3(Mathf.Cos(g)*spread*.7f,0,Mathf.Sin(g)*spread*.7f);
                        Tri((branch+level)%3==0?"LeafSun":"Leaf",v,center+Vector3.up*h*.18f,w);
                        Tri("Leaf",v,w,center-Vector3.up*h*.05f);
                    }
                }
            }
            public void Pool(string mat,Vector3 center,Vector2 radius)
            {
                for(int i=0;i<16;i++)
                {
                    float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;
                    var p=center+new Vector3(Mathf.Cos(a)*radius.x,0,Mathf.Sin(a)*radius.y);
                    var q=center+new Vector3(Mathf.Cos(b)*radius.x,0,Mathf.Sin(b)*radius.y);
                    Tri(mat,center,q,p);
                }
            }
            public void Ribbon(string mat,Vector3[] path,float width)
            {
                for(int i=0;i<path.Length-1;i++){var a=path[i];var b=path[i+1];var x=Vector3.right*width*.5f;Quad(mat,a-x,a+x,b+x,b-x);Quad(mat,a+x,a-x,b-x,b+x);}
            }
            public GameObject Save(string name,bool collide)
            {
                var root=new GameObject(name);
                foreach(var entry in vertices)
                {
                    var mesh=new Mesh{name=name+"_"+entry.Key,indexFormat=IndexFormat.UInt32};mesh.SetVertices(entry.Value);
                    var uv=new List<Vector2>();
                    for(int i=0;i<entry.Value.Count;i+=3)
                    {
                        var n=Vector3.Cross(entry.Value[i+1]-entry.Value[i],entry.Value[i+2]-entry.Value[i]).normalized;
                        for(int j=0;j<3;j++){var v=entry.Value[i+j];uv.Add(Mathf.Abs(n.y)>.65f?new Vector2(v.x,v.z)*.22f:Mathf.Abs(n.z)>Mathf.Abs(n.x)?new Vector2(v.x*.22f,v.y*.35f):new Vector2(v.z*.22f,v.y*.35f));}
                    }
                    mesh.SetUVs(0,uv);mesh.SetTriangles(Enumerable.Range(0,entry.Value.Count).ToArray(),0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                    var path=Root+"/Meshes/"+mesh.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old==null)AssetDatabase.CreateAsset(mesh,path);else{EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;EditorUtility.SetDirty(old);}
                    var obj=new GameObject(entry.Key,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(root.transform,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=Materials[entry.Key];
                    if(collide&&!entry.Key.StartsWith("Leaf")){obj.AddComponent<MeshCollider>().sharedMesh=mesh;obj.AddComponent<AuthoredSurface>().SetSourceMesh(mesh);}
                }
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+name+".prefab");Object.DestroyImmediate(root);return prefab;
            }
        }
    }
}
