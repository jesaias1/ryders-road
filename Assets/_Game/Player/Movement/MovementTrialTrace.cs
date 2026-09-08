using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    // Bounded development evidence, no save-service dependencies or per-frame IO.
    public sealed class MovementTrialTrace : MonoBehaviour
    {
        private struct Sample
        {
            public float time, yaw, projection, request, applied;
            public Vector2 input;
            public Vector3 position, wish, velocity, net, contact;
            public bool ground, surf, limit, safety;
        }
        private readonly Sample[] samples = new Sample[9000];
        private ParkourMotor motor;
        private string scope, profile;
        private int count;
        private float seconds;
        public void Configure(ParkourMotor value, string context)
        {
            if (!Debug.isDebugBuild && !Application.isEditor) { enabled = false; return; }
            Flush();
            if (motor != null) motor.Simulated -= Capture;
            motor = value; scope = context; profile = motor.Profile.ProfileId;
            count = 0; seconds = 0; motor.Simulated += Capture;
        }
        private void Capture(float dt)
        {
            if (profile != motor.Profile.ProfileId) { Flush(); profile = motor.Profile.ProfileId; count = 0; seconds = 0; }
            seconds += dt;
            samples[count++ % samples.Length] = new Sample {
                time = seconds, yaw = motor.CurrentHeadingYaw, input = motor.LastMoveInput,
                position = motor.transform.position, wish = motor.LastDesiredDirection, velocity = motor.Velocity,
                projection = Vector3.Dot(motor.Velocity - motor.AirNetDelta, motor.LastDesiredDirection),
                request = motor.AirRequestedDelta, applied = motor.AirAppliedWishDelta, net = motor.AirNetDelta,
                contact = motor.ContactVelocityDelta, ground = motor.IsGrounded, surf = motor.IsSurfing,
                limit = motor.AirEnergyLimited, safety = motor.SafetyLimited };
        }
        private void OnApplicationPause(bool paused) { if (paused) Flush(); }
        private void OnDestroy() { Flush(); if (motor != null) motor.Simulated -= Capture; }
        private void Flush()
        {
            if (count == 0 || string.IsNullOrEmpty(scope)) return;
            try
            {
                var csv = new StringBuilder("seconds,yaw,inputX,inputY,posX,posY,posZ,wishX,wishZ,velX,velY,velZ,projected,requestedDv,appliedWishDv,netX,netZ,contactX,contactY,contactZ,ground,surf,energyLimit,safetyLimit\n");
                for (int i = Mathf.Max(0,count-samples.Length); i < count; i++)
                {
                    var s = samples[i % samples.Length];
                    csv.AppendFormat(CultureInfo.InvariantCulture,
                        "{0:F4},{1:F3},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F3},{8:F3},{9:F3},{10:F3},{11:F3},{12:F3},{13:F4},{14:F4},{15:F4},{16:F4},{17:F4},{18:F4},{19:F4},{20},{21},{22},{23}\n",
                        s.time,s.yaw,s.input.x,s.input.y,s.position.x,s.position.y,s.position.z,s.wish.x,s.wish.z,
                        s.velocity.x,s.velocity.y,s.velocity.z,s.projection,s.request,s.applied,s.net.x,s.net.z,
                        s.contact.x,s.contact.y,s.contact.z,s.ground,s.surf,s.limit,s.safety);
                }
                var directory = Path.Combine(Application.persistentDataPath,"MovementTrials");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory,scope+"-"+profile+".csv"),csv.ToString());
            }
            catch (Exception e) { Debug.LogWarning("Movement trace unavailable: " + e.Message); }
        }
    }
}
