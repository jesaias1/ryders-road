using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Checkpoints;
using Avoidance.Gameplay.Player;
using Avoidance.Input;
using UnityEngine;

namespace Avoidance.Gameplay.Respawn
{
    public static class VoidRule
    {
        public static bool IsInNullSpace(float playerY, float voidY)
        {
            return playerY < voidY;
        }
    }

    [DisallowMultipleComponent]
    public sealed class RestoreController : MonoBehaviour
    {
        private ParkourMotor _motor;
        private FirstPersonCameraRig _cameraRig;
        private IPlayerInputSource _input;
        private CheckpointService _checkpoints;
        private MovingPlatformMotion[] _movingPlatforms;
        private IModuleResettable[] _moduleResettables;
        private System.Action<bool> _onRestored;
        private System.Action _onFall;
        private float _nullSpaceY = -12f;
        private float _restoreDelay = 0.08f;
        private float _pendingTimer;
        private bool _pending;
        private bool _preservePendingYaw;
        private float _pendingYaw;
        private float _pendingPitch;

        public int RestoreCount { get; private set; }
        public bool IsRestorePending => _pending;
        public float SpawnProtectionRemaining { get; private set; }

        public void Initialize(
            ParkourMotor motor,
            FirstPersonCameraRig cameraRig,
            IPlayerInputSource input,
            CheckpointService checkpoints,
            MovingPlatformMotion[] movingPlatforms,
            IModuleResettable[] moduleResettables,
            System.Action<bool> onRestored,
            System.Action onFall,
            float nullSpaceY = -12f,
            float restoreDelay = 0.08f)
        {
            _motor = motor;
            _cameraRig = cameraRig;
            _input = input;
            _checkpoints = checkpoints;
            _movingPlatforms = movingPlatforms ?? new MovingPlatformMotion[0];
            _moduleResettables = moduleResettables ?? new IModuleResettable[0];
            _onRestored = onRestored;
            _onFall = onFall;
            _nullSpaceY = nullSpaceY;
            _restoreDelay = Mathf.Clamp(restoreDelay, 0f, 0.5f);
        }

        public void Initialize(
            ParkourMotor motor,
            FirstPersonCameraRig cameraRig,
            IPlayerInputSource input,
            CheckpointService checkpoints,
            MovingPlatformMotion[] movingPlatforms,
            System.Action<bool> onRestored,
            System.Action onFall,
            float nullSpaceY = -12f,
            float restoreDelay = 0.08f)
        {
            Initialize(
                motor,
                cameraRig,
                input,
                checkpoints,
                movingPlatforms,
                null,
                onRestored,
                onFall,
                nullSpaceY,
                restoreDelay);
        }

        public void Tick(bool manualRestart, float deltaTime)
        {
            SpawnProtectionRemaining = Mathf.Max(0f, SpawnProtectionRemaining - deltaTime);
            if (!_pending
                && (manualRestart
                    || (SpawnProtectionRemaining <= 0f
                        && VoidRule.IsInNullSpace(transform.position.y, _nullSpaceY))))
            {
                RequestRestore(!manualRestart);
            }

            if (!_pending)
            {
                return;
            }

            _pendingTimer -= deltaTime;
            if (_pendingTimer <= 0f)
            {
                RestoreNow();
            }
        }

        public void RequestRestore(bool countedAsFall)
        {
            if (_pending)
            {
                return;
            }

            _pending = true;
            _pendingTimer = _restoreDelay;
            _preservePendingYaw = countedAsFall;
            _pendingYaw = _cameraRig != null ? _cameraRig.RenderedYaw : transform.eulerAngles.y;
            _pendingPitch = _cameraRig != null ? _cameraRig.Pitch : 0f;
            if (countedAsFall)
            {
                _onFall?.Invoke();
            }
        }

        public void RequestFatalContact()
        {
            if (_pending) return;
            RequestRestore(true);
            _input?.ResetState();
            _motor?.SuspendUntilRestore();
        }

        public void RestoreNow()
        {
            var checkpoint = _checkpoints.Current;
            var restoreRotation = ResolveRestoreRotation(
                checkpoint.Rotation,
                _preservePendingYaw,
                _pendingYaw);
            foreach (var platform in _movingPlatforms)
            {
                if (platform != null)
                {
                    platform.ResetMotion();
                }
            }

            foreach (var resettable in _moduleResettables)
            {
                resettable?.ResetForModule();
            }

            _input.ResetState();
            _motor.ResetMotion(
                checkpoint.Position,
                restoreRotation,
                _motor.Profile.RespawnMovementLockDuration);
            _cameraRig.ResetView(
                restoreRotation,
                _preservePendingYaw ? _pendingPitch : (float?)null);
            RestoreCount++;
            SpawnProtectionRemaining = 0.25f;
            _pending = false;
            _pendingTimer = 0f;
            _preservePendingYaw = false;
            _onRestored?.Invoke(true);
        }

        public static Quaternion ResolveRestoreRotation(
            Quaternion checkpointRotation,
            bool preservePreDeathYaw,
            float preDeathYaw)
        {
            return preservePreDeathYaw
                ? Quaternion.Euler(0f, preDeathYaw, 0f)
                : checkpointRotation;
        }
    }
}
