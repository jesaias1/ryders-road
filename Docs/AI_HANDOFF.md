## 0.16.1 route cadence correction

Windward content 5 now uses one shared sweep: 22 supports, five large anchors,
21 committed gaps and four measured same-route skip opportunities. Repeated
scaffold braces and the separate chord are removed. Shared movement remains 5.
EditMode 249/249 and PlayMode 151/151 pass. See Docs/PRODUCTION_0161.md for the
APK and verification evidence. Stop after delivery for physical S23 feedback.

Release status 2026-09-12: PlayMode 151/151 passed; EditMode 249/249 passed after Unity Hub license refresh; source/LFS checks passed; Android dev APK built and verified. APK: `Builds/Android/RYDERS-ROAD-0.16.0-flow-benchmark-fatal-scenery-dev.apk`, SHA-256 `9BFC0CC2DA87CC0018C2F34A5344BC1BB623F4C57E9FF6D13F6C5C0C08A81825`. Commit/push this milestone, then stop for physical S23 feedback. See PRODUCTION_0160.md.

# Current authority — 0.16.0 movement-aware benchmark and fatal scenery

User S23 feedback approves the stronger 0.15.0 air-strafing. Preserve the motor,
whole-right hold controls and music. Windward is the one reauthored benchmark;
all Campaign scenery now follows explicit traversal/fatal/non-collision intent.
See PRODUCTION_0160.md and ADR 0030 for safe/flow/expert lines, record compatibility,
audit counts, tests and APK. Do not tune speed before physical benchmark feedback.
Deliver then STOP. Older gates and route details below are historical.

# Current authority — 0.15.0 shared movement and music

See PRODUCTION_0150.md, ADR 0029, Movement150QA and Audio150.
The user's physical feedback approves the existing whole-right hold controls and
explicitly authorizes game-wide rollout. New responsive movement.shared compatibility
5 is normal across Campaign, Spiral and practice; no development trial is needed.
Old history/unlocks stay intact, new PBs use separate compatibility buckets.
Original music is integrated with independent Music volume. Physical tuning and
listening remain pending. Deliver then STOP for S23 feedback.
Earlier candidate-only instructions below are historical where they conflict.

# Current authority - 0.14.2 GoldSrc reference verification

See PRODUCTION_0142.md, GOLDSRC_MOVEMENT_REFERENCE.md and ADR 0028.
Same compatibility-4 physics/tuning and whole-right hold controls as 0.14.1;
applied air telemetry now measures the post-bound net change. New regression
measurements confirm useful steering. Accepted Campaign/saves/PBs stay protected.
Deliver the APK and Git milestone, then STOP for physical S23 feedback.

# Current authority - 0.14.1 whole-right-area hold correction

The entire right gameplay look area activates JumpHeld on contact, keeps it true
while dragging the camera, and stops on release/cancellation. There is no separate
Jump glyph or activation gesture in E. The 0.14.0 movement motor, compatibility 4,
all tuning/camera physics and accepted Campaign/save isolation remain unchanged.

Open DEVELOPMENT > CAMPAIGN FLOW TRIAL > E - FOUNDATION; A remains the accepted
comparison. Flow Lab E has the same corrected surface. Read PRODUCTION_0141.md
for the corrected APK, validation and S23 checklist. ADR 0027's correction replaces
the old glyph scheme. STOP after delivery for physical feedback; no S23 test or
Gold approval is claimed. Previous handoff below is historical where it conflicts.

# Historical - 0.14.0 movement foundation candidate

Updated 2026-09-09. The new user mission supersedes 0.13.0's movement-work stop.
ADR 0027 defines the intended future shared standard: Quake-inspired manual wish
acceleration, hold-to-bhop and same-thumb Jump/drag looking. Compatibility 4 is
implemented in the existing motor, isolated as Campaign trial E and Flow Lab E.
It is not Gold or physically approved. Accepted Campaign/compatibility 1, the five
0.13.0 roads/art, legacy candidates and V4 PB/history/progression are preserved.

DEVELOPMENT > CAMPAIGN FLOW TRIAL selects E by default; choose A on the same road
for accepted comparison. Hold the Jump glyph and drag to look. Full manual left
stick movement, surrounding tap/drag and Editor WASD/mouse/Space remain available.
Flow Lab's B -> A -> D -> E cycle provides the existing bhop and surf diagnostics.
All trial attempts/results remain session-only. No global movement promotion,
new worlds, Campaign geometry changes or camera redesign was performed.

Read PRODUCTION_0140.md for the exact APK, verification, captures, known limits and
short S23 checklist; Decisions/0027-movement-foundation-hold-jump-look.md records the
architecture and promotion decision. Implementation evidence
is in Movement140QA and local Logs/foundation-*. The physical test is the remaining
gate, not more autonomous production. STOP after delivery for the user's feedback.

Recovery baseline: f78864629d3af76da7439daa1f3d0fb428869e7b (0.13.0).
Existing approved branding and Docs/VisualReferences remain authoritative for art.
No device was connected or physically tested, and no adb install was attempted.
