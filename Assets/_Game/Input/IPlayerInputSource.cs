using System.Collections.Generic;
using UnityEngine;

namespace Avoidance.Input
{
    public interface IPlayerInputSource
    {
        Vector2 Move { get; }
        Vector2 LookDelta { get; }
        bool JumpPressed { get; }
        void ResetState();
    }

    public interface IFlowSteeringInputSource
    {
        bool FlowSteeringEnabled { get; }
        AutoCameraProfileKind AutoCameraProfile { get; }
    }

    public interface IDevelopmentPlayerInputSource : IPlayerInputSource
    {
        bool RestartPressed { get; }
        bool SwitchProfilePressed { get; }
        bool ToggleTouchZonesPressed { get; }
        bool ToggleCameraEffectsPressed { get; }
        IReadOnlyList<int> ActiveTouchIds { get; }
    }

    public sealed class NullPlayerInputSource : IPlayerInputSource
    {
        public Vector2 Move => Vector2.zero;
        public Vector2 LookDelta => Vector2.zero;
        public bool JumpPressed => false;
        public void ResetState() { }
    }
}
