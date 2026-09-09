# 0.14.2 reference-verification evidence

Fresh Unity 6000.5.6f1 development-PC evidence, 2026-09-09. No physical phone
testing. validation.json records final suites, hashes, APK and measurements.

- windward-scripted.mp4: continuous first 12 simulated seconds of the existing
  Windward normal-route test pilot. Explicit stick corrections and sharp view
  turns; stop-and-go jumps, not continuous held input. No per-link position or
  velocity reset. The test pilot has no authority in gameplay.
- held-bhop-scripted.mp4: existing Flow Lab lane, continuous held input through
  valid contacts; straight chain, not a meaningful cornering demonstration.

Both clips are encoded at 15 fps from simulated frames, not realtime performance
measurements. Inspected representative decoded/source frames. Together they
illustrate route execution and held hopping, but do not provide a single captured
combined-touch turning chain. The strengthened two-pointer integration test
covers that input combination numerically on its flat fixture. Human phone feel,
touch comfort and OS event delivery remain for the S23 test.

Raw frames and full XML/logs stay under Logs. No input replay or capture code is
included in the Android runtime. See ../PRODUCTION_0142.md for the handoff.
