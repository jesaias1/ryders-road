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

    public sealed class NullPlayerInputSource : IPlayerInputSource
    {
        public Vector2 Move => Vector2.zero;
        public Vector2 LookDelta => Vector2.zero;
        public bool JumpPressed => false;
        public void ResetState() { }
    }
}
