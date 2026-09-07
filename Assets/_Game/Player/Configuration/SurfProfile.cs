using UnityEngine;

namespace Avoidance.Gameplay.Player
{
    [CreateAssetMenu(menuName = "RYDERS BLOCK/Surf Profile", fileName = "Surf_Profile")]
    public sealed class SurfProfile : ScriptableObject
    {
        [SerializeField] private string _profileId = "surf.default";
        [Range(0f, 89f)] [SerializeField] private float _minimumSurfAngle = 28f;
        [Range(0f, 89f)] [SerializeField] private float _maximumSurfAngle = 78f;
        [Min(0f)] [SerializeField] private float _surfGravity = 20f;
        [Min(0f)] [SerializeField] private float _surfAcceleration = 12f;
        [Min(.1f)] [SerializeField] private float _surfWishSpeed = 8.2f;
        public float SurfWishSpeed => _surfWishSpeed;
        [Min(0f)] [SerializeField] private float _surfFriction = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float _surfControl = 0.72f;
        [Min(0.1f)] [SerializeField] private float _maximumSurfSpeed = 18f;
        [Range(0f, 1f)] [SerializeField] private float _exitMomentumRetention = 0.95f;
        [Range(0f, 2f)] [SerializeField] private float _surfJumpInfluence = 1f;

        public string ProfileId => _profileId;
        public float MinimumSurfAngle => Mathf.Min(_minimumSurfAngle, _maximumSurfAngle);
        public float MaximumSurfAngle => Mathf.Max(_minimumSurfAngle, _maximumSurfAngle);
        public float SurfGravity => _surfGravity;
        public float SurfAcceleration => _surfAcceleration;
        public float SurfFriction => _surfFriction;
        public float SurfControl => _surfControl;
        public float MaximumSurfSpeed => _maximumSurfSpeed;
        public float ExitMomentumRetention => _exitMomentumRetention;
        public float SurfJumpInfluence => _surfJumpInfluence;

        public bool IsValidAngle(float angle)
        {
            return angle >= MinimumSurfAngle && angle <= MaximumSurfAngle;
        }

        public static SurfProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<SurfProfile>();
            profile.name = "Surf_Default_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }
    }
}
