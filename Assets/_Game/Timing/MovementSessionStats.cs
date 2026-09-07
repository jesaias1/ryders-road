namespace Avoidance.Gameplay.Timing
{
    public sealed class MovementSessionStats
    {
        public int Jumps { get; private set; }
        public int Falls { get; private set; }
        public int Restores { get; private set; }
        public bool FinishReached { get; private set; }

        public void RecordJump() => Jumps++;
        public void RecordFall() => Falls++;
        public void RecordRestore() => Restores++;
        public void RecordFinish() => FinishReached = true;

        public void Reset()
        {
            Jumps = 0;
            Falls = 0;
            Restores = 0;
            FinishReached = false;
        }
    }
}
