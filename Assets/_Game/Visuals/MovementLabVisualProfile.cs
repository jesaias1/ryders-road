using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Movement Lab Visual Profile",
        fileName = ResourceName)]
    public sealed class MovementLabVisualProfile : ScriptableObject
    {
        public const string ResourceName = "MovementLabVisualProfile";

        [Header("Atmosphere")]
        [SerializeField] private Color _skyHorizon = new Color(0.42f, 0.68f, 1f);
        [SerializeField] private Color _skyFog = new Color(0.48f, 0.34f, 0.82f);
        [SerializeField] private Color _ambientLight = new Color(0.58f, 0.64f, 0.82f);
        [SerializeField] private Color _sunLight = new Color(1f, 0.93f, 0.78f);
        [SerializeField] private Color _void = new Color(0.035f, 0.025f, 0.09f);

        [Header("Gameplay roles")]
        [SerializeField] private Color _normalPlatform = new Color(0.78f, 0.86f, 0.96f);
        [SerializeField] private Color _precisionPlatform = new Color(0.42f, 0.92f, 0.74f);
        [SerializeField] private Color _movingPlatform = new Color(0.24f, 0.66f, 1f);
        [SerializeField] private Color _restorePoint = new Color(0.1f, 0.92f, 1f);
        [SerializeField] private Color _patchBlock = new Color(1f, 0.64f, 0.12f);
        [SerializeField] private Color _straightTestPlatform = new Color(0.88f, 0.88f, 0.72f);
        [SerializeField] private Color _centerLine = new Color(1f, 1f, 1f);
        [SerializeField] private Color _boundaryLine = new Color(0.08f, 0.08f, 0.16f);
        [SerializeField] private Color _markerLine = new Color(0.16f, 0.95f, 1f);
        [SerializeField] private Color _surfSurface = new Color(0.08f, 0.82f, 0.92f);
        [SerializeField] private Color _underside = new Color(0.18f, 0.2f, 0.32f);

        private Material[] _materials;

        public Color SkyHorizon => _skyHorizon;
        public Color SkyFog => _skyFog;
        public Color AmbientLight => _ambientLight;
        public Color SunLight => _sunLight;
        public Color Void => _void;

        public static MovementLabVisualProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<MovementLabVisualProfile>();
            profile.name = ResourceName + "_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }

        public Color ColorFor(MovementLabMaterialRole role)
        {
            switch (role)
            {
                case MovementLabMaterialRole.NormalPlatform:
                    return _normalPlatform;
                case MovementLabMaterialRole.PrecisionPlatform:
                    return _precisionPlatform;
                case MovementLabMaterialRole.MovingPlatform:
                    return _movingPlatform;
                case MovementLabMaterialRole.RestorePoint:
                    return _restorePoint;
                case MovementLabMaterialRole.PatchBlock:
                    return _patchBlock;
                case MovementLabMaterialRole.NullSpace:
                    return _void;
                case MovementLabMaterialRole.StraightTestPlatform:
                    return _straightTestPlatform;
                case MovementLabMaterialRole.CenterLine:
                    return _centerLine;
                case MovementLabMaterialRole.BoundaryLine:
                    return _boundaryLine;
                case MovementLabMaterialRole.MarkerLine:
                    return _markerLine;
                case MovementLabMaterialRole.SurfSurface:
                    return _surfSurface;
                case MovementLabMaterialRole.Underside:
                    return _underside;
                default:
                    return _normalPlatform;
            }
        }

        public Material MaterialFor(MovementLabMaterialRole role)
        {
            _materials ??= new Material[System.Enum.GetValues(typeof(MovementLabMaterialRole)).Length];
            var index = (int)role;
            if (_materials[index] == null)
            {
                _materials[index] = VisualMaterialUtility.CreateRuntimeMaterial(
                    "RB " + role,
                    ColorFor(role));
            }

            return _materials[index];
        }
    }
}
