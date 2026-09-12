using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    public enum ModuleVisualQualityTier
    {
        MobileLow = 0,
        MobileBalanced = 1,
        HeroPreview = 2
    }

    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Module Environment Profile",
        fileName = ResourceName)]
    public sealed class ModuleEnvironmentProfile : ScriptableObject
    {
        public const string ResourceName = "ModuleEnvironmentProfile";

        [SerializeField] private Color _skyZenith = new Color(0.08f, 0.35f, 0.85f);
        [SerializeField] private Color _skyMid = new Color(0.18f, 0.68f, 0.98f);
        [SerializeField] private Color _skyHorizon = new Color(0.68f, 0.86f, 0.98f);
        [SerializeField] private Color _skyWarmth = new Color(1f, 0.72f, 0.45f);
        [SerializeField] private Color _fogColor = new Color(0.65f, 0.82f, 0.96f);
        [SerializeField] private Color _ambientLight = new Color(0.72f, 0.82f, 0.96f);
        [SerializeField] private Color _sunLight = new Color(1f, 0.94f, 0.84f);
        [SerializeField] private float _fogDensity = 0.0018f;
        [SerializeField] private float _sunIntensity = 1.6f;
        [SerializeField] private float _nullSpaceY = -22f;
        [SerializeField] private float _cloudDensity = 1f;
        [SerializeField] private float _decorationDensity = 1f;
        [SerializeField] private float _vfxDensity = 1f;
        [SerializeField] private ModuleVisualQualityTier _qualityProfile =
            ModuleVisualQualityTier.MobileBalanced;
        [SerializeField] private float _distantIslandDensity = 1f;
        [SerializeField] private float _horizonLayerOpacity = 0.72f;

        [SerializeField] private AudioClip _ambienceClip;
        [SerializeField, Range(0,1)] private float _ambienceGain = .07f;
        public AudioClip AmbienceClip => _ambienceClip;
        public float AmbienceGain => _ambienceGain;
        [SerializeField] private bool _authoredLighting;
        [SerializeField] private Vector3 _sunEuler = new Vector3(48,-32,0);
        public bool AuthoredLighting => _authoredLighting;
        [SerializeField] private bool _nearArchitectureShadows;
        public bool NearArchitectureShadows => _nearArchitectureShadows;
        [SerializeField] private bool _cohesiveLighting;
        [SerializeField] private Color _ambientGround = new Color(.09f,.14f,.19f);
        [SerializeField, Range(0,1)] private float _shadowStrength = .75f;
        [SerializeField, Range(0,2)] private float _shadowBias = .25f;
        [SerializeField, Range(0,3)] private float _shadowNormalBias = .35f;
        [SerializeField, Range(-2,2)] private float _postExposure;
        [SerializeField, Range(-30,30)] private float _contrast = 8;
        [SerializeField, Range(-30,30)] private float _saturation;
        [SerializeField, Range(0,.5f)] private float _bloomIntensity = .08f;
        [SerializeField, Range(1,3)] private float _bloomThreshold = 1.3f;
        public bool CohesiveLighting => _cohesiveLighting;
        public Color AmbientGround => _ambientGround;
        public float ShadowStrength => _shadowStrength;
        public float ShadowBias => _shadowBias;
        public float ShadowNormalBias => _shadowNormalBias;
        public float PostExposure => _postExposure;
        public float Contrast => _contrast;
        public float Saturation => _saturation;
        public float BloomIntensity => _bloomIntensity;
        public float BloomThreshold => _bloomThreshold;
        public Vector3 SunEuler => _sunEuler;
        [SerializeField] private Material _skyboxMaterial;
        [SerializeField] private bool _overrideBiomeFog;
        public Material SkyboxMaterial => _skyboxMaterial;
        public bool OverrideBiomeFog => _overrideBiomeFog;

        public Color SkyZenith => _skyZenith;
        public Color SkyMid => _skyMid;
        public Color SkyHorizon => _skyHorizon;
        public Color SkyWarmth => _skyWarmth;
        public Color FogColor => _fogColor;
        public Color AmbientLight => _ambientLight;
        public Color SunLight => _sunLight;
        public float FogDensity => _fogDensity;
        public float SunIntensity => _sunIntensity;
        public float NullSpaceY => _nullSpaceY;
        public float CloudDensity => _cloudDensity;
        public float DecorationDensity => _decorationDensity;
        public float VfxDensity => _vfxDensity;
        public ModuleVisualQualityTier QualityProfile => _qualityProfile;
        public float DistantIslandDensity => _distantIslandDensity;
        public float HorizonLayerOpacity => _horizonLayerOpacity;

        public static ModuleEnvironmentProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<ModuleEnvironmentProfile>();
            profile.name = ResourceName + "_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }

        public void ApplyVisualSliceDefaults() => ApplyVisualIdentityDefaults();

        public void ApplyVisualIdentityDefaults()
        {
            _skyZenith = new Color(0.12f, 0.34f, 0.78f);
            _skyMid = new Color(0.04f, 0.58f, 0.95f);
            _skyHorizon = new Color(0.65f, 0.82f, 0.96f);
            _skyWarmth = new Color(1f, 0.6f, 0.28f);
            _fogColor = new Color(0.65f, 0.82f, 0.96f);
            _ambientLight = new Color(0.72f, 0.82f, 0.96f);
            _sunLight = new Color(1f, 0.88f, 0.72f);
            _fogDensity = 0.0018f;
            _sunIntensity = 1.74f;
            _nullSpaceY = -26f;
            _cloudDensity = 1f;
            _decorationDensity = 1f;
            _vfxDensity = 0.85f;
            _qualityProfile = ModuleVisualQualityTier.MobileBalanced;
            _distantIslandDensity = 1f;
            _horizonLayerOpacity = 0.82f;
        }
    }
}
