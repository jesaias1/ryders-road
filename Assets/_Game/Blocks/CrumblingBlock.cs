using Avoidance.Gameplay.Audio;
using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Gameplay.Blocks
{
    public enum CrumblingBlockPhase
    {
        Stable,
        Stage1,
        Stage2,
        Stage3,
        Gone
    }

    public sealed class CrumblingBlockState
    {
        private readonly float _activationDelay;
        private readonly float _collapseDelay;
        private float _elapsed;
        private bool _activated;

        public CrumblingBlockState(float activationDelay, float fallDelay)
        {
            _activationDelay = Mathf.Max(0f, activationDelay);
            _collapseDelay = Mathf.Max(0.03f, fallDelay);
        }

        public CrumblingBlockPhase Phase { get; private set; } = CrumblingBlockPhase.Stable;
        public bool IsActivated => _activated;

        public void Activate()
        {
            if (_activated)
            {
                return;
            }

            _activated = true;
            _elapsed = 0f;
            Phase = CrumblingBlockPhase.Stage1;
        }

        public void Tick(float deltaTime)
        {
            if (!_activated || Phase == CrumblingBlockPhase.Gone || deltaTime <= 0f)
            {
                return;
            }

            _elapsed += deltaTime;
            var activeElapsed = Mathf.Max(0f, _elapsed - _activationDelay);
            if (activeElapsed >= _collapseDelay)
            {
                Phase = CrumblingBlockPhase.Gone;
            }
            else if (activeElapsed >= _collapseDelay * (2f / 3f))
            {
                Phase = CrumblingBlockPhase.Stage3;
            }
            else if (activeElapsed >= _collapseDelay * (1f / 3f))
            {
                Phase = CrumblingBlockPhase.Stage2;
            }
            else
            {
                Phase = CrumblingBlockPhase.Stage1;
            }
        }

        public void Reset()
        {
            _activated = false;
            _elapsed = 0f;
            Phase = CrumblingBlockPhase.Stable;
        }
    }

    [DisallowMultipleComponent]
    public sealed class CrumblingBlock : MonoBehaviour, IModuleResettable
    {
        private Renderer[] _fallbackRenderers;
        private Collider _solidCollider;
        private CrumblingBlockState _state;
        private Transform _visualRoot;
        private GameObject[] _stageVisuals;
        private Vector3 _visualStartLocalPosition;
        private Quaternion _visualStartLocalRotation;
        private Vector3 _startPosition;
        private float _resetTime = 1f;
        private float _shakeAmount = 0.06f;
        private float _goneTimer;
        private float _simulationTime;
        private bool _warningPlayed;
        private bool _collapseHandled;
        private System.Action<Vector3> _onWarning;
        private System.Action<Vector3> _onGone;

        private void Awake()
        {
            CacheDependencies();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_state == null || deltaTime <= 0f)
            {
                return;
            }

            var previousPhase = _state.Phase;
            _simulationTime += deltaTime;
            _state.Tick(deltaTime);
            if (_state.Phase != previousPhase)
            {
                ApplyPhasePresentation();
            }

            ApplyVisualShake();

            if (_state.Phase == CrumblingBlockPhase.Gone && !_collapseHandled)
            {
                _collapseHandled = true;
                SetSolid(false);
                _goneTimer = _resetTime;
                _onGone?.Invoke(_startPosition);
            }

            if (_state.Phase == CrumblingBlockPhase.Gone && _resetTime > 0f)
            {
                _goneTimer -= deltaTime;
                if (_goneTimer <= 0f)
                {
                    ResetForModule();
                }
            }
        }

        public void Initialize(
            float activationDelay,
            float fallDelay,
            float resetTime,
            float shakeAmount,
            System.Action<Vector3> onWarning,
            System.Action<Vector3> onGone)
        {
            CacheDependencies();
            _state = new CrumblingBlockState(activationDelay, fallDelay);
            _resetTime = Mathf.Max(0f, resetTime);
            _shakeAmount = Mathf.Max(0f, shakeAmount);
            _onWarning = onWarning;
            _onGone = onGone;
            ApplyPhasePresentation();
        }

        private void CacheDependencies()
        {
            _fallbackRenderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            _solidCollider = GetComponent<Collider>();
            _startPosition = transform.position;
        }

        public void Initialize(
            float activationDelay,
            float fallDelay,
            float resetTime,
            float shakeAmount,
            Transform visualRoot,
            GameObject stage1Visual,
            GameObject stage2Visual,
            GameObject stage3Visual,
            System.Action<Vector3> onWarning,
            System.Action<Vector3> onGone)
        {
            _visualRoot = visualRoot;
            _stageVisuals = new[] { stage1Visual, stage2Visual, stage3Visual };
            if (_visualRoot != null)
            {
                _visualStartLocalPosition = _visualRoot.localPosition;
                _visualStartLocalRotation = _visualRoot.localRotation;
            }

            Initialize(
                activationDelay,
                fallDelay,
                resetTime,
                shakeAmount,
                onWarning,
                onGone);
        }

        public void Activate(ParkourMotor motor)
        {
            if (motor == null || _state == null || _state.IsActivated)
            {
                return;
            }

            motor.GetComponent<MovementFeedback>()?.PlayCrumblingWarning();
            _state.Activate();
            ApplyPhasePresentation();
        }

        public void ResetForModule()
        {
            _state?.Reset();
            _warningPlayed = false;
            _collapseHandled = false;
            _goneTimer = 0f;
            _simulationTime = 0f;
            SetSolid(true);
            ResetVisualPose();
            ApplyPhasePresentation();
        }

        public bool SolidColliderEnabled => _solidCollider != null && _solidCollider.enabled;
        public CrumblingBlockPhase Phase => _state?.Phase ?? CrumblingBlockPhase.Stable;

        private void ApplyPhasePresentation()
        {
            if (_stageVisuals != null && _stageVisuals.Length == 3)
            {
                var visibleIndex = _state == null || _state.Phase == CrumblingBlockPhase.Stable
                    ? 0
                    : _state.Phase == CrumblingBlockPhase.Stage1
                        ? 0
                        : _state.Phase == CrumblingBlockPhase.Stage2
                            ? 1
                            : _state.Phase == CrumblingBlockPhase.Stage3 ? 2 : -1;
                for (var index = 0; index < _stageVisuals.Length; index++)
                {
                    if (_stageVisuals[index] != null)
                    {
                        _stageVisuals[index].SetActive(index == visibleIndex);
                    }
                }
            }
            else
            {
                SetFallbackVisible(_state == null || _state.Phase != CrumblingBlockPhase.Gone);
            }

            if (_state != null
                && (_state.Phase == CrumblingBlockPhase.Stage2
                    || _state.Phase == CrumblingBlockPhase.Stage3)
                && !_warningPlayed)
            {
                _warningPlayed = true;
                _onWarning?.Invoke(transform.position);
            }
        }

        private void ApplyVisualShake()
        {
            if (_visualRoot == null || _state == null)
            {
                return;
            }

            var multiplier = _state.Phase == CrumblingBlockPhase.Stage2
                ? 0.45f
                : _state.Phase == CrumblingBlockPhase.Stage3 ? 1f : 0f;
            if (multiplier <= 0f)
            {
                ResetVisualPose();
                return;
            }

            var wave = Mathf.Sin(_simulationTime * 43f);
            var crossWave = Mathf.Sin(_simulationTime * 31f + 0.7f);
            var amount = _shakeAmount * multiplier;
            var parentScale = _visualRoot.parent == null
                ? Vector3.one
                : _visualRoot.parent.lossyScale;
            _visualRoot.localPosition = _visualStartLocalPosition
                + new Vector3(
                    wave * amount / Mathf.Max(0.01f, Mathf.Abs(parentScale.x)),
                    0f,
                    crossWave * amount * 0.7f / Mathf.Max(0.01f, Mathf.Abs(parentScale.z)));
            _visualRoot.localRotation = _visualStartLocalRotation
                * Quaternion.Euler(0f, crossWave * multiplier * 1.25f, wave * multiplier * 0.8f);
        }

        private void ResetVisualPose()
        {
            if (_visualRoot == null)
            {
                return;
            }

            _visualRoot.localPosition = _visualStartLocalPosition;
            _visualRoot.localRotation = _visualStartLocalRotation;
        }

        private void SetFallbackVisible(bool visible)
        {
            if (_fallbackRenderers == null)
            {
                return;
            }

            foreach (var renderer in _fallbackRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = visible;
                }
            }
        }

        private void SetSolid(bool solid)
        {
            if (_solidCollider != null)
            {
                _solidCollider.enabled = solid;
            }
        }
    }

    public sealed class CrumblingBlockTrigger : MonoBehaviour
    {
        private CrumblingBlock _block;

        public void Initialize(CrumblingBlock block)
        {
            _block = block;
        }

        private void OnTriggerEnter(Collider other)
        {
            _block?.Activate(other.GetComponentInParent<ParkourMotor>());
        }
    }
}
