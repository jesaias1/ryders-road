using System.Collections.Generic;
using Avoidance.Gameplay.Player;
using Avoidance.Input;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.PlayMode
{
    public sealed class RealRouteAirControlTests
    {
        private readonly List<Object> owned = new List<Object>();
        private readonly List<Collider> suspended = new List<Collider>();
        private sealed class Input : IPlayerInputSource, IFlowSteeringInputSource
        {
            public Vector2 Move { get; set; }
            public Vector2 LookDelta => Vector2.zero;
            public bool JumpPressed { get; set; }
            public bool FlowSteeringEnabled => true;
            public AutoCameraProfileKind AutoCameraProfile => AutoCameraProfileKind.Direct;
            public void ResetState() { Move = Vector2.zero; JumpPressed = false; }
        }
        [SetUp] public void Setup()
        {
            foreach(var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(c.enabled) { suspended.Add(c); c.enabled = false; }
        }
        [TearDown] public void Cleanup()
        {
            foreach(var item in owned) Object.DestroyImmediate(item); owned.Clear();
            foreach(var c in suspended) if(c != null) c.enabled = true; suspended.Clear();
        }
        private ParkourMotor Motor(string resource)
        {
            var go = new GameObject("Real route test", typeof(CharacterController)); owned.Add(go);
            go.layer = LayerMask.NameToLayer("Player"); go.transform.position = Vector3.up * 100;
            var cc = go.GetComponent<CharacterController>(); cc.height = 1.8f; cc.radius = .38f;
            cc.center = Vector3.up * .9f; cc.skinWidth = .045f; cc.stepOffset = .32f;
            var motor = go.AddComponent<ParkourMotor>(); motor.Initialize(Resources.Load<MovementProfile>(resource));
            return motor;
        }
        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void ShortCorrectionCeilingBrakingAndYawComparison(int hz)
        {
            var old = Motor("Training/Movement_Mastery");
            var next = Motor("Training/Movement_RealRoute"); float dt = 1f/hz;
            Vector3 Flight(ParkourMotor motor, Vector2 stick, float speed, bool reverse = false)
            {
                old.gameObject.SetActive(motor == old); next.gameObject.SetActive(motor == next);
                motor.ResetMotion(Vector3.up*100, Quaternion.identity, 0);
                motor.ApplyLaunch(Vector3.forward*speed, true);
                var input = new Input { Move = Vector2.ClampMagnitude(stick,1) };
                for(int i=0;i<hz/3;i++)
                {
                    if(reverse && i==hz/6) input.Move = new Vector2(-input.Move.x,input.Move.y);
                    motor.Simulate(input,dt);
                }
                return motor.Velocity;
            }
            foreach(float speed in new[]{8.2f,10.8f,14f})
            {
                float deflection = speed > 10.8f ? 1f : .65f;
                var a = Flight(old,new Vector2(deflection,1),speed);
                float oldX = old.transform.position.x;
                var b = Flight(next,new Vector2(deflection,1),speed);
                float newX = next.transform.position.x;
                Assert.That(b.x,Is.GreaterThan(a.x+.3f),"Useful correction at speed "+speed);
                Assert.That(newX,Is.GreaterThan(oldX+.05f));
                Assert.That(next.HorizontalSpeed,Is.LessThanOrEqualTo(Mathf.Max(speed,10.8f)+.001f));
                var left = Flight(next,new Vector2(-deflection,1),speed);
                Assert.That(left.x,Is.EqualTo(-b.x).Within(.001f));
                var yaw = Flight(next,Vector2.right,speed);
                Assert.That(yaw.x,Is.EqualTo(0).Within(.001f));
                Assert.That(yaw.z,Is.EqualTo(speed).Within(.001f));
                var brake = Flight(next,Vector2.down,speed);
                Assert.That(brake.z,Is.LessThan(speed-4));
                var recovery = Flight(next,new Vector2(deflection,1),speed,true);
                Assert.That(recovery.x,Is.LessThan(b.x),"Opposing thumb must undo a poor strafe");
                Debug.Log($"ROUTE AIR {hz}Hz start={speed:F1} oldVx={a.x:F3} newVx={b.x:F3} oldX={oldX:F3} newX={newX:F3} brakeVz={brake.z:F3} recoveryVx={recovery.x:F3}");
            }
            var straight = Flight(next,Vector2.up,8.2f);
            Assert.That(straight.z,Is.EqualTo(8.2f).Within(.001f));
        }
        [Test] public void EnergyLimitKeepsSteeringAndOppositeInputActuallyBrakes()
        {
            var v = new Vector3(0,0,10.8f);
            var result = RouteAirControlMath.Accelerate(v,Vector3.right,8.2f,26,30,1,10.8f,.02f,
                out _,out _,out _,out var limited);
            Assert.That(limited,Is.True); Assert.That(result.x,Is.GreaterThan(.5f));
            Assert.That(result.magnitude,Is.EqualTo(v.magnitude).Within(.001f));
            var idle = RouteAirControlMath.Accelerate(v,Vector3.zero,8.2f,26,30,1,10.8f,.02f,out _,out _,out _,out _);
            Assert.That(idle,Is.EqualTo(v));
        }
        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void ExplicitCorrectionReachesSmallLandingThatStraightInputMisses(int hz)
        {
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(platform);
            platform.layer = LayerMask.NameToLayer("Ground");
            platform.transform.position = new Vector3(1.25f,-.5f,5.2f);
            platform.transform.localScale = new Vector3(1,1,3);
            Physics.SyncTransforms();
            var motor = Motor("Training/Movement_RealRoute");
            bool Jump(bool steer)
            {
                motor.ResetMotion(Vector3.up*.05f,Quaternion.identity,0);
                motor.ApplyLaunch(new Vector3(0,motor.Profile.JumpVelocity,8.2f),true);
                var input = new Input();
                for(int i=0;i<hz;i++)
                {
                    input.Move = steer && i < hz/3 ? new Vector2(.65f,1).normalized : Vector2.zero;
                    motor.Simulate(input,1f/hz);
                    if(motor.IsGrounded) return true;
                }
                return false;
            }
            Assert.That(Jump(false),Is.False,"No hidden landing assistance");
            Assert.That(Jump(true),Is.True,"Explicit correction must produce a real platform contact");
            Assert.That(motor.GroundTransform,Is.EqualTo(platform.transform));
            Debug.Log($"ROUTE SMALL LANDING {hz}Hz position={motor.transform.position} speed={motor.HorizontalSpeed:F3}");
        }
        [Test] public void NearbyFloorDoesNotApplyGroundFrictionBeforeContact()
        {
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube); owned.Add(platform);
            platform.layer = LayerMask.NameToLayer("Ground"); platform.transform.position=Vector3.down*.5f;
            platform.transform.localScale=new Vector3(20,1,20); Physics.SyncTransforms();
            var motor=Motor("Training/Movement_RealRoute");
            motor.ResetMotion(Vector3.up*.2f,Quaternion.identity,0);
            motor.ApplyLaunch(Vector3.forward*10.8f,true);
            motor.Simulate(new Input(),1f/60);
            Assert.That(motor.IsGrounded,Is.False);
            Assert.That(motor.HorizontalSpeed,Is.EqualTo(10.8f).Within(.001f));
        }
    }
}
