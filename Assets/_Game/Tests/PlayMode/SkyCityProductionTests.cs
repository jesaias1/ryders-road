using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Avoidance.Tests.PlayMode
{
    public sealed class SkyCityProductionTests
    {
        [UnityTest]
        public IEnumerator AllIntroJumpsLandWithUnmodifiedMotorAndArchitectureHasTrueSupport()
        {
            yield return IntroJumps(false);
        }
        [UnityTest] public IEnumerator SharedTrialIntroJumpsUseActualSkyCityGeometry()
        {
            yield return IntroJumps(true);
        }
        private IEnumerator IntroJumps(bool candidate)
        {
            if(candidate) CampaignFlowTrial.Launch("module.001.first-steps",CampaignTrialMode.Foundation);
            else ModuleSelectionState.Select("module.001.first-steps");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.4f);
            var motor=Object.FindAnyObjectByType<ParkourMotor>();
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            Physics.SyncTransforms();
            foreach(var block in module.Blocks)
            {
                foreach(var x in new[]{-.35f,0,.35f})
                foreach(var z in new[]{-.35f,0,.35f})
                {
                    var top=block.Pose.Position+new Vector3(x*block.Size.x,block.Size.y*.5f,z*block.Size.z);
                    Assert.That(Physics.Raycast(top+Vector3.up*.15f,Vector3.down,out var hit,.3f,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore),Is.True,block.StableId);
                    Assert.That(hit.point.y,Is.EqualTo(top.y).Within(.035f),block.StableId+" footing height");
                }
            }
            foreach(var surface in Object.FindObjectsByType<Avoidance.Gameplay.Worlds.AuthoredSurface>(FindObjectsSortMode.None))
                Assert.That(surface.HasMatchingCollision,Is.True,surface.CollisionDetails);
            for(int i=0;i<module.Blocks.Count-1;i++)
            {
                var from=module.Blocks[i];var to=module.Blocks[i+1];
                var direction=to.Pose.Position-from.Pose.Position;direction.y=0;direction.Normalize();
                motor.ResetMotion(from.Pose.Position+Vector3.up*(from.Size.y*.5f+.04f)-direction*.5f,Quaternion.LookRotation(direction),0);
                Physics.SyncTransforms();
                var input=new RouteInput { FlowSteeringEnabled=false };
                float edge=Mathf.Min(from.Size.x*.5f/Mathf.Max(.001f,Mathf.Abs(direction.x)),from.Size.z*.5f/Mathf.Max(.001f,Mathf.Abs(direction.z)));
                for(int frame=0;frame<150&&Vector3.Dot(motor.transform.position-from.Pose.Position,direction)<edge-.4f;frame++)motor.Simulate(input,1f/60);
                input.JumpPressed=true;motor.Simulate(input,1f/60);input.JumpPressed=false;
                bool airborne=false,landed=false;
                for(int frame=0;frame<100;frame++)
                {
                    motor.Simulate(input,1f/60);
                    airborne|=!motor.IsGrounded;
                    if(airborne&&motor.IsGrounded){landed=true;break;}
                }
                Assert.That(landed,Is.True,from.StableId+" -> "+to.StableId);
                Directory.CreateDirectory("Logs/Phase093VisualQA");
                File.AppendAllText("Logs/Phase093VisualQA/jump-trace.txt",$"{from.StableId}->{to.StableId}: pos={motor.transform.position} ground={motor.GroundTransform?.name} takeoff={motor.LastTakeoffHorizontalSpeed} vy={motor.LastTakeoffVerticalVelocity} jumpCount={motor.JumpCount} apex={motor.LastJumpApexHeight} distance={motor.LastJumpHorizontalDistance}\n");
                var delta=motor.transform.position-to.Pose.Position;
                Assert.That(Mathf.Abs(delta.x),Is.LessThan(to.Size.x*.5f+.25f),to.StableId+" x");
                Assert.That(Mathf.Abs(delta.z),Is.LessThan(to.Size.z*.5f+.25f),to.StableId+" z");
            }
        }
        private sealed class RouteInput : Avoidance.Input.IPlayerInputSource, Avoidance.Input.IFlowSteeringInputSource
        {
            public bool FlowSteeringEnabled {get;set;}
            public Avoidance.Input.AutoCameraProfileKind AutoCameraProfile=>Avoidance.Input.AutoCameraProfileKind.Direct;
            public Vector2 Move=>new Vector2(0,.85f);
            public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed {get;set;}
            public void ResetState(){JumpPressed=false;}
        }
        [UnityTest]
        public IEnumerator CaptureFiveSkyCityGameplayViews()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            yield return new WaitForSecondsRealtime(.4f);
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x => x.name == "CAMPAIGN Button").onClick.Invoke();
            var card = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(x => x.name == "Journey module.001.first-steps");
            card.GetComponentsInChildren<Button>().Single(x => x.interactable).onClick.Invoke();
            var deadline = Time.realtimeSinceStartup + 12;
            while ((!UnitySceneLevelLoader.ActiveRequest.IsTerminal || Object.FindAnyObjectByType<ModuleSceneController>() == null) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSecondsRealtime(.5f);
            var module = Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            Assert.That(module.StableModuleId, Is.EqualTo("module.001.first-steps"));
            Assert.That(Object.FindAnyObjectByType<LoadingPresentation>().GetComponent<CanvasGroup>().blocksRaycasts, Is.False);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled = false;
            var camera = Camera.main;
            var spawnCameraPosition = camera.transform.position;
            foreach (var behavior in camera.GetComponents<MonoBehaviour>()) behavior.enabled = false;
            var folder = "Logs/Phase093VisualQA/" + (module.ContentVersion == 1 ? "before" : "after");
            Directory.CreateDirectory(folder);
            var ids = new[] { "m01.start", "m01.step.03", "m01.restore.approach", "m01.flow.03", "m01.final-runup" };
            var names = new[] { "01-opening", "02-bridge", "03-garden", "04-gateway", "05-patch-approach" };
            for (var i = 0; i < ids.Length; i++)
            {
                var block = module.Blocks.Single(x => x.StableId == ids[i]);
                var position = i == 0 ? spawnCameraPosition : block.Pose.Position + Vector3.up * (block.Size.y * .5f + 1.55f);
                var target = i < 4 ? module.Blocks.SkipWhile(x => x.StableId != ids[i]).Skip(1).First().Pose.Position : module.PatchBlock.Pose.Position;
                var direction = target - position; direction.y = 0;
                camera.transform.position = position; camera.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(9, 0, 0);
                yield return null;
                var render = new RenderTexture(1560, 720, 24);
                var image = new Texture2D(1560, 720, TextureFormat.RGB24, false);
                var previous = RenderTexture.active; var aspect = camera.aspect;
                var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
                try
                {
                    camera.targetTexture = render; camera.aspect = 1560f / 720;
                    foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
                    Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = render;
                    image.ReadPixels(new Rect(0, 0, 1560, 720), 0, 0); image.Apply();
                    File.WriteAllBytes(folder + "/" + names[i] + ".png", image.EncodeToPNG());
                }
                finally
                {
                    foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                    camera.targetTexture = null; camera.aspect = aspect; RenderTexture.active = previous;
                    Object.Destroy(render); Object.Destroy(image);
                }
            }
            File.WriteAllText(folder + "/scene-budget.txt", $"renderers={Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length} colliders={Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length}");
        }
    }
}
