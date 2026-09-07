using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.Gameplay.Visuals
{
    public enum VisualFitMode
    {
        Uniform = 0,
        Fixed = 1,
        ModularTile = 2,
        TopAlignedUniform = 3,
        ExactFootprint = 4
    }

    [Serializable]
    public readonly struct VisualPrefabPlacement
    {
        public VisualPrefabPlacement(
            GameObject prefab,
            Vector3 localPosition,
            Vector3 localEulerAngles,
            Vector3 localScale,
            VisualFitMode fitMode = VisualFitMode.Uniform)
        {
            Prefab = prefab;
            LocalPosition = localPosition;
            LocalEulerAngles = localEulerAngles;
            LocalScale = localScale;
            FitMode = fitMode;
        }

        public GameObject Prefab { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalEulerAngles { get; }
        public Vector3 LocalScale { get; }
        public VisualFitMode FitMode { get; }
        public bool IsUniformOnly => FitMode == VisualFitMode.Uniform;
        public bool IsValid => Prefab != null;
    }

    [CreateAssetMenu(
        menuName = "RYDERS BLOCK/Module Visual Prefab Library",
        fileName = ResourceName)]
    public sealed class ModuleVisualPrefabLibrary : ScriptableObject
    {
        public const string ResourceName = "ModuleVisualPrefabLibrary";

        private static readonly VisualPrefabPlacement EmptyPlacement =
            new VisualPrefabPlacement(null, Vector3.zero, Vector3.zero, Vector3.one, VisualFitMode.Uniform);

        [SerializeField] private RolePrefab[] _rolePrefabs = Array.Empty<RolePrefab>();

        public static ModuleVisualPrefabLibrary CreateRuntimeDefault()
        {
            var library = CreateInstance<ModuleVisualPrefabLibrary>();
            library.name = ResourceName + "_Runtime";
            library.hideFlags = HideFlags.DontSave;
            return library;
        }

        public GameObject PrefabFor(ModuleMaterialRole role) =>
            PlacementFor(role, Vector3.one).Prefab;

        public VisualPrefabPlacement PlacementFor(ModuleMaterialRole role, Vector3 blockSize)
        {
            if (_rolePrefabs == null)
            {
                return EmptyPlacement;
            }

            var blockExtent = Mathf.Max(Mathf.Abs(blockSize.x), Mathf.Abs(blockSize.z));
            var minimumHorizontalExtent = Mathf.Max(
                0.01f,
                Mathf.Min(Mathf.Abs(blockSize.x), Mathf.Abs(blockSize.z)));
            var aspectRatio = blockExtent / minimumHorizontalExtent;
            RolePrefab? best = null;
            var bestSpecificity = float.MinValue;
            for (var index = 0; index < _rolePrefabs.Length; index++)
            {
                var candidate = _rolePrefabs[index];
                if (candidate.Role != role || candidate.Prefab == null)
                {
                    continue;
                }

                if (blockExtent + 0.001f < candidate.MinimumBlockExtent)
                {
                    continue;
                }

                if (aspectRatio + 0.001f < candidate.MinimumAspectRatio
                    || aspectRatio - 0.001f > candidate.MaximumAspectRatio)
                {
                    continue;
                }

                var specificity = candidate.MinimumBlockExtent * 10f
                    + candidate.MinimumAspectRatio;
                if (specificity >= bestSpecificity)
                {
                    best = candidate;
                    bestSpecificity = specificity;
                }
            }

            if (!best.HasValue)
            {
                return EmptyPlacement;
            }

            var resolved = best.Value;
            return new VisualPrefabPlacement(
                resolved.Prefab,
                resolved.LocalPosition,
                resolved.LocalEulerAngles,
                resolved.LocalScale,
                resolved.FitMode);
        }

        public bool HasPrefabFor(ModuleMaterialRole role) => PrefabFor(role) != null;

        public static Vector2Int ResolveModularTileCounts(Vector3 blockSize, float tileExtent = 1.5f)
        {
            var extent = Mathf.Max(0.01f, tileExtent);
            return new Vector2Int(
                Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(blockSize.x) / extent)),
                Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(blockSize.z) / extent)));
        }

        public static Vector3 ResolveTopAlignedUniformLocalScale(Vector3 blockSize)
        {
            var uniformWorldExtent = Mathf.Max(
                0.01f,
                Mathf.Min(Mathf.Abs(blockSize.x), Mathf.Abs(blockSize.z)));
            return new Vector3(
                uniformWorldExtent / Mathf.Max(0.01f, Mathf.Abs(blockSize.x)),
                uniformWorldExtent / Mathf.Max(0.01f, Mathf.Abs(blockSize.y)),
                uniformWorldExtent / Mathf.Max(0.01f, Mathf.Abs(blockSize.z)));
        }

        public static Vector3 ResolveExactFootprintLocalScale(
            Vector3 canonicalCorrection,
            Vector3 canonicalBlockSize)
        {
            return new Vector3(
                canonicalCorrection.x / Mathf.Max(0.01f, Mathf.Abs(canonicalBlockSize.x)),
                canonicalCorrection.y / Mathf.Max(0.01f, Mathf.Abs(canonicalBlockSize.y)),
                canonicalCorrection.z / Mathf.Max(0.01f, Mathf.Abs(canonicalBlockSize.z)));
        }

        public static void FitExactBounds(Transform visual, Transform support)
        {
            var bounds = new Bounds();
            var found = false;
            foreach (var filter in visual.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                var meshBounds = filter.sharedMesh.bounds;
                for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var point = support.InverseTransformPoint(filter.transform.TransformPoint(meshBounds.center
                        + Vector3.Scale(meshBounds.extents, new Vector3(x, y, z))));
                    if (!found) { bounds = new Bounds(point, Vector3.zero); found = true; } else bounds.Encapsulate(point);
                }
            }
            if (!found || bounds.size.x < 0.001f || bounds.size.y < 0.001f || bounds.size.z < 0.001f) return;
            var correction = new Vector3(1 / bounds.size.x, 1 / bounds.size.y, 1 / bounds.size.z);
            visual.localScale = Vector3.Scale(visual.localScale, correction);
            visual.localPosition = -Vector3.Scale(bounds.center - visual.localPosition, correction);
        }

        public static void PrepareVisualInstance(GameObject instance, bool preserveAuthoredSurfaces = false)
        {
            if (instance == null)
            {
                return;
            }

            foreach (var collider in instance.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                if (preserveAuthoredSurfaces && collider.GetComponent<Avoidance.Gameplay.Worlds.AuthoredSurface>() != null)
                {
                    collider.gameObject.layer = LayerMask.NameToLayer("Ground");
                    continue;
                }
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                renderer.allowOcclusionWhenDynamic = false;
            }
        }

        [Serializable]
        private struct RolePrefab
        {
            [SerializeField] private ModuleMaterialRole _role;
            [SerializeField] private GameObject _prefab;
            [SerializeField] private Vector3 _localPosition;
            [SerializeField] private Vector3 _localEulerAngles;
            [SerializeField] private Vector3 _localScale;
            [SerializeField] private float _minimumBlockExtent;
            [SerializeField] private float _minimumAspectRatio;
            [SerializeField] private float _maximumAspectRatio;
            [SerializeField] private VisualFitMode _fitMode;

            public ModuleMaterialRole Role => _role;
            public GameObject Prefab => _prefab;
            public Vector3 LocalPosition => _localPosition;
            public Vector3 LocalEulerAngles => _localEulerAngles;
            public Vector3 LocalScale => _localScale == Vector3.zero ? Vector3.one : _localScale;
            public float MinimumBlockExtent => Mathf.Max(0f, _minimumBlockExtent);
            public float MinimumAspectRatio => Mathf.Max(1f, _minimumAspectRatio);
            public float MaximumAspectRatio => _maximumAspectRatio <= 0f
                ? float.MaxValue
                : Mathf.Max(MinimumAspectRatio, _maximumAspectRatio);
            public VisualFitMode FitMode => _fitMode;
        }
    }
}
