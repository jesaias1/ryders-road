# Cadence 0.16.1 evidence

Windward content 5. Geometry CSV measures horizontal nearest rectangle separation,
not centre distance or required diagonal flight distance. All 21 gaps are 2–3 m.
Route CSVs are controller simulations with explicit test-only aiming. Safe pauses
before jumps; direct means fewer pauses on the same sweep, not the retired chord.
Slow uses 70% ground input and full available airborne correction; the previous
0.16.0 constant-70%-input route is not an acceptance requirement for this geometry.
Skips use controlled 17 m/s incoming states and 0 / 0.75 m lateral aim offsets.
They are opportunities, not proof of speed earned throughout a human run.

Runtime captures are actual Unity renders. No physical S23, final rank, visual
acceptance, sustained FPS, thermal or multitouch test is claimed.
See ../PRODUCTION_0161.md for final validation and APK information.
