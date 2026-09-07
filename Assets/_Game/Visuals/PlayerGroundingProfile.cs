using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Player Grounding Profile",
        fileName = ResourceName)]
    public sealed class PlayerGroundingProfile : ScriptableObject
    {
        public const string ResourceName = "PlayerGroundingProfile";

        [Range(0.2f, 0.8f)] [SerializeField] private float _radius = 0.42f;
        [Range(0.5f, 8f)] [SerializeField] private float _maximumSurfaceDistance = 4.2f;
        [Range(0f, 1f)] [SerializeField] private float _fullOpacityHeight = 0.16f;
        [Range(0.1f, 8f)] [SerializeField] private float _fadeOutHeight = 3.6f;
        [Range(0.001f, 0.05f)] [SerializeField] private float _surfaceOffset = 0.012f;
        [Range(0f, 1f)] [SerializeField] private float _minimumSurfaceNormalY = 0.55f;
        [Range(8, 24)] [SerializeField] private int _segmentCount = 16;
        [SerializeField] private Color _shadowColor = new Color(0.025f, 0.04f, 0.09f, 0.3f);

        public float Radius => Mathf.Max(0.05f, _radius);
        public float MaximumSurfaceDistance => Mathf.Max(0.1f, _maximumSurfaceDistance);
        public float FullOpacityHeight => Mathf.Max(0f, _fullOpacityHeight);
        public float FadeOutHeight => Mathf.Max(FullOpacityHeight + 0.05f, _fadeOutHeight);
        public float SurfaceOffset => Mathf.Max(0.001f, _surfaceOffset);
        public float MinimumSurfaceNormalY => Mathf.Clamp01(_minimumSurfaceNormalY);
        public int SegmentCount => Mathf.Clamp(_segmentCount, 8, 24);
        public Color ShadowColor => _shadowColor;

        public static PlayerGroundingProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<PlayerGroundingProfile>();
            profile.name = ResourceName + "_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }
    }
}
