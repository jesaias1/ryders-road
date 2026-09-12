# Benchmark 0.16 evidence

Actual Unity PlayMode measurements and rendered captures from the final passing
151-test run. See ../PRODUCTION_0160.md for geometry, measurement limitations,
scenery counts and current release status.

- safe/direct CSVs: complete scripted runs from rest, with per-link dimensions
  and measured speeds. Trial and normal use the same shared movement.
- slow CSV: complete ordinary route at 70% input, including walking setup on the
  lens recovery terrace. It does not represent constant-speed movement.
- held-chains.csv: continuous opening segments at controlled incoming speed.
- skips.csv: controlled 17 m/s entries, centered and 0.75 m offset targets.
  These do not establish a full human run retaining that speed.
- scenery-audit.csv: retained collider classification and actual drop samples.
- arrival.png / overview.png: actual Editor renders, not physical S23 captures.

Final EditMode passed 249/249 after the Unity Hub license refresh. The Android
development APK was built and verified:
`Builds/Android/RYDERS-ROAD-0.16.0-flow-benchmark-fatal-scenery-dev.apk`
with SHA-256 `9BFC0CC2DA87CC0018C2F34A5344BC1BB623F4C57E9FF6D13F6C5C0C08A81825`.
No physical-device testing, FPS measurement or final rank calibration is claimed.
