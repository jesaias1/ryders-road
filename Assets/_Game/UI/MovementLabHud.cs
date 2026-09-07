using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Respawn;
using UnityEngine;
using UnityEngine.UI;

namespace Avoidance.UI
{
    [DisallowMultipleComponent]
    public sealed class MovementLabHud : MonoBehaviour
    {
        private Text _status;
        private Text _completion;
        private PlayerRuntimeCoordinator _player;
        private RestoreController _restore;
        private string _buildVersion;

        public void Initialize(
            Text status,
            Text completion,
            PlayerRuntimeCoordinator player,
            RestoreController restore,
            string buildVersion)
        {
            _status = status;
            _completion = completion;
            _player = player;
            _restore = restore;
            _buildVersion = buildVersion;
        }

        private void Update()
        {
            if (_status == null || _player == null)
            {
                return;
            }

            _status.text =
                $"{_buildVersion}  {_player.Motor.Profile.DisplayName.ToUpperInvariant()}\n" +
                $"SPD {_player.Motor.HorizontalSpeed:0.0}  " +
                $"GROUND {(_player.Motor.IsGrounded ? "YES" : "NO")}  " +
                $"RESTORE {_restore.RestoreCount}\n" +
                $"JUMP {_player.Motor.Profile.JumpHeight:0.00}/{_player.Motor.Profile.TimeToApex:0.000}s  " +
                $"EST {_player.Motor.Profile.EstimatedMaximumJumpDistance:0.0}m  " +
                $"LAST {_player.Motor.LastJumpHorizontalDistance:0.0}m  " +
                $"FOV {_player.CameraRig.CurrentFieldOfView:0}";
        }

        public void ShowModuleFixed()
        {
            if (_completion == null)
            {
                return;
            }

            _completion.text = "MOVEMENT LAB COMPLETE\nTEMPORARY PATCH BLOCK REACHED";
            _completion.enabled = true;
        }
    }
}
