using UnityEngine;

namespace Avoidance.Gameplay.Visuals
{
    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Module Visual Profile",
        fileName = ResourceName)]
    public sealed class ModuleVisualProfile : ScriptableObject
    {
        public const string ResourceName = "ModuleVisualProfile";

        [Header("Gameplay roles")]
        [SerializeField] private Color _normal = new Color(0.82f, 0.88f, 0.96f);
        [SerializeField] private Color _precision = new Color(0.58f, 0.94f, 0.82f);
        [SerializeField] private Color _moving = new Color(0.22f, 0.58f, 0.95f);
        [SerializeField] private Color _restore = new Color(0.08f, 0.92f, 1f);
        [SerializeField] private Color _patch = new Color(1f, 0.62f, 0.08f);
        [SerializeField] private Color _boost = new Color(1f, 0.82f, 0.08f);
        [SerializeField] private Color _water = new Color(0.08f, 0.72f, 1f, 0.72f);
        [SerializeField] private Color _crumbling = new Color(0.94f, 0.34f, 0.18f);
        [SerializeField] private Color _shortcutCue = new Color(1f, 0.24f, 0.42f);
        [SerializeField] private Color _nullSpace = new Color(0.035f, 0.022f, 0.085f);
        [SerializeField] private Color _underside = new Color(0.17f, 0.19f, 0.31f);
        [SerializeField] private Color _edge = new Color(0.08f, 0.12f, 0.22f);
        [SerializeField] private Color _towerCore = new Color(0.33f, 0.37f, 0.52f);
        [SerializeField] private Color _decorationStone = new Color(0.46f, 0.52f, 0.68f);
        [SerializeField] private Color _vegetation = new Color(0.16f, 0.68f, 0.26f);
        [SerializeField] private Color _cloud = new Color(0.88f, 0.91f, 1f, 0.72f);
        [SerializeField] private Color _stoneSide = new Color(0.48f, 0.55f, 0.74f);
        [SerializeField] private Color _waterFoam = new Color(0.72f, 1f, 1f, 0.84f);
        [SerializeField] private Color _patchCore = new Color(1f, 0.92f, 0.32f);
        [SerializeField] private Color _boostArrow = new Color(1f, 0.98f, 0.58f);
        [SerializeField] private Color _movingAccent = new Color(0.1f, 0.92f, 1f);
        [SerializeField] private Color _crumbleFault = new Color(1f, 0.22f, 0.12f);
        [SerializeField] private Color _corruption = new Color(0.62f, 0.12f, 1f);
        [SerializeField] private Color _gloveDark = new Color(0.06f, 0.08f, 0.16f);
        [SerializeField] private Color _gloveAccent = new Color(1f, 0.58f, 0.08f);
        [SerializeField] private Color _surfaceHighlight = new Color(0.94f, 0.98f, 1f);
        [SerializeField] private Color _surfaceInset = new Color(0.7f, 0.78f, 0.92f);
        [SerializeField] private Color _circuitLine = new Color(0.14f, 0.9f, 1f);
        [SerializeField] private Color _warmAccent = new Color(1f, 0.62f, 0.12f);
        [SerializeField] private Color _flower = new Color(1f, 0.24f, 0.48f);
        [SerializeField] private Color _surf = new Color(0.04f, 0.86f, 0.92f, 0.82f);

        [Header("HUD and rank colors")]
        [SerializeField] private Color _hudPanel = new Color(0.02f, 0.04f, 0.1f, 0.56f);
        [SerializeField] private Color _hudText = new Color(0.94f, 0.98f, 1f);
        [SerializeField] private Color _bronze = new Color(0.9f, 0.46f, 0.18f);
        [SerializeField] private Color _silver = new Color(0.78f, 0.88f, 0.94f);
        [SerializeField] private Color _gold = new Color(1f, 0.78f, 0.08f);
        [SerializeField] private Color _diamond = new Color(0.22f, 0.9f, 1f);
        [SerializeField] private float _hudPanelOpacity = 0.58f;

        [Header("Reusable presentation kit")]
        [SerializeField] private float _surfaceInsetScale = 0.72f;
        [SerializeField] private float _edgeTrimScale = 0.07f;
        [SerializeField] private float _heroAccentDensity = 1f;

        private Material[] _materials;
        private Texture2D _routeStoneTexture;
        private Texture2D _goldRouteTexture;
        private Texture2D _cyanEnergyTexture;

        public static ModuleVisualProfile CreateRuntimeDefault()
        {
            var profile = CreateInstance<ModuleVisualProfile>();
            profile.name = ResourceName + "_Runtime";
            profile.hideFlags = HideFlags.DontSave;
            return profile;
        }

        public void ApplyVisualSliceDefaults() => ApplyVisualIdentityDefaults();

        public void ApplyVisualIdentityDefaults()
        {
            _normal = new Color(0.86f, 0.88f, 0.91f);
            _precision = new Color(0.63f, 0.96f, 0.84f);
            _moving = new Color(0.12f, 0.38f, 0.82f);
            _restore = new Color(0.08f, 0.92f, 1f);
            _patch = new Color(1f, 0.48f, 0.03f);
            _boost = new Color(1f, 0.66f, 0.04f);
            _water = new Color(0.02f, 0.74f, 1f, 0.62f);
            _crumbling = new Color(0.86f, 0.36f, 0.2f);
            _shortcutCue = new Color(1f, 0.22f, 0.46f);
            _nullSpace = new Color(0.025f, 0.018f, 0.075f);
            _underside = new Color(0.16f, 0.18f, 0.28f);
            _edge = new Color(0.08f, 0.11f, 0.19f);
            _towerCore = new Color(0.3f, 0.34f, 0.48f);
            _decorationStone = new Color(0.54f, 0.58f, 0.72f);
            _vegetation = new Color(0.08f, 0.56f, 0.36f);
            _cloud = new Color(0.96f, 0.92f, 1f, 0.56f);
            _stoneSide = new Color(0.44f, 0.5f, 0.66f);
            _waterFoam = new Color(0.72f, 1f, 1f, 0.86f);
            _patchCore = new Color(1f, 0.92f, 0.32f);
            _boostArrow = new Color(1f, 0.98f, 0.58f);
            _movingAccent = new Color(0.12f, 0.9f, 1f);
            _crumbleFault = new Color(1f, 0.24f, 0.08f);
            _corruption = new Color(0.42f, 0.1f, 0.78f, 0.5f);
            _gloveDark = new Color(0.055f, 0.07f, 0.15f);
            _gloveAccent = new Color(1f, 0.52f, 0.06f);
            _surfaceHighlight = new Color(0.98f, 0.97f, 0.9f);
            _surfaceInset = new Color(0.66f, 0.72f, 0.86f);
            _circuitLine = new Color(0.1f, 0.94f, 1f);
            _warmAccent = new Color(1f, 0.52f, 0.06f);
            _flower = new Color(1f, 0.24f, 0.48f);
            _surf = new Color(0.04f, 0.86f, 0.92f, 0.82f);
            _hudPanel = new Color(0.015f, 0.025f, 0.07f, 0.58f);
            _hudText = new Color(0.94f, 0.98f, 1f);
            _bronze = new Color(0.88f, 0.43f, 0.15f);
            _silver = new Color(0.78f, 0.88f, 0.94f);
            _gold = new Color(1f, 0.78f, 0.08f);
            _diamond = new Color(0.22f, 0.9f, 1f);
            _hudPanelOpacity = 0.58f;
            _surfaceInsetScale = 0.7f;
            _edgeTrimScale = 0.08f;
            _heroAccentDensity = 1f;
            _materials = null;
        }

        public Color ColorFor(ModuleMaterialRole role)
        {
            switch (role)
            {
                case ModuleMaterialRole.Normal: return _normal;
                case ModuleMaterialRole.Precision: return _precision;
                case ModuleMaterialRole.Moving: return _moving;
                case ModuleMaterialRole.Restore: return _restore;
                case ModuleMaterialRole.Patch: return _patch;
                case ModuleMaterialRole.Boost: return _boost;
                case ModuleMaterialRole.Water: return _water;
                case ModuleMaterialRole.Crumbling: return _crumbling;
                case ModuleMaterialRole.ShortcutCue: return _shortcutCue;
                case ModuleMaterialRole.NullSpace: return _nullSpace;
                case ModuleMaterialRole.Underside: return _underside;
                case ModuleMaterialRole.Edge: return _edge;
                case ModuleMaterialRole.TowerCore: return _towerCore;
                case ModuleMaterialRole.DecorationStone: return _decorationStone;
                case ModuleMaterialRole.Vegetation: return _vegetation;
                case ModuleMaterialRole.Cloud: return _cloud;
                case ModuleMaterialRole.StoneSide: return _stoneSide;
                case ModuleMaterialRole.WaterFoam: return _waterFoam;
                case ModuleMaterialRole.PatchCore: return _patchCore;
                case ModuleMaterialRole.BoostArrow: return _boostArrow;
                case ModuleMaterialRole.MovingAccent: return _movingAccent;
                case ModuleMaterialRole.CrumbleFault: return _crumbleFault;
                case ModuleMaterialRole.Corruption: return _corruption;
                case ModuleMaterialRole.GloveDark: return _gloveDark;
                case ModuleMaterialRole.GloveAccent: return _gloveAccent;
                case ModuleMaterialRole.SurfaceHighlight: return _surfaceHighlight;
                case ModuleMaterialRole.SurfaceInset: return _surfaceInset;
                case ModuleMaterialRole.CircuitLine: return _circuitLine;
                case ModuleMaterialRole.WarmAccent: return _warmAccent;
                case ModuleMaterialRole.Flower: return _flower;
                case ModuleMaterialRole.Surf: return _surf;
                default: return _normal;
            }
        }

        public Color HudPanelColor => new Color(
            _hudPanel.r,
            _hudPanel.g,
            _hudPanel.b,
            Mathf.Clamp01(_hudPanelOpacity));
        public Color HudTextColor => _hudText;
        public float SurfaceInsetScale => Mathf.Clamp(_surfaceInsetScale, 0.45f, 0.92f);
        public float EdgeTrimScale => Mathf.Clamp(_edgeTrimScale, 0.035f, 0.12f);
        public float HeroAccentDensity => Mathf.Clamp01(_heroAccentDensity);
        public Color RankColor(Avoidance.Gameplay.Ranking.ModuleRank rank)
        {
            switch (rank)
            {
                case Avoidance.Gameplay.Ranking.ModuleRank.Bronze: return _bronze;
                case Avoidance.Gameplay.Ranking.ModuleRank.Silver: return _silver;
                case Avoidance.Gameplay.Ranking.ModuleRank.Gold: return _gold;
                case Avoidance.Gameplay.Ranking.ModuleRank.Diamond: return _diamond;
                default: return _hudText;
            }
        }

        public Material MaterialFor(ModuleMaterialRole role)
        {
            _materials ??= new Material[System.Enum.GetValues(typeof(ModuleMaterialRole)).Length];
            var index = (int)role;
            if (_materials[index] == null)
            {
                var color = ColorFor(role);
                if (role == ModuleMaterialRole.Water
                    || role == ModuleMaterialRole.Cloud
                    || role == ModuleMaterialRole.WaterFoam
                    || role == ModuleMaterialRole.Surf)
                {
                    _materials[index] = ShouldUseCyanEnergyTexture(role)
                        ? VisualMaterialUtility.CreateRuntimeTransparentTexturedMaterial(
                            "RB Module " + role,
                            color,
                            CyanEnergyTexture,
                            Vector2.one)
                        : VisualMaterialUtility.CreateRuntimeTransparentMaterial(
                            "RB Module " + role,
                            color);
                }
                else if (role == ModuleMaterialRole.Restore
                    || role == ModuleMaterialRole.PatchCore
                    || role == ModuleMaterialRole.BoostArrow
                    || role == ModuleMaterialRole.MovingAccent
                    || role == ModuleMaterialRole.CrumbleFault
                    || role == ModuleMaterialRole.Corruption
                    || role == ModuleMaterialRole.CircuitLine
                    || role == ModuleMaterialRole.WarmAccent)
                {
                    var texture = ShouldUseGoldRouteTexture(role)
                        ? GoldRouteTexture
                        : ShouldUseCyanEnergyTexture(role)
                            ? CyanEnergyTexture
                            : null;
                    _materials[index] = texture != null
                        ? VisualMaterialUtility.CreateRuntimeEmissiveTexturedMaterial(
                            "RB Module " + role,
                            color,
                            texture,
                            Vector2.one)
                        : VisualMaterialUtility.CreateRuntimeEmissiveMaterial(
                            "RB Module " + role,
                            color);
                }
                else
                {
                    _materials[index] = ShouldUseGoldRouteTexture(role)
                        ? VisualMaterialUtility.CreateRuntimeTexturedMaterial(
                            "RB Module " + role,
                            color,
                            GoldRouteTexture,
                            Vector2.one)
                        : ShouldUseRouteStoneTexture(role)
                            ? VisualMaterialUtility.CreateRuntimeTexturedMaterial(
                                "RB Module " + role,
                                color,
                                RouteStoneTexture,
                                Vector2.one)
                            : VisualMaterialUtility.CreateRuntimeMaterial(
                                "RB Module " + role,
                                color);
                }
            }

            return _materials[index];
        }

        private Texture2D RouteStoneTexture =>
            _routeStoneTexture ??= Resources.Load<Texture2D>("Textures/RydersRoad_RouteStone_02")
                ?? Resources.Load<Texture2D>("Textures/RydersRoad_RouteStone_01");

        private Texture2D GoldRouteTexture =>
            _goldRouteTexture ??= Resources.Load<Texture2D>("Textures/RydersRoad_GoldRoute_01");

        private Texture2D CyanEnergyTexture =>
            _cyanEnergyTexture ??= Resources.Load<Texture2D>("Textures/RydersRoad_CyanEnergy_01");

        private static bool ShouldUseRouteStoneTexture(ModuleMaterialRole role) =>
            role == ModuleMaterialRole.Normal
            || role == ModuleMaterialRole.Precision
            || role == ModuleMaterialRole.SurfaceHighlight
            || role == ModuleMaterialRole.DecorationStone
            || role == ModuleMaterialRole.StoneSide;

        private static bool ShouldUseGoldRouteTexture(ModuleMaterialRole role) =>
            role == ModuleMaterialRole.Boost
            || role == ModuleMaterialRole.Patch
            || role == ModuleMaterialRole.WarmAccent
            || role == ModuleMaterialRole.GloveAccent;

        private static bool ShouldUseCyanEnergyTexture(ModuleMaterialRole role) =>
            role == ModuleMaterialRole.Water
            || role == ModuleMaterialRole.WaterFoam
            || role == ModuleMaterialRole.Surf
            || role == ModuleMaterialRole.Restore
            || role == ModuleMaterialRole.PatchCore
            || role == ModuleMaterialRole.MovingAccent
            || role == ModuleMaterialRole.CircuitLine;
    }
}
