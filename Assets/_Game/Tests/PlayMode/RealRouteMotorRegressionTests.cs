using Avoidance.Gameplay.Player;
using UnityEngine;

namespace Avoidance.Tests.PlayMode
{
    // Run the same real-controller hop, wall, surf and safety contracts on both motors.
    public sealed class RealRouteMotorRegressionTests : MovementMasteryTests
    {
        protected override MovementProfile Candidate => Resources.Load<MovementProfile>("Training/Movement_RealRoute");
    }
}
