using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Core.Services;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Worlds;
using Avoidance.Input;
using Avoidance.SaveSystem;
using Avoidance.UI;
using Avoidance.UI.Touch;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Avoidance.Tests.PlayMode
{
    public sealed class Production099Tests
    {
        private const string Foundry="module.004.solar-foundry";
        private ServiceContainer previous, isolated;
        private JsonSaveService save;
        private Store store;
        private sealed class Store : ISaveFileStore
        {
            private string json;
            public int Writes;
            public bool PrimaryExists=>json!=null;
            public bool BackupExists=>false;
            public bool TryReadPrimary(out string value){value=json;return value!=null;}
            public bool TryReadBackup(out string value){value=null;return false;}
            public void WriteAtomic(string value,bool preserveBackup=false){json=value;Writes++;}
            public void DeleteAll(){json=null;}
        }
        private sealed class Idle : IPlayerInputSource
        {
            public Vector2 Move=>Vector2.zero;
            public Vector2 LookDelta=>Vector2.zero;
            public bool JumpPressed=>false;
            public void ResetState(){}
        }
        [SetUp] public void Isolate()
        {
            CampaignFlowTrial.Clear();previous=GameServices.Current;isolated=new ServiceContainer();store=new Store();
            save=new JsonSaveService(store,"trial-test");save.Initialize();isolated.Register<ISaveService>(save);GameServices.Publish(isolated);
        }
        [TearDown] public void Cleanup()
        {
            CampaignFlowTrial.Clear();Time.timeScale=1;
            if(previous!=null)GameServices.Publish(previous);else GameServices.Clear(isolated);
        }
        private static IEnumerator Open()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");
            yield return new WaitForSecondsRealtime(.2f);
            Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
        }
        [UnityTest] public IEnumerator SceneryFallsRecoverButRouteAndSideContactsRemainPlayable()
        {
            ModuleSelectionState.Select(Foundry,true);yield return Open();
            var motor=Object.FindAnyObjectByType<ParkourMotor>();var restore=motor.GetComponent<RestoreController>();
            var relay=motor.GetComponent<SceneryLandingRecovery>();var idle=new Idle();
            var surfaces=Object.FindObjectsByType<AuthoredSurface>().Where(s=>s.RestoreOnLanding).ToArray();
            Assert.That(surfaces.Length,Is.GreaterThan(10));
            foreach(var surface in surfaces)
            {
                Assert.That(surface.HasMatchingCollision,Is.True);
                relay.NotifyContact(surface.GetComponent<Collider>(),Vector3.right);
                Assert.That(restore.IsRestorePending,Is.False,"Wall brushes must not restore");
            }
            // Sample real low ledges, then fall with the actual character controller onto each.
            int recovered=0;
            foreach(float x in new[]{-18f,-12f,-8f,0f,8f,16f,23f})
            foreach(float z in new[]{6f,15f,34f,55f,75f,100f})
            {
                if(recovered>=5)break;
                motor.ResetMotion(new Vector3(0,50,0),Quaternion.identity,0);Physics.SyncTransforms();
                if(!Physics.Raycast(new Vector3(x,1,z),Vector3.down,out var hit,16,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore))continue;
                if(hit.normal.y<.8f || hit.collider.GetComponent<AuthoredSurface>()?.RestoreOnLanding!=true)continue;
                motor.ResetMotion(hit.point+Vector3.up*1.5f,Quaternion.identity,0);Physics.SyncTransforms();
                int count=restore.RestoreCount;
                for(int step=0;step<120 && restore.RestoreCount==count;step++)
                {motor.Simulate(idle,1f/60);restore.Tick(false,1f/60);}
                Assert.That(restore.RestoreCount,Is.EqualTo(count+1),"Stranded at "+hit.point);
                restore.Tick(false,1);recovered++;
            }
            Assert.That(recovered,Is.GreaterThanOrEqualTo(3));
            var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            foreach(var block in module.Blocks)
            {
                motor.ResetMotion(block.Pose.Position+Vector3.up*.36f,Quaternion.identity,0);Physics.SyncTransforms();
                for(int i=0;i<5;i++)motor.Simulate(idle,1f/60);
                Assert.That(restore.IsRestorePending,Is.False,block.StableId);
            }
            // View cone through the transfer approach: no overhead crane across the jump.
            foreach(float eyeY in new[]{4.92f,5.8f,6.5f})
            foreach(float yaw in new[]{5f,15f,25f,35f,45f})
            foreach(float pitch in new[]{-20f,-10f,0f,10f})
            {
                var origin=new Vector3(11,eyeY,29);
                foreach(var hit in Physics.RaycastAll(origin,Quaternion.Euler(pitch,yaw,0)*Vector3.forward,17))
                    Assert.That(hit.collider.GetComponentsInParent<Transform>().Any(t=>t.name.Contains("transfer-crane")),Is.False,"Jump view obstruction");
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator TrialMenuAllRoadsModesRetryCompletionAndExitPreserveSave()
        {
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            yield return new WaitForSecondsRealtime(.2f);
            int preference=PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1);
            string before=JsonUtility.ToJson(save.Current);int writes=store.Writes;
            Click("DEVELOPMENT Button");Click("CAMPAIGN FLOW TRIAL Button");yield return null;
            Capture("trial-menu");Click("Trial Start");yield return new WaitForSecondsRealtime(.6f);
            Assert.That(CampaignFlowTrial.Mode,Is.EqualTo(CampaignTrialMode.Foundation));
            Assert.That(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule.StableModuleId,Is.EqualTo(Foundry));
            foreach(var id in ModuleSelectionState.GetCampaignModuleIds())
            foreach(var mode in new[]{CampaignTrialMode.Accepted,CampaignTrialMode.FlowManual,CampaignTrialMode.FlowLanding,CampaignTrialMode.PreviousFlow,CampaignTrialMode.Foundation})
            {
                CampaignFlowTrial.Launch(id,mode);yield return Open();
                var player=Object.FindAnyObjectByType<PlayerRuntimeCoordinator>();var motor=player.Motor;
                var touch=Object.FindAnyObjectByType<TouchInputCoordinator>();
                Assert.That(motor.Profile.MovementMastery,Is.EqualTo(mode!=CampaignTrialMode.Accepted));
                if(mode==CampaignTrialMode.Foundation)
                {
                    Assert.That(motor.Profile.CompatibilityVersion,Is.EqualTo(4));
                    Assert.That(touch.JumpLookEnabled,Is.True);
                    Assert.That(touch.RuntimeProfile.EnableFixedButton,Is.False);
                    Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.False);
                    player.Input.SetMode(PlayerInputMode.Touch);
                    var button=Object.FindAnyObjectByType<TouchLookControl>();
                    var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                        {pointerId=901,position=new Vector2(1000,400),delta=new Vector2(20,-10)};
                    Assert.That(touch.TryClaim(TouchControlRole.Movement,900),Is.True);
                    touch.SetMovement(Vector2.up);
                    button.OnPointerDown(e);button.OnDrag(e);player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.True);
                    Assert.That(player.Input.JumpPressed,Is.False);
                    Assert.That(player.Input.Move,Is.EqualTo(Vector2.up));
                    Assert.That(player.Input.LookDelta.sqrMagnitude,Is.GreaterThan(0));
                    var yaw=player.transform.eulerAngles.y;
                    player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/60,motor);
                    Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw,player.transform.eulerAngles.y)),Is.GreaterThan(.01f));
                    e.position=new Vector2(700,600);button.OnDrag(e);player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.True);Assert.That(player.Input.JumpPressed,Is.False);
                    var stranger=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=902};
                    button.OnPointerUp(stranger);Assert.That(touch.JumpHeld,Is.True);
                    button.OnPointerUp(e);player.Input.Sample();Assert.That(player.Input.JumpHeld,Is.False);
                    Assert.That(player.Input.JumpPressed,Is.False,"Release must not emit a second jump");
                    button.OnPointerDown(e);touch.ResetState();player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.False);Assert.That(player.Input.JumpPressed,Is.False);
                    touch.Release(TouchControlRole.Movement,900);

                }
                else if(mode!=CampaignTrialMode.Accepted)
                {
                    Assert.That(touch.RuntimeProfile.FlowSteeringEnabled,Is.True);
                    Assert.That(touch.RuntimeProfile.EnableFixedButton,Is.False);
                    Assert.That(player.CameraRig.Pitch,Is.EqualTo(mode==CampaignTrialMode.FlowLanding?16:9).Within(.1f));
                }
                Assert.That(JsonUtility.ToJson(save.Current),Is.EqualTo(before));Assert.That(store.Writes,Is.EqualTo(writes));
                if(id==Foundry && (mode==CampaignTrialMode.FlowLanding || mode==CampaignTrialMode.Foundation))
                {
                    player.Input.SetMode(PlayerInputMode.Touch);touch.SetMovement(Vector2.up);
                    Assert.That(touch.TryClaim(TouchControlRole.Look,77),Is.True);
                    touch.BeginLookGesture(77,new Vector2(1000,400),1);touch.EndLookGesture(77,new Vector2(1000,400),1.05f,800);
                    touch.Release(TouchControlRole.Look,77);Assert.That(touch.ConsumeJumpPressed(),Is.EqualTo(mode!=CampaignTrialMode.Foundation));
                    touch.AddLookDelta(new Vector2(0,20));player.Input.Sample();float pitch=player.CameraRig.Pitch;
                    player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/60,motor);
                    Assert.That(player.CameraRig.Pitch,Is.LessThan(pitch));
                    player.GetComponent<RestoreController>().RestoreNow();Assert.That(touch.Movement,Is.EqualTo(Vector2.zero));
                    Assert.That(player.CameraRig.Pitch,Is.EqualTo(mode==CampaignTrialMode.FlowLanding?16:9).Within(.1f));
                    Capture("foundry-flow");
                    Assert.That(Object.FindAnyObjectByType<PatchBlock>().TryComplete(motor),Is.True);
                    yield return new WaitForSecondsRealtime(.3f);Capture("trial-results");
                    Assert.That(CampaignFlowTrial.Best(Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule),Is.GreaterThan(0));
                    Click("RETRY Button");yield return new WaitForSecondsRealtime(.6f);
                    Assert.That(CampaignFlowTrial.Mode,Is.EqualTo(mode));
                }
            }
            Assert.That(JsonUtility.ToJson(save.Current),Is.EqualTo(before));Assert.That(store.Writes,Is.EqualTo(writes));
            Assert.That(PlayerPrefs.GetInt(TouchInputCoordinator.ControlProfilePreferenceKey,-1),Is.EqualTo(preference));
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
            Assert.That(CampaignFlowTrial.Active,Is.False);
            ModuleSelectionState.Select(Foundry,true);yield return Open();
            Assert.That(Object.FindAnyObjectByType<ParkourMotor>().Profile.MovementMastery,Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
        [UnityTest] public IEnumerator FoundationWholeRightSurfaceHoldDragReleaseAndUiBoundaries()
        {
            CampaignFlowTrial.Launch(Foundry,CampaignTrialMode.Foundation);yield return Open();
            var player=Object.FindAnyObjectByType<PlayerRuntimeCoordinator>();
            var touch=Object.FindAnyObjectByType<TouchInputCoordinator>();
            var look=Object.FindAnyObjectByType<TouchLookControl>();
            var jump=Object.FindAnyObjectByType<JumpTouchControl>();
            Assert.That(jump.GetComponent<Image>().raycastTarget,Is.False);
            var safe=(RectTransform)look.transform.parent;
            Canvas.ForceUpdateCanvases();
            UnityEngine.EventSystems.PointerEventData Pointer(int id,Vector2 normalized)
            {
                var rect=safe.rect;
                var world=safe.TransformPoint(new Vector3(Mathf.Lerp(rect.xMin,rect.xMax,normalized.x),Mathf.Lerp(rect.yMin,rect.yMax,normalized.y),0));
                return new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                    {pointerId=id,position=RectTransformUtility.WorldToScreenPoint(null,world)};
            }
            GameObject Target(UnityEngine.EventSystems.PointerEventData e)
            {
                var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(e,hits);
                Assert.That(hits,Is.Not.Empty);
                return UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerDownHandler>(hits[0].gameObject);
            }
            foreach(float x in new[]{.62f,.77f,.94f}) foreach(float y in new[]{.2f,.5f,.8f})
            {
                var e=Pointer(801,new Vector2(x,y));var target=Target(e);
                Assert.That(target,Is.EqualTo(look.gameObject),"Right camera acquisition at "+x+","+y);
                UnityEngine.EventSystems.ExecuteEvents.Execute(target,e,UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
                Assert.That(touch.JumpHeld,Is.True,"Immediate hold, no gesture delay");
                look.OnInitializePotentialDrag(e);Assert.That(e.useDragThreshold,Is.False);
                e.delta=new Vector2(.1f,.1f);look.OnDrag(e);
                Assert.That(touch.ConsumeLookDelta().sqrMagnitude,Is.GreaterThan(0));
                look.OnPointerUp(e);Assert.That(touch.JumpHeld,Is.False);
                Assert.That(touch.ConsumeJumpPressed(),Is.False,"No queued release tap");
            }
            var left=Pointer(802,new Vector2(.25f,.4f));
            Assert.That(Target(left),Is.Not.EqualTo(look.gameObject));
            var menu=Object.FindObjectsByType<Button>().Single(b=>b.name=="II Button");
            var menuPoint=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {pointerId=803,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,
                 position=RectTransformUtility.WorldToScreenPoint(null,menu.transform.position)};
            Assert.That(Target(menuPoint),Is.EqualTo(menu.gameObject));
            UnityEngine.EventSystems.ExecuteEvents.Execute(menu.gameObject,menuPoint,UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
            Assert.That(touch.JumpHeld,Is.False,"UI down must not enter gameplay");

            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.layer=LayerMask.NameToLayer("Ground");
            floor.transform.position=new Vector3(1000,-.5f,0);floor.transform.localScale=new Vector3(300,1,300);
            Physics.SyncTransforms();player.Input.SetMode(PlayerInputMode.Touch);
            var joystick=Object.FindAnyObjectByType<MovementJoystickControl>();
            var ring=GameObject.Find("Movement Floating Origin").GetComponent<RectTransform>();
            foreach(int hz in new[]{30,60,120})
            {
                touch.ResetState();player.Motor.ResetMotion(new Vector3(1000,.05f,0),Quaternion.identity,0);
                var e=Pointer(804,new Vector2(.9f,.4f));look.OnPointerDown(e);
                var move=Pointer(807,new Vector2(.25f,.4f));joystick.OnPointerDown(move);
                int before=player.Motor.JumpCount;
                for(int i=0;i<hz*5;i++)
                {
                    // Two real UI pointer streams, including deliberate alternating strafe.
                    var stick=new Vector2(i<hz*2 ? .65f : -.65f,1).normalized;
                    move.position=RectTransformUtility.WorldToScreenPoint(null,
                        ring.TransformPoint(stick*ring.rect.width*.5f));
                    joystick.OnDrag(move);
                    e.delta=new Vector2(24f/hz,-1.2f/hz);e.position+=e.delta;
                    look.OnDrag(e);player.Input.Sample();
                    Assert.That(player.Input.JumpHeld,Is.True);
                    Assert.That(player.Input.Move.x*stick.x,Is.GreaterThan(0));
                    player.CameraRig.ApplyLook(player.Input,PlayerInputMode.Touch,1f/hz,player.Motor);
                    player.Motor.Simulate(player.Input,1f/hz);
                }
                Assert.That(player.Motor.JumpCount-before,Is.GreaterThanOrEqualTo(6));
                Debug.Log($"REFERENCE TOUCH hz={hz} jumps={player.Motor.JumpCount-before} yaw={player.transform.eulerAngles.y:F2} speed={player.Motor.HorizontalSpeed:F3}");
                look.OnPointerUp(e);int released=player.Motor.JumpCount;
                joystick.OnPointerUp(move);
                for(int i=0;i<hz*2;i++){player.Input.Sample();player.Motor.Simulate(player.Input,1f/hz);}
                Assert.That(player.Motor.JumpCount,Is.EqualTo(released));
                Assert.That(player.Input.JumpHeld,Is.False);
            }
            Object.Destroy(floor);
            var held=Pointer(805,new Vector2(.8f,.4f));look.OnPointerDown(held);
            UnityEngine.EventSystems.ExecuteEvents.Execute(menu.gameObject,menuPoint,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            Assert.That(touch.JumpHeld,Is.False,"Opening the menu cancels gameplay hold");
            yield return null;
            Assert.That(GameObject.Find("Alpha Run Menu"),Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var covered=Pointer(806,new Vector2(.6f,.5f));Assert.That(Target(covered),Is.Not.EqualTo(look.gameObject));
            menu.onClick.Invoke();
            look.OnPointerDown(held);look.enabled=false;Assert.That(touch.JumpHeld,Is.False);look.enabled=true;
            look.OnPointerDown(held);touch.SendMessage("OnApplicationPause",true);Assert.That(touch.JumpHeld,Is.False);
            look.OnPointerDown(held);touch.SendMessage("OnApplicationFocus",false);Assert.That(touch.JumpHeld,Is.False);
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }

        private static void Click(string name)=>Object.FindObjectsByType<Button>().Single(b=>b.name==name).onClick.Invoke();
        private static void Capture(string name)
        {
            Directory.CreateDirectory("Logs/Production099QA");var camera=Camera.main;
            var rt=new RenderTexture(1560,720,24);var tex=new Texture2D(1560,720,TextureFormat.RGB24,false);
            var active=RenderTexture.active;var aspect=camera.aspect;
            var canvases=Object.FindObjectsByType<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
            try
            {
                camera.targetTexture=rt;camera.aspect=1560f/720;
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=camera;c.planeDistance=1;}
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,1560,720),0,0);tex.Apply();File.WriteAllBytes("Logs/Production099QA/"+name+".png",tex.EncodeToPNG());
            }
            finally
            {
                foreach(var c in canvases){c.renderMode=RenderMode.ScreenSpaceOverlay;c.worldCamera=null;}
                camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=active;Object.Destroy(rt);Object.Destroy(tex);
            }
        }
    }
}
