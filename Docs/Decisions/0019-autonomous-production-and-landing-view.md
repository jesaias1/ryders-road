# ADR 0019 — autonomous production, isolated landing view and authored preservation

2026-09-08. The user's Autonomous Game Production Mission supersedes earlier
instructions to stop all production after physical gates. Continue independent
work; promotion of a movement/control candidate still requires physical acceptance.
No publishing, spending, services, save wipe or unrelated destructive work.

The inspected 0.9.6 source retains manual right-thumb pitch. The user reports
promising physical strafing with unresolved landing visibility, not acceptance of
the candidate. Preserve both production and mastery motor bytes. Compare existing
manual view against one removable, session-only landing view in Flow Lab. Do not
add automatic strafing, yaw-generated energy, platform targets or landing aids.

LandingView owns only a bounded pitch offset. Its settings live in training JSON.
It runs only for Flow touch input after explicit VIEW selection, never in Campaign
or Classic/Editor. Manual vertical drag adopts the rendered angle before removing
the offset; it suppresses framing for 1.2 seconds without snapping the view back.
The default room is identified by stable ID. One ordinary two-gap landing exercise
provides comparable footing evidence without creating an advanced-map collection.
Session bests separate room, motor, controls and view. V4 is unchanged.

Startup must not rebuild authored Ancient Abyss. Existing landmark prefabs load
as assets; only missing ones generate. Version-7 module/biome content is preserved,
matching the existing Sky City/Mountain preservation boundary. Explicit Recovery
still regenerates when deliberately invoked. Byte-preservation regression covers
landmarks, biome and a real serialized module edit through full project setup.

Six original CC0 Kenney contact clips populate existing hooks at conservative
profile gains. No runtime tone generation. Licensing, duration/decode/headroom and
hook verification do not approve the mix; phone listening is still required.
Loop cue switching must replace the actual clip; a missing cue/profile stops the
previous sound, and missing-asset intent remains a rising edge rather than a spam.

These are implementation/evaluation candidates. Do not label them Gold, physically
approved or production-frozen. See PRODUCTION_097.md for evidence and continuation.
