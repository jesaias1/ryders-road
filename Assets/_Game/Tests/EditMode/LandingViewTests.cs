using Avoidance.Gameplay.Camera;
using NUnit.Framework;

namespace Avoidance.Tests.EditMode
{
    public sealed class LandingViewTests
    {
        [TestCase(30)] [TestCase(60)] [TestCase(120)]
        public void DescentFramesSmoothlyAndReturnsOnGround(int rate)
        {
            var view = new LandingView(new LandingViewSettings());
            float dt = 1f / rate, previous = 0f;
            for (int i = 0; i < rate; i++)
            {
                float offset = view.Tick(true, false, false, -12f, 16f, dt);
                Assert.That(offset - previous, Is.InRange(0f, 36f * dt + .001f));
                previous = offset;
            }
            Assert.That(view.Offset, Is.EqualTo(8f).Within(.001f));
            for (int i = 0; i < rate; i++) view.Tick(true, true, false, 0, 16f, dt);
            Assert.That(view.Offset, Is.Zero);
        }

        [Test]
        public void ManualTakeoverHoldsAndNeverForcesAnAlreadyDownwardView()
        {
            var view = new LandingView(new LandingViewSettings());
            view.Tick(true, false, false, -10, 16, 1);
            view.YieldToManual();
            Assert.That(view.Tick(true, false, false, -10, 24, .5f), Is.Zero);
            Assert.That(view.Tick(true, false, false, -10, 24, .5f), Is.Zero);
            Assert.That(view.Tick(true, false, false, -10, 40, 1), Is.Zero);
        }

        [Test]
        public void DisabledSurfAndAscentDoNotAddPitchAndResetClearsHistory()
        {
            var view = new LandingView(new LandingViewSettings());
            Assert.That(view.Tick(true, false, true, -10, 16, 1), Is.Zero);
            Assert.That(view.Tick(true, false, false, 10, 16, 1), Is.Zero);
            view.Tick(true, false, false, -10, 16, 1);
            Assert.That(view.Tick(false, false, false, -10, 16, .01f), Is.Zero);
            view.YieldToManual();view.Reset();
            Assert.That(view.Tick(true, false, false, -10, 16, 1), Is.EqualTo(8f));
        }
    }
}
