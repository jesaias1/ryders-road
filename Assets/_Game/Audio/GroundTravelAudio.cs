using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Audio
{
    // Presentation observes the motor, never writes to it. Ground travel excludes idle/platform carry.
    public sealed class GroundTravelAudio : MonoBehaviour
    {
        private ParkourMotor _motor;
        private MovementFeedback _feedback;
        private GameplayAudioProfile _profile;
        private float _distance;
        public void Initialize(ParkourMotor motor,MovementFeedback feedback)
        { _motor=motor;_feedback=feedback;_profile=Resources.Load<GameplayAudioProfile>("GameplayAudioProfile"); }
        public void Tick(float dt)
        {
            if(_motor==null || _profile==null || dt<=0) return;
            if(!_motor.IsGrounded || _motor.HorizontalSpeed<.5f){_distance=0;return;}
            _distance+=_motor.HorizontalSpeed*dt;
            if(_distance<_profile.FootstepDistance)return;
            _distance=0;_feedback.PlayCue(GameplayAudioCue.Footstep);
        }
        private void Update()=>Tick(Time.deltaTime);
        private void OnDisable()=>_distance=0;
    }
}
