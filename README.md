## 0.17.0 existing-world flow and cohesion

The existing five Campaign worlds and Spiral now use authored route/presentation
updates with shared lighting and distinct deck kits. Windward content-6 geometry
and movement compatibility 5 remain unchanged. EditMode **252/252** and PlayMode
**156/156** pass, including 180 new sequence cases, Windward’s 72 cases and ten
complete Campaign runs. The ARM64 Android development APK is verified:
`Builds/Android/RYDERS-ROAD-0.17.0-world-flow-dev.apk` (189,190,245 bytes).

See [delivery report](Docs/PRODUCTION_0170.md) and [before/after evidence](Docs/World170QA/README.md).
New art is Editor-authored or reused/refitted production content: **3DAIStudio was
unavailable and that requested portion remains incomplete**. No physical S23
acceptance is claimed. This authority supersedes the earlier art stop; stop after
this delivery for physical and visual feedback. Earlier entries below are history.

## 0.16.2 flow sequences

Windward content 6 uses four coherent sequences, 20 shared supports and three
recovery Restores. Slow/flow/expert sequence testing passes all 72 clean/error
cases; EditMode 249/249 and PlayMode 152/152 pass. Movement compatibility 5,
controls, music and fatal scenery remain unchanged. See Docs/PRODUCTION_0162.md
for before/after captures, measurements and APK. Stop for physical S23 feedback;
do not begin 3DAIStudio art production before physical route approval.

## 0.16.1 route cadence correction

Windward content 5 now uses one shared sweep: 22 supports, five large anchors,
21 committed gaps and four measured same-route skip opportunities. Repeated
scaffold braces and the separate chord are removed. Shared movement remains 5.
EditMode 249/249 and PlayMode 151/151 pass. See Docs/PRODUCTION_0161.md for the
APK and verification evidence. Stop after delivery for physical S23 feedback.

> Current milestone: [0.16.0 Windward benchmark and fatal scenery](Docs/PRODUCTION_0160.md). Movement tuning remains unchanged. Awaiting physical S23 benchmark feedback.

> Current milestone: [0.15.0 shared responsive movement and original soundtrack](Docs/PRODUCTION_0150.md). Normal Campaign/Spiral/practice use compatibility 5. Physical S23 tuning remains pending.

# RYDER'S ROAD

Unity 6 landscape mobile first-person block parkour project. The current
player-facing title is `RYDER'S ROAD`; `RYDERS BLOCK` remains only as a legacy
internal codename in stable technical identifiers. The current build is
`0.4.7-brand-alpha-presentation`.

Open with Unity `6000.5.6f1`, let packages import, then run
`RYDERS BLOCK > Apply Project Settings` and
`RYDERS BLOCK > Validate Project`. Start play mode from `Bootstrap`; it loads
the development module selector.

See `AGENTS.md`, `ARCHITECTURE.md`, and `TESTING.md` before editing.
