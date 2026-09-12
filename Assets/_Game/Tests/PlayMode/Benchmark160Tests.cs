using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class Benchmark160Tests
    {
        private sealed class Input : IPlayerInputSource,IHeldJumpInputSource
        {
            public Vector2 Move{get;set;}public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed{get;set;}public bool JumpHeld{get;set;}
            public void ResetState(){Move=Vector2.zero;JumpHeld=JumpPressed=false;}
        }
        private static IEnumerator Open(string id)
        {
            ModuleSelectionState.Select(id);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
        }
        [UnityTest] public IEnumerator AuditAllWorldSceneryAndActualFatalLandings()
        {
            var report=new List<string>{"module,solidObjects,colliders,previouslyUnmarkedObjects,previouslyUnmarkedColliders,actualFatalDrops"};
            foreach(string id in ModuleSelectionState.GetCampaignModuleIds().Concat(new[]{ModuleSelectionState.SpiralModuleId}))
            {
                yield return Open(id);
                var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
                var relay=motor.GetComponent<SceneryLandingRecovery>();var input=new Input();
                var objects=module.EnvironmentBiomeProfile?.WorldObjects.Where(w=>w.IsValid&&w.HasPlayableArchitecture).ToArray()??new BiomeWorldObject[0];
                int oldObjects=objects.Count(w=>w.Prefab.GetComponentsInChildren<AuthoredSurface>(true).Any(s=>!s.RestoreOnLanding));
                int oldSurfaces=objects.Sum(w=>w.Prefab.GetComponentsInChildren<AuthoredSurface>(true).Count(s=>!s.RestoreOnLanding));
                var surfaces=Object.FindObjectsByType<AuthoredSurface>().Where(s=>s.GetComponent<Collider>().enabled).ToArray();
                foreach(var surface in surfaces)
                {
                    Assert.That(surface.GeometryKind,Is.EqualTo(WorldGeometryKind.FatalScenery),id+"/"+surface.name);
                    Assert.That(surface.HasMatchingCollision,Is.True);
                }
                int drops=0;
                foreach(var surface in surfaces)
                {
                    if(drops>=5)break;
                    var collider=surface.GetComponent<Collider>();var b=collider.bounds;bool sampled=false;
                    foreach(float x in new[]{.2f,.5f,.8f})foreach(float z in new[]{.2f,.5f,.8f})
                    {
                        if(sampled)continue;
                        var origin=new Vector3(Mathf.Lerp(b.min.x,b.max.x,x),b.max.y+2,Mathf.Lerp(b.min.z,b.max.z,z));
                        if(!collider.Raycast(new Ray(origin,Vector3.down),out var hit,b.size.y+4)||hit.normal.y<.5f)continue;
                        restore.RestoreNow();restore.Tick(false,1);
                        motor.ResetMotion(hit.point+Vector3.up*1.4f,Quaternion.identity,0);Physics.SyncTransforms();
                        input.Move=Vector2.zero;input.JumpHeld=true;int count=restore.RestoreCount;
                        for(int step=0;step<90&&!restore.IsRestorePending;step++)motor.Simulate(input,1f/60);
                        if(!restore.IsRestorePending)continue; // A legitimate route surface may occlude this scenery sample.
                        int jumps=motor.JumpCount;var position=motor.transform.position;
                        for(int step=0;step<5;step++)motor.Simulate(input,1f/60);
                        Assert.That(motor.JumpCount,Is.EqualTo(jumps),"No second jump after fatal contact");
                        Assert.That(motor.transform.position,Is.EqualTo(position),"Fatal contact freezes the pending restore");
                        restore.Tick(false,.1f);Assert.That(restore.RestoreCount,Is.EqualTo(count+1));
                        drops++;sampled=true;
                    }
                }
                if(surfaces.Length>0)Assert.That(drops,Is.GreaterThan(0),id);
                // All ordinary route decks remain legitimate, including side recovery space.
                foreach(var block in module.Blocks)
                {
                    restore.RestoreNow();motor.ResetMotion(block.Pose.Position+Vector3.up*.6f,Quaternion.identity,0);Physics.SyncTransforms();
                    input.ResetState();for(int i=0;i<12;i++)motor.Simulate(input,1f/60);
                    Assert.That(restore.IsRestorePending,Is.False,id+" false fatal "+block.StableId);
                }
                if(surfaces.Length>0)
                {
                    restore.RestoreNow();relay.NotifyContact(surfaces[0].GetComponent<Collider>(),Vector3.right);
                    Assert.That(restore.IsRestorePending,Is.True,"Spawn protection cannot make scenery landable");restore.Tick(false,.1f);
                }
                report.Add($"{id},{objects.Length},{surfaces.Length},{oldObjects},{oldSurfaces},{drops}");
                Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllLines("Logs/Benchmark160QA/scenery-audit.csv",report);
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator CommitmentGapCannotBeWalkedAndRunwaySupportsHeldJump()
        {
            yield return Open("module.005.foundry-pulse");
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var input=new Input { Move=Vector2.up };
            motor.ResetMotion(new Vector3(-24,.35f,8),Quaternion.identity,0);Physics.SyncTransforms();
            for(int i=0;i<150;i++)motor.Simulate(input,1f/60);
            Assert.That(motor.JumpCount,Is.EqualTo(0));
            Assert.That(motor.transform.position.y< -1 || restore.IsRestorePending,Is.True,"Opening must require commitment, not walking");
            restore.RestoreNow();motor.ResetMotion(new Vector3(-24,.35f,6),Quaternion.identity,0);Physics.SyncTransforms();
            input.Move=Vector2.up;input.JumpHeld=true;
            int before=motor.JumpCount;
            for(int i=0;i<80;i++)motor.Simulate(input,1f/120);
            Assert.That(motor.JumpCount-before,Is.GreaterThanOrEqualTo(2),"Runway supports contact-rearmed holding");
            Assert.That(restore.IsRestorePending,Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator CaptureFlowSequenceOverview()
        {
            yield return Open("module.005.foundry-pulse");
            CaptureOverview();
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void CaptureOverview()
        {
            var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;
            var rt=new RenderTexture(1560,900,24);var image=new Texture2D(1560,900,TextureFormat.RGB24,false);
            var active=RenderTexture.active;float aspect=camera.aspect;
            try
            {
                camera.transform.position=new Vector3(60,130,8);camera.transform.LookAt(new Vector3(-40,0,80));
                camera.targetTexture=rt;camera.aspect=1560f/900;camera.Render();RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,1560,900),0,0);image.Apply();
                Directory.CreateDirectory("Logs/Benchmark160QA");File.WriteAllBytes("Logs/Benchmark160QA/overview.png",image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;camera.aspect=aspect;camera.transform.SetPositionAndRotation(position,rotation);
                RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(image);
            }
        }
    }
}
