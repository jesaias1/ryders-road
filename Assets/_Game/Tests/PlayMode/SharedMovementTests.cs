using System.Collections.Generic;
using Avoidance.Gameplay.Player;
using Avoidance.Input;
using NUnit.Framework;
using UnityEngine;

namespace Avoidance.Tests.PlayMode
{
    public sealed class SharedMovementTests
    {
        private readonly List<Object> owned = new List<Object>();
        private sealed class Input : IPlayerInputSource, IHeldJumpInputSource
        {
            public Vector2 Move {get;set;} public Vector2 LookDelta => Vector2.zero;
            public bool JumpPressed {get;set;} public bool JumpHeld {get;set;}
            public void ResetState(){Move=Vector2.zero;JumpHeld=JumpPressed=false;}
        }
        private ParkourMotor Motor(MovementProfile profile)
        {
            var go=new GameObject("Shared fixture",typeof(CharacterController));owned.Add(go);
            go.layer=LayerMask.NameToLayer("Player");go.transform.position=new Vector3(3000,100,3000);
            var cc=go.GetComponent<CharacterController>();cc.height=1.8f;cc.center=Vector3.up*.9f;cc.radius=.38f;
            var motor=go.AddComponent<ParkourMotor>();motor.Initialize(profile);return motor;
        }
        [TearDown] public void Cleanup(){foreach(var item in owned)Object.DestroyImmediate(item);owned.Clear();}

        [TestCase(30)] [TestCase(50)] [TestCase(60)] [TestCase(120)]
        public void BeforeAfterCorrectionBrakingCoastingAndSkillGain(int hz)
        {
            var old=Resources.Load<MovementProfile>("Training/Movement_Foundation");
            var shared=MovementProfile.LoadShared();
            Assert.That(shared.ResponsiveAirControl,Is.True,"Serialized shared profile must select responsive math");
            Vector3 Fly(MovementProfile profile,float speed,float angle,bool neutral=false,bool opposite=false)
            {
                var motor=Motor(profile);motor.transform.rotation=Quaternion.Euler(0,angle,0);
                motor.ApplyLaunch(Vector3.forward*speed,true);
                var input=new Input{Move=neutral?Vector2.zero:opposite?Vector2.down:Vector2.up};
                for(int i=0;i<hz/2;i++)
                {
                    if(neutral)motor.transform.Rotate(0,240f/hz,0);
                    motor.Simulate(input,1f/hz);
                }
                Assert.That(motor.HorizontalSpeed,Is.LessThanOrEqualTo(18.001f));
                var result=motor.ActualHorizontalVelocity;
                motor.gameObject.SetActive(false);
                return result;
            }
            foreach(float speed in new[]{7.8f,10.8f,14f,17f})
            {
                float before=Vector3.Angle(Vector3.forward,Fly(old,speed,10));
                float after=Vector3.Angle(Vector3.forward,Fly(shared,speed,10));
                float sideOld=Vector3.Angle(Vector3.forward,Fly(old,speed,90));
                float sideNew=Vector3.Angle(Vector3.forward,Fly(shared,speed,90));
                Assert.That(after,Is.GreaterThan(5));
                Assert.That(sideNew,Is.GreaterThan(sideOld+5));
                Assert.That(Fly(shared,speed,0,true),Is.EqualTo(Vector3.forward*speed));
                Assert.That(Fly(shared,speed,0,false,true).z,Is.LessThan(0));
                Debug.Log($"SHARED150 hz={hz} speed={speed} correction10={before:F3}->{after:F3} side={sideOld:F3}->{sideNew:F3}");
            }
            var skilled=Motor(shared);skilled.ApplyLaunch(Vector3.forward*10.8f,true);var strafe=new Input();
            for(int i=0;i<hz;i++)
            {
                // Test pilot only: deliberately keep wish nearly perpendicular to momentum.
                skilled.transform.rotation=Quaternion.LookRotation(skilled.ActualHorizontalVelocity)*Quaternion.Euler(0,70,0);
                strafe.Move=Vector2.up;skilled.Simulate(strafe,1f/hz);
            }
            Assert.That(skilled.HorizontalSpeed,Is.GreaterThan(12),"Strafing can earn momentum beyond the old envelope");
            Assert.That(Fly(shared,14,0).magnitude,Is.EqualTo(14).Within(.001),"Straight overspeed input earns no bonus");
        }

        [Test] public void ResetFromGroundThenLaunchDoesNotReuseStaleContact()
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);owned.Add(floor);
            floor.layer=LayerMask.NameToLayer("Ground");floor.transform.position=new Vector3(3000,-.5f,3000);floor.transform.localScale=new Vector3(50,1,50);
            var motor=Motor(MovementProfile.LoadShared());var input=new Input();
            motor.ResetMotion(new Vector3(3000,.05f,3000),Quaternion.identity,0);Physics.SyncTransforms();
            for(int i=0;i<10;i++)motor.Simulate(input,1f/60);
            Assert.That(motor.IsGrounded,Is.True);
            motor.ResetMotion(new Vector3(3000,100,3000),Quaternion.identity,0);
            motor.ApplyLaunch(Vector3.forward*14,true);input.JumpHeld=true;Physics.SyncTransforms();
            motor.Simulate(input,1f/60);
            Assert.That(motor.IsGrounded,Is.False);Assert.That(motor.JumpCount,Is.Zero);
            Assert.That(motor.HorizontalSpeed,Is.EqualTo(14).Within(.001));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void GroundReleaseReversalAndCleanHeldHops(int hz)
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);owned.Add(floor);
            floor.layer=LayerMask.NameToLayer("Ground");floor.transform.position=new Vector3(3000,-.5f,3000);floor.transform.localScale=new Vector3(200,1,200);
            var motor=Motor(MovementProfile.LoadShared());motor.ResetMotion(new Vector3(3000,.05f,3000),Quaternion.identity,0);Physics.SyncTransforms();
            var input=new Input{Move=Vector2.up};
            for(int i=0;i<hz;i++)motor.Simulate(input,1f/hz);
            Assert.That(motor.HorizontalSpeed,Is.EqualTo(7.8f).Within(.01));
            input.Move=Vector2.zero;int stop=0;
            while(motor.HorizontalSpeed>.01f && stop<hz){motor.Simulate(input,1f/hz);stop++;}
            Assert.That((float)stop/hz,Is.LessThan(.16));
            input.Move=Vector2.up;for(int i=0;i<hz;i++)motor.Simulate(input,1f/hz);
            input.Move=Vector2.down;int reverse=0;
            while(motor.ActualHorizontalVelocity.z>=0 && reverse<hz){motor.Simulate(input,1f/hz);reverse++;}
            Assert.That((float)reverse/hz,Is.LessThan(.16));
            input.Move=Vector2.zero;motor.ApplyLaunch(Vector3.forward*14+Vector3.up*motor.Profile.JumpVelocity,true);input.JumpHeld=true;
            for(int i=0;i<hz*4;i++)motor.Simulate(input,1f/hz);
            Assert.That(motor.JumpCount,Is.GreaterThanOrEqualTo(5));
            Assert.That(motor.HorizontalSpeed,Is.EqualTo(14).Within(.02));
            Debug.Log($"SHARED150 GROUND hz={hz} stop={stop/(float)hz:F3} reverse={reverse/(float)hz:F3} heldSpeed={motor.HorizontalSpeed:F3}");
        }
    }
}
