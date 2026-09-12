using System;
using System.Collections.Generic;
using UnityEngine;

namespace Avoidance.Gameplay.Worlds
{
    public enum EnvironmentBiomeKind
    {
        SkyCity,
        MountainSky,
        AncientAbyss,
        EnergyVoid,
        SolarFoundry,
        Windward
    }

    public enum BiomeDepthBand
    {
        Gameplay = 0,
        NearEnvironment = 1,
        MidWorld = 2,
        FarWorld = 3,
        LowerAbyss = 4
    }

    [Serializable]
    public sealed class BiomeWorldObject
    {
        [SerializeField] private string _stableId;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _eulerAngles;
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private bool _staticBatch = true;
        [SerializeField, Tooltip("Retain mesh collision; this does not authorize traversal.")] private bool _hasPlayableArchitecture;
        [SerializeField, Tooltip("Explicit route opportunity. Otherwise retained scenery collision is fatal.")] private bool _traversableArchitecture;
        [SerializeField] private BiomeDepthBand _depthBand = BiomeDepthBand.MidWorld;
        [SerializeField] private float _depthFadeStrength = 1f;

        public BiomeWorldObject(
            string stableId,
            GameObject prefab,
            Vector3 position,
            Vector3 eulerAngles,
            Vector3 scale,
            bool staticBatch = true,
            BiomeDepthBand depthBand = BiomeDepthBand.MidWorld,
            float depthFadeStrength = 1f,
            bool hasPlayableArchitecture = false,
            bool traversableArchitecture = false)
        {
            _hasPlayableArchitecture = hasPlayableArchitecture;
            _traversableArchitecture = traversableArchitecture;
            _stableId = stableId;
            _prefab = prefab;
            _position = position;
            _eulerAngles = eulerAngles;
            _scale = scale;
            _staticBatch = staticBatch;
            _depthBand = depthBand;
            _depthFadeStrength = depthFadeStrength;
        }

        public string StableId => _stableId;
        public GameObject Prefab => _prefab;
        public Vector3 Position => _position;
        public Vector3 EulerAngles => _eulerAngles;
        public Quaternion Rotation => Quaternion.Euler(_eulerAngles);
        public Vector3 Scale => _scale == Vector3.zero ? Vector3.one : _scale;
        public bool HasPlayableArchitecture => _hasPlayableArchitecture;
        // Historical "playable" flag means mesh collision, not route authorization.
        public WorldGeometryKind GeometryKind => !_hasPlayableArchitecture ? WorldGeometryKind.NonCollidingScenery
            : _traversableArchitecture ? WorldGeometryKind.Traversable : WorldGeometryKind.FatalScenery;
        public bool StaticBatch => _staticBatch;
        public BiomeDepthBand DepthBand => _depthBand;
        public float DepthFadeStrength => Mathf.Clamp01(_depthFadeStrength);
        public bool IsValid => !string.IsNullOrWhiteSpace(_stableId) && _prefab != null;
    }

    [Serializable]
    public sealed class BiomeMistLayer
    {
        [SerializeField] private string _stableId;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private float _alpha = 0.22f;
        [SerializeField] private float _motionRadius;
        [SerializeField] private float _motionSpeed;

        public BiomeMistLayer(
            string stableId,
            Vector3 position,
            Vector3 scale,
            float alpha,
            float motionRadius = 0f,
            float motionSpeed = 0f)
        {
            _stableId = stableId;
            _position = position;
            _scale = scale;
            _alpha = alpha;
            _motionRadius = motionRadius;
            _motionSpeed = motionSpeed;
        }

        public string StableId => _stableId;
        public Vector3 Position => _position;
        public Vector3 Scale => _scale == Vector3.zero ? Vector3.one : _scale;
        public float Alpha => Mathf.Clamp01(_alpha);
        public float MotionRadius => Mathf.Max(0f, _motionRadius);
        public float MotionSpeed => Mathf.Max(0f, _motionSpeed);
        public bool IsValid => !string.IsNullOrWhiteSpace(_stableId);
    }

    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Environment Biome Profile",
        fileName = ResourceName)]
    public sealed class EnvironmentBiomeProfile : ScriptableObject
    {
        public const string ResourceName = "EnvironmentBiomes";

        [SerializeField] private bool _combineStaticGeometry;
        public bool CombineStaticGeometry => _combineStaticGeometry;

        [SerializeField] private string _biomeId = "biome.sky-city";
        [SerializeField] private string _displayName = "Sky City";
        [SerializeField] private EnvironmentBiomeKind _kind = EnvironmentBiomeKind.SkyCity;
        [SerializeField] private Color _atmosphereTint = new Color(0.62f, 0.82f, 0.98f, 1f);
        [SerializeField] private Color _mistColor = new Color(0.78f, 0.9f, 1f, 0.24f);
        [SerializeField] private Color _hazeColor = new Color(0.38f, 0.58f, 0.85f, 0.18f);
        [SerializeField] private float _mistOceanY = -38f;
        [SerializeField] private BiomeMistLayer[] _mistLayers = Array.Empty<BiomeMistLayer>();
        [SerializeField] private BiomeWorldObject[] _worldObjects = Array.Empty<BiomeWorldObject>();

        public string BiomeId => _biomeId;
        public string DisplayName => _displayName;
        public EnvironmentBiomeKind Kind => _kind;
        public Color AtmosphereTint => _atmosphereTint;
        public Color MistColor => _mistColor;
        public Color HazeColor => _hazeColor;
        public float MistOceanY => _mistOceanY;
        public IReadOnlyList<BiomeMistLayer> MistLayers => _mistLayers;
        public IReadOnlyList<BiomeWorldObject> WorldObjects => _worldObjects;

        public void Configure(
            string biomeId,
            string displayName,
            EnvironmentBiomeKind kind,
            Color atmosphereTint,
            Color mistColor,
            Color hazeColor,
            float mistOceanY,
            BiomeMistLayer[] mistLayers,
            BiomeWorldObject[] worldObjects)
        {
            _biomeId = biomeId;
            _displayName = displayName;
            _kind = kind;
            _atmosphereTint = atmosphereTint;
            _mistColor = mistColor;
            _hazeColor = hazeColor;
            _mistOceanY = mistOceanY;
            _mistLayers = mistLayers ?? Array.Empty<BiomeMistLayer>();
            _worldObjects = worldObjects ?? Array.Empty<BiomeWorldObject>();
        }
    }
}
