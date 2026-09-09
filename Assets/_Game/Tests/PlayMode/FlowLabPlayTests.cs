using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.UI;
using Avoidance.UI.Touch;
using Avoidance.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Avoidance.Tests.PlayMode
{
    public sealed class FlowLabPlayTests
    {
        private sealed class FlowInput : IPlayerInputSource,IFlowSteeringInputSource,IHeldJumpInputSource
        {
            public Vector2 Move {get;set;}=Vector2.up;
            public Vector2 LookDelta {get;set;}
            public bool JumpPressed {get;set;}
            public bool JumpHeld {get;set;}
            public bool FlowSteeringEnabled=>true;
            public AutoCameraProfileKind AutoCameraProfile=>AutoCameraProfileKind.Direct;
            public void ResetState(){Move=Vector2.zero;JumpPressed=false;}
        }
        [UnityTest] public IEnumerator FrontendTrainingRoomsComparisonRetryAndTouchContract()
        {
            var campaignBefore=ModuleSelectionState.GetCampaignModuleIds();
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            var preference=PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1);
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x=>x.name=="DEVELOPMENT Button").onClick.Invoke();
            Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(x=>x.name=="FLOW LAB  /  MOVEMENT PRACTICE Button").onClick.Invoke();
            var deadline=Time.realtimeSinceStartup+15;
            while(Object.FindAnyObjectByType<FlowLabSceneController>()==null && Time.realtimeSinceStartup<deadline)yield return null;
            yield return new WaitForSecondsRealtime(.3f);
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();Assert.That(lab,Is.Not.Null);
            var player=lab.Player;player.enabled=false;var motor=player.Motor;
            Assert.That(motor.Profile.MovementMastery,Is.True);
            var touch=Object.FindAnyObjectByType<TouchInputCoordinator>();
            Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.True);
            Assert.That(touch.RuntimeProfile.EnableFixedButton,Is.False);
            Assert.That(touch.RuntimeProfile.MovementOnRight,Is.False);
            touch.SetMovement(Vector2.up);Assert.That(touch.TryClaim(TouchControlRole.Look,72),Is.True);
            touch.BeginLookGesture(72,new Vector2(1000,400),1);
            touch.EndLookGesture(72,new Vector2(1000,400),1.06f,800);
            touch.Release(TouchControlRole.Look,72);
            Assert.That(touch.ConsumeJumpPressed(),Is.True);Assert.That(touch.Movement.y,Is.GreaterThan(0));
            player.Input.SetMode(PlayerInputMode.Touch);touch.AddLookDelta(new Vector2(20,10));player.Input.Sample();
            float yaw=player.transform.eulerAngles.y,pitch=player.CameraRig.Pitch;
            player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/60,motor);
            Assert.That(Mathf.DeltaAngle(yaw,player.transform.eulerAngles.y),Is.EqualTo(0).Within(.01f));
            Assert.That(player.CameraRig.Pitch,Is.Not.EqualTo(pitch));
            lab.Retry();Assert.That(motor.Velocity,Is.EqualTo(Vector3.zero));Assert.That(touch.Movement,Is.EqualTo(Vector2.zero));
            for(int room=0;room<6;room++)
            {
                lab.SelectRoom(room);yield return null;
                Assert.That(lab.Room.id,Does.StartWith("training.flow."));
                Assert.That(Physics.Raycast(lab.Room.start+Vector3.up,Vector3.down,3,LayerMask.GetMask("Ground")),Is.True);
                Capture("Logs/MovementMasteryQA/room-"+room+".png");
            }
            lab.Compare();Assert.That(motor.Profile.MovementMastery,Is.False);
            lab.Compare();Assert.That(motor.Profile.MovementMastery,Is.True);
            lab.Compare();Assert.That(motor.Profile.MovementFoundation,Is.True);
            Assert.That(touch.JumpLookEnabled,Is.True);Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.False);
            Assert.That(touch.TryClaim(TouchControlRole.Look,73),Is.True);
            lab.Compare();Assert.That(motor.Profile.CompatibilityVersion,Is.EqualTo(3));
            Assert.That(touch.JumpHeld,Is.False);Assert.That(touch.JumpLookEnabled,Is.False);
            Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.True);
            Assert.That(PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1),Is.EqualTo(preference));
            Assert.That(ModuleSelectionState.GetCampaignModuleIds(),Is.EqualTo(campaignBefore));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator SurfRoomRealEntryContactExitAndRetry() => VerifySurfRoom(false);
        [UnityTest] public IEnumerator FoundationSurfRoomEntryExitAndRetry() => VerifySurfRoom(true);
        private IEnumerator VerifySurfRoom(bool foundation)
        {
            FlowLabSceneController.RequestLaunch();yield return new UnitySceneLevelLoader().LoadAsync("MovementLab");yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();lab.Player.enabled=false;lab.enabled=false;
            if(foundation){lab.Compare();lab.Compare();lab.Compare();}
            lab.SelectRoom(2);yield return null;
            var motor=lab.Player.Motor;var input=new FlowInput();float contact=0;
            for(int i=0;i<600 && !lab.Session.Complete;i++)
            {
                motor.Simulate(input,1f/60);lab.Session.Tick(motor,1f/60);
                if(motor.IsSurfing)contact+=1f/60;
            }
            Debug.Log($"FLOW LAB surf route contact={contact:F3}s position={motor.transform.position} time={lab.Session.Seconds:F3}");
            Assert.That(contact,Is.GreaterThan(.3f));Assert.That(lab.Session.Complete,Is.True);
            float best=lab.Session.BestSeconds;lab.Retry();
            Assert.That(lab.Session.Complete,Is.False);Assert.That(lab.Session.Seconds,Is.Zero);Assert.That(lab.Session.BestSeconds,Is.EqualTo(best));
            Assert.That(motor.transform.position,Is.EqualTo(lab.Room.start));Assert.That(motor.IsSurfing,Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator FoundationHeldLaneCompletesWithoutRepeatedPresses()
        {
            FlowLabSceneController.RequestLaunch();yield return new UnitySceneLevelLoader().LoadAsync("MovementLab");yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();lab.Player.enabled=false;lab.enabled=false;
            lab.Compare();lab.Compare();lab.Compare();lab.SelectRoom(1);yield return null;
            var motor=lab.Player.Motor;var input=new FlowInput();int frame=0;
            bool capture=System.Environment.GetEnvironmentVariable("RYDERS_FOUNDATION_CAPTURE")=="1";
            for(int i=0;i<600 && !lab.Session.Complete;i++)
            {
                input.JumpHeld=motor.transform.position.z>5;
                motor.Simulate(input,1f/60);lab.Session.Tick(motor,1f/60);
                if(capture && i%4==0){lab.RefreshFeedback();Capture("Logs/Foundation140QA/held-"+(frame++).ToString("D4")+".png");}
            }
            Assert.That(lab.Session.Complete,Is.True);Assert.That(motor.JumpCount,Is.GreaterThanOrEqualTo(3));
            Assert.That(input.JumpPressed,Is.False,"No repeated tap intents");
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator AirBhopAndShortFlowHaveReachableMeasuredFinishes()
        {
            FlowLabSceneController.RequestLaunch();yield return new UnitySceneLevelLoader().LoadAsync("MovementLab");yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();lab.Player.enabled=false;lab.enabled=false;
            foreach(int room in new[]{0,1,3})
            {
                lab.SelectRoom(room);yield return null;
                var motor=lab.Player.Motor;var input=new FlowInput();bool armed=true;int jumps=0;
                for(int i=0;i<1200 && !lab.Session.Complete;i++)
                {
                    float z=motor.transform.position.z;
                    input.Move=room==0 && !motor.IsGrounded && motor.VerticalSpeed!=0 ? new Vector2(.65f,1).normalized : Vector2.up;
                    input.JumpPressed=armed && (jumps==0 ? z>5 : room==1
                        ? motor.VerticalSpeed < -3 && motor.transform.position.y<.8f
                        : room==3 && jumps==1 && z>10 && motor.IsGrounded);
                    if(input.JumpPressed)armed=false;
                    int before=motor.JumpCount;motor.Simulate(input,1f/60);
                    if(motor.JumpCount>before){jumps++;armed=true;}
                    lab.Session.Tick(motor,1f/60);
                }
                Debug.Log($"FLOW LAB {lab.Room.id} complete={lab.Session.Complete} time={lab.Session.Seconds:F3} airGain={lab.Session.AirGain:F3} chain={lab.Session.BestChain} position={motor.transform.position}");
                Assert.That(lab.Session.Complete,Is.True,lab.Room.id);
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator LandingViewComparisonPreservesManualControlsAndMotor()
        {
            FlowLabSceneController.RequestLaunch();yield return new UnitySceneLevelLoader().LoadAsync("MovementLab");yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();lab.Player.enabled=false;lab.enabled=false;
            Assert.That(lab.Room.id,Is.EqualTo("training.flow.real-route"));
            lab.SelectRoom("training.flow.landing");yield return null;
            var player=lab.Player;var rig=player.CameraRig;var input=new FlowInput();
            lab.CompareView();Assert.That(lab.LandingViewEnabled,Is.True);
            var velocity=player.Motor.Velocity;var position=player.transform.position;
            for(int i=0;i<30;i++)
            {
                rig.ApplyLook(input,PlayerInputMode.Touch,1f/60,player.Motor);
                rig.UpdatePresentation(8,8,false,false,-8,1f/60);
            }
            Assert.That(rig.CameraPitchTarget,Is.EqualTo(24).Within(.01f));
            Assert.That(player.Motor.Velocity,Is.EqualTo(velocity));Assert.That(player.transform.position,Is.EqualTo(position));
            float before=rig.CameraPitchTarget;input.LookDelta=new Vector2(0,1);
            rig.ApplyLook(input,PlayerInputMode.Touch,1f/60,player.Motor);
            Assert.That(rig.CameraPitchTarget,Is.InRange(before-1,before));
            input.LookDelta=Vector2.zero;
            rig.UpdatePresentation(8,8,false,false,-8,.5f);
            Assert.That(rig.CameraPitchTarget,Is.EqualTo(rig.Pitch).Within(.001f));
            lab.Retry();Assert.That(rig.CameraPitchTarget,Is.EqualTo(16));
            input.LookDelta=new Vector2(15,5);
            rig.ApplyLook(input,PlayerInputMode.Editor,1f/60,player.Motor);
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(0,rig.Yaw)),Is.GreaterThan(0));
            rig.UpdatePresentation(8,8,false,false,-8,1f/60);
            // Editor uses its existing presentation; evaluation framing must be absent.
            Assert.That(rig.CameraPitchTarget-rig.Pitch,Is.LessThan(2f));
            lab.CompareView();Assert.That(rig.Pitch,Is.EqualTo(9));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        [UnityTest] public IEnumerator LandingCourseIsCompletableWithBothMotorsAndViews()
        {
            FlowLabSceneController.RequestLaunch();yield return new UnitySceneLevelLoader().LoadAsync("MovementLab");yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>();lab.Player.enabled=false;lab.enabled=false;
            foreach(bool candidate in new[]{true,false})
            foreach(bool landing in new[]{false,true})
            {
                if(lab.Player.Motor.Profile.MovementMastery!=candidate)lab.Compare();
                if(lab.LandingViewEnabled!=landing)lab.CompareView();
                lab.SelectRoom("training.flow.landing");yield return null;
                lab.Retry();var motor=lab.Player.Motor;var input=new FlowInput();int jumps=0;
                Assert.That(lab.Session.BestSeconds,Is.Zero,"Each motor/view combination needs independent evidence.");
                for(int i=0;i<600 && !lab.Session.Complete;i++)
                {
                    float z=motor.transform.position.z;
                    input.JumpPressed=motor.IsGrounded && (jumps==0 ? z>5.8f : jumps==1 && z>11.8f);
                    int before=motor.JumpCount;motor.Simulate(input,1f/60);
                    if(motor.JumpCount>before)jumps++;
                    lab.Player.CameraRig.ApplyLook(input,PlayerInputMode.Touch,1f/60,motor);
                    lab.Player.CameraRig.UpdatePresentation(motor.HorizontalSpeed,8,motor.IsGrounded,motor.IsSurfing,motor.VerticalSpeed,1f/60);
                    lab.Session.Tick(motor,1f/60);
                    if(i==51 || i==95 || i==114)
                    {
                        lab.RefreshFeedback();
                        Capture($"Logs/Production097QA/landing-{candidate}-{landing}-{i}.png");
                    }
                }
                Assert.That(lab.Session.Complete,Is.True,$"candidate={candidate} landing={landing} position={motor.transform.position}");
                Assert.That(jumps,Is.EqualTo(2));
                Assert.That(lab.Session.BestSeconds,Is.GreaterThan(0));
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator DiagnosticCircuitHasReachableRealLandingsForAllThreeMotors()
        {
            FlowLabSceneController.RequestLaunch(); yield return new UnitySceneLevelLoader().LoadAsync("MovementLab"); yield return null;
            var lab=Object.FindAnyObjectByType<FlowLabSceneController>(); lab.Player.enabled=false; lab.enabled=false;
            lab.SelectRoom("training.flow.real-route"); yield return null;
            var motor=lab.Player.Motor;
            foreach(var resource in new[]{"MovementProfiles/Movement_Default","Training/Movement_Mastery","Training/Movement_RealRoute"})
            {
                motor.SetProfile(Resources.Load<MovementProfile>(resource));
                for(int index=0;index<lab.Room.solids.Length-1;index++)
                {
                    var from=lab.Room.solids[index]; var to=lab.Room.solids[index+1];
                    var direction=to.position-from.position; direction.y=0; direction.Normalize();
                    float edge=Mathf.Min(from.size.x*.5f/Mathf.Max(.001f,Mathf.Abs(direction.x)),
                        from.size.z*.5f/Mathf.Max(.001f,Mathf.Abs(direction.z)));
                    var start=from.position+Vector3.up*(from.size.y*.5f+.05f)+direction*(edge-.7f);
                    motor.ResetMotion(start,Quaternion.LookRotation(direction),0); Physics.SyncTransforms();
                    var input=new FlowInput {Move=Vector2.zero};
                    for(int i=0;i<12;i++) motor.Simulate(input,1f/60);
                    Assert.That(motor.IsGrounded,Is.True,from.id);
                    motor.ApplyLaunch(direction*7.8f,true);
                    // Establish run velocity, then issue one ordinary jump edge.
                    motor.Simulate(input,1f/60);
                    input.Move=Vector2.up; input.JumpPressed=true; motor.Simulate(input,1f/60); input.JumpPressed=false;
                    bool air=false,landed=false;
                    for(int i=0;i<100;i++)
                    {
                        motor.Simulate(input,1f/60); air |= !motor.IsGrounded;
                        if(air && motor.IsGrounded){landed=true;break;}
                    }
                    Assert.That(landed,Is.True,resource+" to "+to.id+" at "+motor.transform.position);
                    Assert.That(motor.GroundTransform.name,Is.EqualTo(to.id),resource+" at "+motor.transform.position);
                }
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        private static void Capture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var camera=Camera.main;
            var render=new RenderTexture(1560,720,24);var image=new Texture2D(1560,720,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;var aspect=camera.aspect;
            var canvases=Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
            try
            {
                camera.targetTexture=render;camera.aspect=1560f/720;
                foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                image.ReadPixels(new Rect(0,0,1560,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            }
            finally
            {
                foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
                camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=previous;Object.Destroy(render);Object.Destroy(image);
            }
        }
    }
}
