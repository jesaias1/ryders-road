namespace Avoidance.Gameplay.Player
{
    public sealed class JumpWindowState
    {
        public float TimeSinceGrounded { get; private set; } = float.MaxValue;
        public float BufferedJumpRemaining { get; private set; }
        public bool IsCoyoteEligible { get; private set; }
        public bool IsJumpBuffered => BufferedJumpRemaining > 0f;

        public void Tick(
            float deltaTime,
            bool grounded,
            bool jumpPressed,
            float coyoteDuration,
            float bufferDuration)
        {
            if (grounded)
            {
                TimeSinceGrounded = 0f;
            }
            else
            {
                TimeSinceGrounded += deltaTime;
            }

            if (jumpPressed)
            {
                BufferedJumpRemaining = bufferDuration;
            }
            else
            {
                BufferedJumpRemaining = System.Math.Max(
                    0f,
                    BufferedJumpRemaining - deltaTime);
            }

            IsCoyoteEligible = grounded || TimeSinceGrounded <= coyoteDuration;
        }

        public bool TryConsumeJump()
        {
            if (!IsJumpBuffered || !IsCoyoteEligible)
            {
                return false;
            }

            BufferedJumpRemaining = 0f;
            IsCoyoteEligible = false;
            TimeSinceGrounded = float.MaxValue;
            return true;
        }

        public void Reset()
        {
            TimeSinceGrounded = float.MaxValue;
            BufferedJumpRemaining = 0f;
            IsCoyoteEligible = false;
        }
    }
}

