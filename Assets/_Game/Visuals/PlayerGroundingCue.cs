using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Avoidance.Gameplay.Visuals
{
    [DisallowMultipleComponent]
    public sealed class PlayerGroundingCue : MonoBehaviour
    {
        public const string ShadowObjectName = "Ryder Contact Shadow";

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static Texture2D _softDiscTexture;
        private static Material _sharedShadowMaterial;

        private Transform _player;
        private CharacterController _controller;
        private PlayerGroundingProfile _profile;
        private Transform _shadowTransform;
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Vector3[] _vertices;
        private Vector2[] _uvs;
        private bool[] _supported;
        private readonly List<int> _triangles = new List<int>(72);
        private MaterialPropertyBlock _propertyBlock;

        public bool IsVisible => _renderer != null && _renderer.enabled;
        public float CurrentOpacity { get; private set; }
        public PlayerGroundingProfile Profile => _profile;

        public void Initialize(
            Transform player,
            CharacterController controller,
            PlayerGroundingProfile profile = null)
        {
            _player = player != null ? player : transform;
            _controller = controller;
            _profile = profile != null ? profile : PlayerGroundingProfile.CreateRuntimeDefault();
            BuildShadowMesh();
            UpdateShadow();
        }

        private void LateUpdate()
        {
            UpdateShadow();
        }

        private void BuildShadowMesh()
        {
            var shadow = new GameObject(ShadowObjectName, typeof(MeshFilter), typeof(MeshRenderer));
            shadow.layer = LayerMask.NameToLayer("Ignore Raycast");
            shadow.transform.SetParent(transform, false);
            _shadowTransform = shadow.transform;

            var count = _profile.SegmentCount;
            _vertices = new Vector3[count + 1];
            _uvs = new Vector2[count + 1];
            _supported = new bool[count + 1];
            _uvs[0] = new Vector2(0.5f, 0.5f);
            for (var index = 0; index < count; index++)
            {
                var angle = index / (float)count * Mathf.PI * 2f;
                _uvs[index + 1] = new Vector2(
                    0.5f + Mathf.Cos(angle) * 0.5f,
                    0.5f + Mathf.Sin(angle) * 0.5f);
            }

            _mesh = new Mesh
            {
                name = "Mesh_RR_PlayerContactShadow",
                hideFlags = HideFlags.DontSave
            };
            _mesh.MarkDynamic();
            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            shadow.GetComponent<MeshFilter>().sharedMesh = _mesh;

            _renderer = shadow.GetComponent<MeshRenderer>();
            _renderer.sharedMaterial = ResolveShadowMaterial();
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.lightProbeUsage = LightProbeUsage.Off;
            _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            _renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            _renderer.enabled = false;
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void UpdateShadow()
        {
            if (_player == null || _profile == null || _renderer == null)
            {
                return;
            }

            var footPosition = ResolveFootPosition();
            var rayLift = 0.12f;
            var rayDistance = _profile.MaximumSurfaceDistance + rayLift;
            var groundMask = LayerMask.GetMask("Ground");
            if (groundMask == 0)
            {
                groundMask = Physics.DefaultRaycastLayers;
            }

            var centerSupported = TryResolveSurface(
                footPosition + Vector3.up * rayLift,
                rayDistance,
                groundMask,
                out var centerHit);
            _supported[0] = centerSupported;
            if (!centerSupported)
            {
                HideShadow();
                return;
            }

            _vertices[0] = _shadowTransform.InverseTransformPoint(
                centerHit.point + centerHit.normal * _profile.SurfaceOffset);
            var segmentCount = _profile.SegmentCount;
            for (var index = 0; index < segmentCount; index++)
            {
                var angle = index / (float)segmentCount * Mathf.PI * 2f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _profile.Radius;
                var supported = TryResolveSurface(
                    footPosition + offset + Vector3.up * rayLift,
                    rayDistance,
                    groundMask,
                    out var hit);
                _supported[index + 1] = supported;
                _vertices[index + 1] = _shadowTransform.InverseTransformPoint(
                    supported
                        ? hit.point + hit.normal * _profile.SurfaceOffset
                        : centerHit.point + offset);
            }

            _triangles.Clear();
            for (var index = 0; index < segmentCount; index++)
            {
                var current = index + 1;
                var next = (index + 1) % segmentCount + 1;
                if (!_supported[current] || !_supported[next])
                {
                    continue;
                }

                _triangles.Add(0);
                _triangles.Add(next);
                _triangles.Add(current);
            }

            if (_triangles.Count < 3)
            {
                HideShadow();
                return;
            }

            var height = Mathf.Max(0f, footPosition.y - centerHit.point.y);
            var fade = 1f - Mathf.InverseLerp(
                _profile.FullOpacityHeight,
                _profile.FadeOutHeight,
                height);
            CurrentOpacity = _profile.ShadowColor.a * Mathf.Clamp01(fade);
            if (CurrentOpacity <= 0.005f)
            {
                HideShadow();
                return;
            }

            _mesh.Clear(false);
            _mesh.vertices = _vertices;
            _mesh.uv = _uvs;
            _mesh.SetTriangles(_triangles, 0, true);
            _mesh.RecalculateBounds();

            var color = _profile.ShadowColor;
            color.a = CurrentOpacity;
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColorId, color);
            _propertyBlock.SetColor(ColorId, color);
            _renderer.SetPropertyBlock(_propertyBlock);
            _renderer.enabled = true;
        }

        private Vector3 ResolveFootPosition()
        {
            if (_controller == null)
            {
                return _player.position;
            }

            var localFootY = _controller.center.y - _controller.height * 0.5f;
            return _player.TransformPoint(new Vector3(
                _controller.center.x,
                localFootY,
                _controller.center.z));
        }

        private bool TryResolveSurface(
            Vector3 origin,
            float distance,
            int groundMask,
            out RaycastHit hit)
        {
            return Physics.Raycast(
                    origin,
                    Vector3.down,
                    out hit,
                    distance,
                    groundMask,
                    QueryTriggerInteraction.Ignore)
                && hit.normal.y >= _profile.MinimumSurfaceNormalY;
        }

        private void HideShadow()
        {
            CurrentOpacity = 0f;
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }

        private static Material ResolveShadowMaterial()
        {
            if (_sharedShadowMaterial != null)
            {
                return _sharedShadowMaterial;
            }

            _sharedShadowMaterial = VisualMaterialUtility.CreateRuntimeTransparentTexturedMaterial(
                "MAT_RR_PlayerContactShadow_Runtime",
                Color.white,
                ResolveSoftDiscTexture(),
                Vector2.one,
                Shader.Find(VisualMaterialUtility.UnlitShaderName));
            return _sharedShadowMaterial;
        }

        private static Texture2D ResolveSoftDiscTexture()
        {
            if (_softDiscTexture != null)
            {
                return _softDiscTexture;
            }

            const int size = 32;
            _softDiscTexture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = "TEX_RR_PlayerContactShadow_Runtime",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    var alpha = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01(distance));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            _softDiscTexture.SetPixels32(pixels);
            _softDiscTexture.Apply(false, true);
            return _softDiscTexture;
        }
    }
}
