using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using Avoidance.Gameplay.Visuals;
using Avoidance.Gameplay.Blocks;
using Avoidance.Input;
using Avoidance.UI.Touch;
using UnityEngine;
using UnityEngine.UI;

namespace Avoidance.UI
{
    public sealed class FlowLabSceneController : MonoBehaviour
    {
        private static bool launchRequested;
        public static void RequestLaunch() => launchRequested=true;
        public static bool ConsumeLaunchRequest() { bool value=launchRequested;launchRequested=false;return value; }
        private FlowLabDefinition definition;
        private readonly FlowLabSession session=new FlowLabSession();
        private CheckpointService checkpoints;
        private PlayerRuntimeCoordinator player;
        private TouchInputCoordinator touch;
        private Transform course;
        private Text feedback, instruction;
        private MovementProfile baseline, candidate;
        private MovementLabVisualProfile visuals;
        private int roomIndex, restoreCount;
        private float feedbackCountdown;
        private bool landingViewEnabled;
        private Text viewButton;
        public bool LandingViewEnabled => landingViewEnabled;
        public FlowLabSession Session => session;
        public FlowLabRoom Room => definition.rooms[roomIndex];
        public PlayerRuntimeCoordinator Player => player;

        public void Initialize(MovementLabSceneController factory)
        {
            definition=JsonUtility.FromJson<FlowLabDefinition>(Resources.Load<TextAsset>("Training/FlowLab").text);
            roomIndex=Mathf.Max(0,System.Array.FindIndex(definition.rooms,r=>r.id==definition.initialRoomId));
            visuals=Resources.Load<MovementLabVisualProfile>(MovementLabVisualProfile.ResourceName);
            baseline=Resources.Load<MovementProfile>("MovementProfiles/Movement_Default");
            candidate=Resources.Load<MovementProfile>("Training/Movement_Mastery");
            RenderSettings.fog=true;RenderSettings.fogColor=visuals.SkyFog;RenderSettings.fogDensity=.004f;
            RenderSettings.ambientLight=visuals.AmbientLight;
            var sun=new GameObject("Flow Lab Sun",typeof(Light));sun.transform.rotation=Quaternion.Euler(45,-25,0);
            sun.GetComponent<Light>().type=LightType.Directional;
            sun.GetComponent<Light>().intensity=1.2f;
            touch=factory.CreateTouchInterface(Resources.Load<TouchControlLayout>("Touch_Default"),out var oldStatus,out var oldCompletion,true);
            oldStatus.enabled=false;oldCompletion.enabled=false;
            touch.SetSessionControlProfile(TouchControlProfileKind.FlowSteerAutoDirect);
            checkpoints=new CheckpointService();checkpoints.SetStart(Room.start,Quaternion.identity);
            BuildRoom();
            player=factory.CreatePlayer(Room.start,new MovementProfileSet(new[]{candidate,baseline}),
                Resources.Load<CameraProfile>("Camera_Default"),touch,checkpoints,Application.version,oldStatus,oldCompletion,true);
            RenderSettings.skybox=Resources.Load<Material>("Materials/MAT_RR_Skybox_Seamless");
            Camera.main.clearFlags=CameraClearFlags.Skybox;
            var safe=oldStatus.transform.parent;
            instruction=Label(safe,"Flow instruction",new Vector2(.15f,.82f),new Vector2(.85f,.94f),26);
            feedback=Label(safe,"Flow feedback",new Vector2(.25f,.71f),new Vector2(.75f,.81f),24);
            Button(safe,"ROOM",.02f,()=>SelectRoom((roomIndex+1)%definition.rooms.Length));
            Button(safe,"RETRY",.18f,Retry);
            Button(safe,"MOTOR A/B",.34f,Compare);
            viewButton=Button(safe,"VIEW: MANUAL",.50f,CompareView);
            Button(safe,"CONTROLS",.66f,()=> {touch.SetSessionControlProfile(touch.RuntimeProfile.FlowSteeringEnabled
                ? TouchControlProfileKind.LeftMoveRightLookTapJump : TouchControlProfileKind.FlowSteerAutoDirect);Retry();});
            Button(safe,"EXIT",.82f,()=>StartCoroutine(new UnitySceneLevelLoader().LoadAsync("ModuleSelector")));
            Retry();
            UnitySceneLevelLoader.NotifyReady(gameObject.scene.name);
        }
        public void SelectRoom(int index)
        {
            roomIndex=Mathf.Clamp(index,0,definition.rooms.Length-1);BuildRoom();
            checkpoints.Reset();checkpoints.SetStart(Room.start,Quaternion.identity);Retry();
        }
        public void Compare()
        {
            player.Profiles.Select(player.Motor.Profile.MovementMastery ? baseline.ProfileId : candidate.ProfileId);
            player.Motor.SetProfile(player.Profiles.Current);Retry();
        }
        public void Retry()
        {
            player.GetComponent<RestoreController>().RestoreNow();
            bool useLandingView=landingViewEnabled && touch.RuntimeProfile.FlowSteeringEnabled;
            player.CameraRig.ConfigureLandingView(useLandingView ? definition.landingView : null);
            player.CameraRig.ResetView(Quaternion.identity,useLandingView
                ? Mathf.Max(Room.initialPitch,definition.landingView.initialPitch) : Room.initialPitch);
            restoreCount=player.GetComponent<RestoreController>().RestoreCount;
            session.Reset(definition,Room,player.Motor,touch.RuntimeProfile.FlowSteeringEnabled
                ? useLandingView ? "flow.landing-view" : "flow.manual-view" : "classic.manual-view");feedbackCountdown=0;
            RefreshFeedback();
        }
        public void CompareView()
        {
            landingViewEnabled=!landingViewEnabled;
            viewButton.text=landingViewEnabled?"VIEW: LANDING":"VIEW: MANUAL";
            Retry();
        }
        private void LateUpdate()
        {
            if(player==null)return;
            var restore=player.GetComponent<RestoreController>();
            if(restoreCount!=restore.RestoreCount || player.transform.position.y<Room.start.y-definition.fallDepth)
                Retry();
            session.Tick(player.Motor,Time.deltaTime);
            feedbackCountdown-=Time.unscaledDeltaTime;if(feedbackCountdown>0)return;feedbackCountdown=.1f;
            RefreshFeedback();
        }
        public void RefreshFeedback()
        {
            string mode=player.Motor.Profile.MovementMastery?"CANDIDATE":"PRODUCTION BASELINE";
            string view=!touch.RuntimeProfile.FlowSteeringEnabled ? "CLASSIC MANUAL"
                : landingViewEnabled ? "LANDING VIEW" : "MANUAL VIEW";
            instruction.text=$"{Room.title}  /  {mode}  /  {view}\n{Room.hint}";
            string metric=Room.exercise=="air"?$"Air gain +{session.AirGain:0.0} m/s"
                :Room.exercise=="bhop"?$"Chain {session.BestChain}/{definition.requiredChain}  |  Takeoff retained {player.Motor.LastCompleteTakeoffRetention:P0}"
                :Room.exercise=="landing"?$"Clean jumps {session.Jumps}  |  Gold finish ahead"
                :$"Surf contact {session.SurfSeconds:0.0}s  |  Jumps {session.Jumps}";
            feedback.text=session.Complete?$"COMPLETE  {session.Seconds:0.00}s  |  Session best {session.BestSeconds:0.00}s\nRetry for a cleaner line · ROOM for next exercise"
                :$"{player.Motor.HorizontalSpeed:0.0} m/s  |  {metric}\n{session.Seconds:0.0}s  ·  {(player.Motor.IsSurfing?"SURF":touch.RuntimeProfile.FlowSteeringEnabled?"Left steer · Right tap / pitch":"Classic controls")}";
        }
        private void BuildRoom()
        {
            if(course!=null){course.gameObject.SetActive(false);Destroy(course.gameObject);}
            course=new GameObject(Room.id).transform;
            foreach(var solid in Room.solids)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=solid.id;go.transform.SetParent(course);
                go.layer=LayerMask.NameToLayer("Ground");go.transform.position=solid.position;
                go.transform.localScale=solid.size;go.transform.rotation=Quaternion.Euler(solid.slope,0,0);
                go.GetComponent<Renderer>().sharedMaterial=visuals.MaterialFor(solid.surf?MovementLabMaterialRole.SurfSurface:MovementLabMaterialRole.NormalPlatform);
                if(solid.surf)go.AddComponent<SurfSurface>();
                foreach(float side in new[]{-.49f,.49f})
                    Stripe(go.transform,new Vector3(side,.502f,0),new Vector3(.008f,.004f,1),MovementLabMaterialRole.BoundaryLine);
                for(float z=-solid.size.z*.5f+3;z<solid.size.z*.5f;z+=5)
                    Stripe(go.transform,new Vector3(0,.503f,z/solid.size.z),new Vector3(.08f,.005f,.12f/solid.size.z),
                        solid.surf?MovementLabMaterialRole.CenterLine:MovementLabMaterialRole.BoundaryLine);
            }
            var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.name="Finish stripe";marker.transform.SetParent(course);
            marker.transform.position=Room.finish+Vector3.down*.45f;marker.transform.localScale=new Vector3(Room.finishSize.x,.02f,1);
            Destroy(marker.GetComponent<Collider>());marker.GetComponent<Renderer>().sharedMaterial=visuals.MaterialFor(MovementLabMaterialRole.PatchBlock);
            foreach(float side in new[]{-1f,1f})
            {
                var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Finish side post";post.transform.SetParent(course);
                post.layer=LayerMask.NameToLayer("Ground");post.transform.position=Room.finish+new Vector3(side*(Room.finishSize.x*.5f+.3f),.5f,0);
                post.transform.localScale=new Vector3(.25f,2,.25f);post.GetComponent<Renderer>().sharedMaterial=visuals.MaterialFor(MovementLabMaterialRole.PatchBlock);
            }
            Physics.SyncTransforms();
        }
        private void Stripe(Transform parent,Vector3 position,Vector3 size,MovementLabMaterialRole role)
        {
            var stripe=GameObject.CreatePrimitive(PrimitiveType.Cube);stripe.name="Surface marking";
            stripe.transform.SetParent(parent,false);stripe.transform.localPosition=position;stripe.transform.localScale=size;
            Destroy(stripe.GetComponent<Collider>());stripe.GetComponent<Renderer>().sharedMaterial=visuals.MaterialFor(role);
        }
        private static Text Label(Transform parent,string name,Vector2 min,Vector2 max,int size)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.raycastTarget=false;return text;
        }
        private static Text Button(Transform parent,string name,float x,UnityEngine.Events.UnityAction action)
        {
            var text=Label(parent,name+" Button",new Vector2(x,.945f),new Vector2(x+.15f,.995f),20);
            text.text=name;
            text.raycastTarget=true;var button=text.gameObject.AddComponent<Button>();button.targetGraphic=text;button.onClick.AddListener(action);
            return text;
        }
    }
}
