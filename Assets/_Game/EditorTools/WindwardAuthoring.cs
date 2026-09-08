using System;
using System.Collections.Generic;
using System.IO;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Ranking;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Worlds;
using UnityEditor;
using UnityEngine;

namespace Avoidance.EditorTools
{
    // Explicit production tool; serialized content is the runtime authority.
    public static class WindwardAuthoring
    {
        public const string Id="module.005.foundry-pulse"; // Reserved identity retained; display concept evolved after Solar Foundry.
        public const string Root="Assets/_Game/Art/Environment/Windward110";
        public const string ModulePath="Assets/_Game/Levels/Resources/Modules/Module_005_Windward.asset";
        [MenuItem("RYDERS BLOCK/Production/Author Windward Observatory")]
        public static void Build()
        {
            Directory.CreateDirectory(Root+"/Meshes"); Directory.CreateDirectory(Root+"/Materials");
            var mats=new Dictionary<string,Material>();
            void Mat(string name,Color color,float gloss=.16f)
            {
                string path=Root+"/Materials/"+name+".mat";
                var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
                m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",gloss);EditorUtility.SetDirty(m);mats[name]=m;
            }
            Mat("Porcelain",new Color(.70f,.76f,.72f));Mat("Ink",new Color(.09f,.16f,.24f));
            Mat("Brass",new Color(.65f,.43f,.21f),.28f);Mat("Sail",new Color(.54f,.64f,.72f));
            var module=AssetDatabase.LoadAssetAtPath<ModuleDefinition>(ModulePath);
            if(module==null){module=ScriptableObject.CreateInstance<ModuleDefinition>();AssetDatabase.CreateAsset(module,ModulePath);}
            var blocks=new List<ModuleBlockDefinition>();
            void B(string id,Vector3 p,float width=3.6f,float depth=3.6f,bool skill=false)
                => blocks.Add(new ModuleBlockDefinition("m05."+id,p,new Vector3(width,.6f,depth),skill?ModuleMaterialRole.Precision:ModuleMaterialRole.Normal));
            // Broad run-up courts alternate with separated landings; no continuous walkway.
            B("arrival",new Vector3(-22,0,20),6,4);
            B("approach.a",new Vector3(-22,.35f,26),4.2f,3.4f);
            B("approach.b",new Vector3(-22,.8f,32),3.8f,3.2f);
            B("approach.c",new Vector3(-22,1.2f,38),4.2f,3.4f);
            B("west.restore",new Vector3(-22,1.6f,44),5,3.6f);
            for(int i=1;i<12;i++)
            {
                float angle=Mathf.PI-i*Mathf.PI/12;
                B("arc."+i.ToString("00"),new Vector3(Mathf.Cos(angle)*22,1.6f+i*.25f,44+Mathf.Sin(angle)*22),3.2f,3.2f);
            }
            B("east.restore",new Vector3(22,4.6f,44),5,3.6f);
            B("ascent.a",new Vector3(27,5,39));B("ascent.b",new Vector3(32,5.4f,34));
            B("ascent.c",new Vector3(37,5.8f,29));B("ascent.d",new Vector3(42,6.2f,24));
            B("lens.restore",new Vector3(47,6.6f,19),5,4);
            B("lens.a",new Vector3(51,7,25),4,3.6f);B("lens.b",new Vector3(54,7.4f,31),4,3.6f);
            B("lens.c",new Vector3(55,7.8f,37),4.2f,3.6f);B("patch.base",new Vector3(55,8.2f,43),6,4);
            for(int i=1;i<6;i++) B("skill.chord."+i,new Vector3(-22+i*44f/6,1.6f+i*.5f,44),3.2f,3.2f,true);
            var restores=new[]{
                new ModuleRestorePointDefinition("restore.module-005.west",0,new Vector3(-22,1.6f,44),new Vector3(0,.35f,0),supportBlockStableId:"m05.west.restore"),
                new ModuleRestorePointDefinition("restore.module-005.east",1,new Vector3(22,4.6f,44),new Vector3(0,.35f,0),new Vector3(0,135,0),supportBlockStableId:"m05.east.restore"),
                new ModuleRestorePointDefinition("restore.module-005.lens",2,new Vector3(47,6.6f,19),new Vector3(0,.35f,0),new Vector3(0,34,0),supportBlockStableId:"m05.lens.restore")};
            module.Configure(Id,"Campaign 05 - Windward Observatory","module_005_windward","project.ryders-block","world.windward-observatory",2,
                ModuleSelectionState.ModuleRunnerSceneName,ModuleDifficulty.Medium,new ModulePose(new Vector3(-22,.35f,19),Vector3.zero),
                new ModulePatchBlockDefinition("patch.module-005.observatory",new Vector3(55,8.5f,43),new Vector3(2.2f,2.2f,2.2f),supportBlockStableId:"m05.patch.base"),
                restores,50,120,new[]{ModuleMechanic.StandardBlock,ModuleMechanic.PrecisionBlock,ModuleMechanic.RestorePoint,ModuleMechanic.PatchBlock,ModuleMechanic.Shortcut},
                new[]{new ModuleShortcutDefinition("shortcut.module-005.chord","Instrument maintenance chord",new Vector3(-22+44f/6,2.1f,44),new Vector3(22,4.6f,44),12,new[]{ModuleMechanic.PrecisionBlock})},
                Resources.Load<ModuleEnvironmentProfile>("ModuleEnvironmentProfile"),Resources.Load<ModuleVisualProfile>("ModuleVisualProfile"),
                "Cross the wind instrument and reconnect the observatory.","Approach jump rhythm / West Anchorage / separated wind arc / diagonal lens ascent / telescope finale. Five-pad chord saves eleven-pad arc; approach speed and late takeoff matter. Physical timing pending.",
                blocks.ToArray(),null,null,null,null,null,null);
            module.ConfigureRankThresholds(new ModuleRankThresholds(100,72,51,RankCalibrationState.Uncalibrated,1,"Provisional; Bronze has no deadline."));
            module.ConfigurePlayability("start.module-005.windward","m05.arrival",-20,true);
            WorldMeshBake Bake()=>new WorldMeshBake(Root,mats);
            var supports=Bake();
            foreach(var b in blocks)
            {
                var p=b.Pose.Position;var size=b.Size;
                supports.Box("Ink",p+Vector3.down*1.1f,new Vector3(size.x*.87f,1.1f,size.z*.87f));
                supports.Beam("Brass",p+new Vector3(-size.x*.3f,-1.6f,0),p+new Vector3(0,-4.2f,0),new Vector2(.22f,.22f));
                supports.Beam("Brass",p+new Vector3(size.x*.3f,-1.6f,0),p+new Vector3(0,-4.2f,0),new Vector2(.22f,.22f));
            }
            var instrument=Bake();
            // The whole wind instrument sits below the playable chord. No cross-course beams.
            instrument.Cylinder("Ink",new Vector3(0,-16,44),6,3,32);
            instrument.Cylinder("Porcelain",new Vector3(0,-8,44),4,5,9);
            for(int i=0;i<32;i++)
            {
                float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;
                var p=new Vector3(Mathf.Cos(a)*23,-4,44+Mathf.Sin(a)*23);
                var q=new Vector3(Mathf.Cos(b)*23,-4,44+Mathf.Sin(b)*23);
                instrument.Beam("Brass",p,q,new Vector2(.45f,.6f));
                if(i%4==0)instrument.Beam("Ink",new Vector3(0,-11,44),p,new Vector2(.6f,.9f));
            }
            var lens=Bake();
            lens.Cylinder("Porcelain",new Vector3(57,-10,46),7,5,30);
            lens.Ring("Brass",new Vector3(57,14,48),8,.6f);
            lens.Ring("Ink",new Vector3(57,14,50),8,.45f);
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4;var p=new Vector3(57+Mathf.Cos(a)*8,14+Mathf.Sin(a)*8,48);
                lens.Beam("Porcelain",p,p+Vector3.forward*2,new Vector2(.8f,.8f));
            }
            // Off-course sail banks give the world its wind-harvesting silhouette.
            var sails=Bake();
            sails.Cylinder("Porcelain",new Vector3(0,-4,0),4,2,45);
            sails.Ring("Brass",new Vector3(0,16,0),13,.45f);
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3;
                var end=new Vector3(Mathf.Cos(a)*12,16+Mathf.Sin(a)*12,0);
                sails.Beam("Ink",new Vector3(0,16,0),end,new Vector2(.28f,.28f));
                sails.Beam("Sail",Vector3.Lerp(new Vector3(0,16,0),end,.42f),end,new Vector2(2.7f,.16f));
            }
            var sailPrefab=sails.Save("WindSail",false);
            var world=new List<BiomeWorldObject>();
            void Place(string id,GameObject prefab,Vector3 p,bool solid,float scale=1)
                =>world.Add(new BiomeWorldObject("biome.windward."+id,prefab,p,Vector3.zero,Vector3.one*scale,true,solid?BiomeDepthBand.NearEnvironment:BiomeDepthBand.FarWorld,.65f,solid));
            Place("route-structure",supports.Save("RouteStructure",true),Vector3.zero,true);
            Place("wind-instrument",instrument.Save("WindInstrument",true),Vector3.zero,true);
            Place("telescope",lens.Save("Telescope",true),Vector3.zero,true);
            Place("west-sail",sailPrefab,new Vector3(-62,-12,65),false,1.5f);
            Place("north-sail",sailPrefab,new Vector3(8,-24,119),false,2.2f);
            Place("east-sail",sailPrefab,new Vector3(98,-18,74),false,1.3f);
            const string biomePath="Assets/_Game/Worlds/Resources/EnvironmentBiomes/Biome_Windward.asset";
            var biome=AssetDatabase.LoadAssetAtPath<EnvironmentBiomeProfile>(biomePath);
            if(biome==null){biome=ScriptableObject.CreateInstance<EnvironmentBiomeProfile>();AssetDatabase.CreateAsset(biome,biomePath);}
            biome.Configure("biome.windward","Windward Observatory",EnvironmentBiomeKind.Windward,new Color(.63f,.7f,.8f),new Color(.7f,.74f,.8f),new Color(.36f,.43f,.6f),-65,Array.Empty<BiomeMistLayer>(),world.ToArray());
            module.ConfigureEnvironmentBiome(biome);
            EditorUtility.SetDirty(biome);EditorUtility.SetDirty(module);AssetDatabase.SaveAssets();
        }
    }
}
