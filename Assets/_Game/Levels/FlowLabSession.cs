using System.Collections.Generic;
using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Levels
{
    // Attempt-local training evidence. No save, rank, route or force authority.
    public sealed class FlowLabSession
    {
        private readonly Dictionary<string, float> best = new Dictionary<string, float>();
        private FlowLabDefinition definition;
        private FlowLabRoom room;
        private string resultKey;
        private int lastJumpCount;
        private float takeoffSpeed;
        private bool wasGrounded;
        public float Seconds { get; private set; }
        public float SurfSeconds { get; private set; }
        public float AirGain { get; private set; }
        public int Chain { get; private set; }
        public int BestChain { get; private set; }
        public int Jumps { get; private set; }
        public bool Complete { get; private set; }
        public bool Started { get; private set; }
        public float BestSeconds => best.TryGetValue(resultKey, out var value) ? value : 0;
        public void Reset(FlowLabDefinition config, FlowLabRoom selected, ParkourMotor motor, string comparisonId = "")
        {
            definition=config;room=selected;resultKey=room.id+":"+motor.Profile.ProfileId+":"+comparisonId;
            Seconds=SurfSeconds=AirGain=takeoffSpeed=0;Chain=BestChain=Jumps=0;
            Complete=Started=false;lastJumpCount=motor.JumpCount;wasGrounded=motor.IsGrounded;
        }
        public void Tick(ParkourMotor motor, float dt)
        {
            if (Complete) return;
            if (motor.HorizontalSpeed > .1f || motor.JumpCount != lastJumpCount) Started=true;
            if (!Started) return;
            Seconds+=dt;
            if(motor.IsSurfing) SurfSeconds+=dt;
            if(motor.JumpCount!=lastJumpCount)
            {
                Jumps++;lastJumpCount=motor.JumpCount;
                Chain=motor.LastCompleteTakeoffRetention>=.95f && Chain>0 ? Chain+1 : 1;
                BestChain=Mathf.Max(BestChain,Chain);takeoffSpeed=motor.HorizontalSpeed;
            }
            if(!motor.IsGrounded && !motor.IsSurfing && takeoffSpeed>0)
                AirGain=Mathf.Max(AirGain,motor.HorizontalSpeed-motor.Profile.AirWishSpeed);
            if(motor.IsGrounded && wasGrounded && motor.HorizontalSpeed < motor.Profile.BaseRunSpeed*.9f)Chain=0;
            wasGrounded=motor.IsGrounded;
            bool evidence=room.exercise=="landing" ? Jumps>=2
                : room.exercise=="air" ? Jumps>0 && AirGain>=definition.minimumAirGain
                : room.exercise=="bhop" ? BestChain>=definition.requiredChain
                : room.exercise=="surf" ? SurfSeconds>=definition.minimumSurfSeconds && !motor.IsSurfing
                : Jumps>=2 && SurfSeconds>=definition.minimumSurfSeconds && !motor.IsSurfing;
            if(evidence && motor.IsGrounded && new Bounds(room.finish,room.finishSize).Contains(motor.transform.position))
            {
                Complete=true;
                if(BestSeconds<=0 || Seconds<BestSeconds)best[resultKey]=Seconds;
            }
        }
    }
}
