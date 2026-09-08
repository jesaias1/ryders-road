using System.Collections.Generic;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Blocks;
using Avoidance.Input;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.PlayMode
{
    public class MovementMasteryTests
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<Collider> suspended = new List<Collider>();
        [SetUp] public void IsolatePhysics()
        {
            foreach(var collider in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(collider.enabled){suspended.Add(collider);collider.enabled=false;}
        }
        private MovementProfile Profile => Resources.Load<MovementProfile>("MovementProfiles/Movement_Default");
        protected virtual MovementProfile Candidate => Resources.Load<MovementProfile>("Training/Movement_Mastery");
        private sealed class Input : IPlayerInputSource, IFlowSteeringInputSource
        {
            public Vector2 Move { get; set; }
            public Vector2 LookDelta => Vector2.zero;
            public bool JumpPressed { get; set; }
            public bool FlowSteeringEnabled { get; set; } = true;
            public AutoCameraProfileKind AutoCameraProfile => AutoCameraProfileKind.Balanced;
            public void ResetState() { Move = Vector2.zero; JumpPressed = false; }
        }
        private ParkourMotor Motor(Vector3 position, MovementProfile profile = null)
        {
            var go = new GameObject("mastery fixture", typeof(CharacterController)); owned.Add(go);
            go.layer=LayerMask.NameToLayer("Player");
            go.transform.position = position;
            var cc = go.GetComponent<CharacterController>(); cc.height=1.8f; cc.radius=.38f;
            cc.center=Vector3.up*.9f; cc.skinWidth=.045f; cc.stepOffset=.32f; cc.slopeLimit=52;
            var motor=go.AddComponent<ParkourMotor>(); motor.Initialize(profile ?? Profile); return motor;
        }
        private GameObject Box(Vector3 position, Vector3 size, float slope = 0)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(go);
            go.layer=LayerMask.NameToLayer("Ground"); go.transform.position=position;
            go.transform.localScale=size; go.transform.rotation=Quaternion.Euler(slope,0,0);
            Physics.SyncTransforms(); return go;
        }
        [TearDown] public void Cleanup()
        {
            foreach(var item in owned) Object.DestroyImmediate(item); owned.Clear();
            foreach(var collider in suspended)if(collider!=null)collider.enabled=true;suspended.Clear();
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void ProductionBaseline_RealControllerWalkJumpSteerAndRetry(int hz)
        {
            Box(new Vector3(0,-.5f,0),new Vector3(200,1,200));
            var motor=Motor(new Vector3(0,.05f,0)); var input=new Input { Move=Vector2.up }; float dt=1f/hz;
            for(int i=0;i<hz;i++) motor.Simulate(input,dt);
            Assert.That(motor.HorizontalSpeed,Is.EqualTo(7.8f).Within(.02f));
            input.JumpPressed=true; motor.Simulate(input,dt); input.JumpPressed=false;
            var start=motor.transform.position; float time=0;
            while(!motor.IsGrounded && time<2) {motor.Simulate(input,dt);time+=dt;}
            float distance=Vector3.ProjectOnPlane(motor.transform.position-start,Vector3.up).magnitude;
            Assert.That(time,Is.InRange(.4f,.8f)); Assert.That(distance,Is.InRange(3f,6f));
            Debug.Log($"MASTERY BASELINE {hz}Hz run=7.8 airtime={time:F3} distance={distance:F3} landing={motor.HorizontalSpeed:F3}");
            motor.ResetMotion(new Vector3(0,20,0),Quaternion.identity,0); motor.ApplyLaunch(new Vector3(0,0,8.2f),true);
            input.Move=Vector2.up; for(int i=0;i<hz/2;i++)motor.Simulate(input,dt); float straight=motor.HorizontalSpeed;
            motor.ResetMotion(new Vector3(0,20,0),Quaternion.identity,0); motor.ApplyLaunch(new Vector3(0,0,8.2f),true);
            input.Move=new Vector2(.65f,1); for(int i=0;i<hz/2;i++)motor.Simulate(input,dt);
            Debug.Log($"MASTERY BASELINE {hz}Hz straight={straight:F3} curve={motor.HorizontalSpeed:F3} position={motor.transform.position}");
            motor.ResetMotion(new Vector3(0,.05f,0),Quaternion.identity,.08f); input.ResetState();
            Assert.That(motor.Velocity,Is.EqualTo(Vector3.zero));Assert.That(motor.JumpBuffered,Is.False);
            Assert.That(motor.IsSurfing,Is.False);
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void Candidate_ActualNormalizedFlowInputRewardsArcAndBoundsLongFlight(int hz)
        {
            Assert.That(Candidate,Is.Not.Null,"Training profile is loadable");
            Assert.That(Candidate.MovementMastery,Is.True,"Training profile opts in");
            var motor=Motor(new Vector3(0,100,0),Candidate);float dt=1f/hz;
            float Flight(Vector2 move)
            {
                motor.ResetMotion(new Vector3(0,100,0),Quaternion.identity,0);
                motor.ApplyLaunch(new Vector3(0,0,8.2f),true);
                var input=new Input { Move=move.normalized };
                for(int i=0;i<hz/2;i++)motor.Simulate(input,dt);
                return motor.HorizontalSpeed;
            }
            float straight=Flight(Vector2.up), curved=Flight(new Vector2(.65f,1)),turnOnly=Flight(Vector2.right);
            Assert.That(straight,Is.EqualTo(8.2f).Within(.001f));
            Assert.That(curved,Is.GreaterThan(straight+.2f));
            Assert.That(turnOnly,Is.EqualTo(straight).Within(.001f),"Yaw alone adds no energy");
            var steering=new Input {Move=new Vector2(.65f,1).normalized};
            for(int i=0;i<hz*20;i++)motor.Simulate(steering,dt);
            Assert.That(motor.HorizontalSpeed,Is.LessThanOrEqualTo(Candidate.SoftMomentumLimit+.001f));
            Debug.Log($"MASTERY CANDIDATE {hz}Hz normalized straight={straight:F3} curve={curved:F3} turnOnly={turnOnly:F3}");
        }

        [Test] public void ProjectedAccelerationPreservesPerpendicularVelocityAndOverspeed()
        {
            var initial=new Vector3(5,0,12);
            var result=MasteryMovementMath.Accelerate(initial,Vector3.right,8.2f,13,10.8f,.016f);
            Assert.That(result.z,Is.EqualTo(initial.z));
            Assert.That(result.magnitude,Is.EqualTo(initial.magnitude).Within(.001f));
            result=MasteryMovementMath.Accelerate(new Vector3(3,0,7),Vector3.right,8.2f,13,10.8f,.016f);
            Assert.That(result.z,Is.EqualTo(7));Assert.That(result.x,Is.GreaterThan(3));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void BufferedHopsAreReliableAndRetainMoreThanDelayedJumps(int hz)
        {
            Box(new Vector3(0,-.5f,0),new Vector3(300,1,300));
            var motor=Motor(new Vector3(0,.05f,0),Candidate);float dt=1f/hz;
            float Chain(bool buffered)
            {
                motor.ResetMotion(new Vector3(0,.05f,0),Quaternion.identity,0);
                var input=new Input {Move=Vector2.up};
                for(int i=0;i<hz;i++)motor.Simulate(input,dt);
                int begin=motor.JumpCount; input.JumpPressed=true;motor.Simulate(input,dt);input.JumpPressed=false;
                bool armed=true;float groundTime=0;float retained=0;
                for(int i=0;i<hz*8 && motor.JumpCount<begin+5;i++)
                {
                    if(motor.IsGrounded)groundTime+=dt; else groundTime=0;
                    input.JumpPressed=armed && (buffered ? motor.VerticalSpeed < -3 && motor.transform.position.y<.8f : groundTime>.22f);
                    if(input.JumpPressed)armed=false;
                    int before=motor.JumpCount; motor.Simulate(input,dt);
                    if(motor.JumpCount>before){armed=true;retained=motor.LastCompleteTakeoffRetention;}
                }
                Assert.That(motor.JumpCount,Is.EqualTo(begin+5),"Five separate tap intents must produce five jumps");
                Assert.That(motor.HorizontalSpeed,Is.LessThanOrEqualTo(Candidate.HardVelocitySafetyLimit));
                return retained;
            }
            var clean=Chain(true);var late=Chain(false);
            Assert.That(clean,Is.GreaterThan(.98f));Assert.That(late,Is.LessThan(clean-.04f));
            Debug.Log($"MASTERY HOPS {hz}Hz clean={clean:F3} delayed={late:F3}");
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void SurfRealControllerMaintainsContactAndCanJumpAway(int hz)
        {
            var ramp=Box(new Vector3(0,.43f,15.18f),new Vector3(18,.5f,24),32);ramp.AddComponent<SurfSurface>();
            Box(new Vector3(0,-6.5f,34),new Vector3(24,1,18));
            var motor=Motor(new Vector3(0,5.5f,8),Candidate);var input=new Input{Move=Vector2.up};float dt=1f/hz;
            motor.ApplyLaunch(Vector3.forward*7.8f,true);float contact=0;bool jumped=false;float afterJump=0;
            for(int i=0;i<hz*3;i++)
            {
                if(motor.IsSurfing)contact+=dt;
                input.JumpPressed=!jumped && contact>.35f;
                if(input.JumpPressed)jumped=true;
                motor.Simulate(input,dt);
                if(jumped){afterJump+=dt;if(afterJump<.1f)Assert.That(motor.IsSurfing,Is.False,"Jump must separate from ramp");}
                Assert.That(float.IsNaN(motor.Velocity.sqrMagnitude),Is.False);
                Assert.That(motor.HorizontalSpeed,Is.LessThanOrEqualTo(18.001f));
            }
            Debug.Log($"MASTERY SURF {hz}Hz contact={contact:F3} position={motor.transform.position} jumped={jumped}");
            Assert.That(contact,Is.GreaterThan(.35f));Assert.That(jumped,Is.True);
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void WallCollisionClipsStoredVelocityAndResetClearsIt(int hz)
        {
            Box(new Vector3(0,5,4),new Vector3(20,20,1));
            var motor=Motor(new Vector3(0,5,0),Candidate);motor.ApplyLaunch(Vector3.forward*12,true);
            var input=new Input();for(int i=0;i<hz;i++)motor.Simulate(input,1f/hz);
            Assert.That(motor.transform.position.z,Is.LessThan(3.5f));Assert.That(motor.Velocity.z,Is.LessThan(.01f));
            motor.ResetMotion(Vector3.up*5,Quaternion.identity,.08f);
            Assert.That(motor.DisplacementVelocity,Is.EqualTo(Vector3.zero));Assert.That(motor.Velocity,Is.EqualTo(Vector3.zero));
            Assert.That(motor.JumpBuffered,Is.False);Assert.That(motor.IsSurfing,Is.False);
        }
    }
}
