# ADR 0016 — Mountain World and sequential Campaign

2026-09-06. The user physically tested 0.9.3 on Samsung S23 and accepted the
large Module 001 quality improvement. Phase 0.9.4 authorizes Module 002 production
and Campaign progression repairs. Module 001 is frozen; Module 003 is preserved.

Module 002 keeps its stable module, existing block, Moving, first Restore and
Patch IDs. Content version 2 authors a 12.4 m climb through Cliffside Arrival,
Broken Mountain Pass, Waterfall Garden, High Ridge and Summit Patch. Two Moving
crossings belong to this course. Two optional rock lines and attempt-local Flow
Shards use existing systems. Standard completion requires no advanced technique.
Rank times remain provisional; prior records are retained with their metadata.

MountainWorldProductionAuthoring bakes independent original geology, alpine
vegetation, small mountain buildings and shared materials. Near structural meshes
carry matching AuthoredSurface collision. Distant ridges are deliberately separate
and non-colliding. Mineral grain is an original deterministic authoring texture.
Waterfall ribbons use one inexpensive opaque scrolling shader; no particle storm,
transparent floor, world floor, or gameplay water rule is introduced. The existing
seamless sky resource is referenced through an isolated mountain material/profile.
Startup factories must preserve Module 002 v2 and its biome. The mountain profile
opts into runtime static geometry combining; other profiles retain their defaults.
AuthoredSurface keeps its collision source intact after render batching.

Restore/Patch definitions optionally reference an authored support by stable ID.
Module 002 uses this to eliminate overlapping generated mechanic platforms. Empty
references preserve existing modules' behavior. Validation rejects unresolved
references; runtime resolves from its authored-block dictionary, not hierarchy or
list position. These are content fields, not persisted player-save fields.

Campaign order remains the explicit 001 -> 002 -> 003 list. The Spiral remains a
separate challenge despite its legacy module.004.the-spiral identifier. Unbuilt
Campaign 004–008 have no selectable content or automatic successor in this APK.
The visible QA PRACTICE bypass is removed from the ordinary Campaign UI, including
development builds. Continue selects the highest unlocked unfinished module;
completed modules remain deliberate replays. Next is the primary results action.

V4 stores all finish attempts in completed/completionCount, including historical
practice results, but stores best/highest ranks only for eligible completions.
Unlock derivation now uses that existing validity/rank evidence. A valid stored
best/highest rank survives a later practice finish. A legacy completed flag with
no invalid marker remains legitimate evidence. Practice-only records and mere
attempts grant nothing. Later earned completions preserve earlier road access;
explicit exceptional unlock grants are retained. Unknown IDs never become members
of the Campaign by grant. This is an eligibility correction using existing fields,
not a persisted-field rename, deletion, or reinterpretation of completionCount.
No save migration or wipe is needed; queries do not rewrite user records.

New regression coverage isolates its save in memory, checks actual UI-driven
Bronze progression/Next/Continue/replay, and validates world preservation, bounded
geometry, collision and unmodified motor traversal. Physical S23 input, render
quality, sustained FPS and rank calibration remain the user's next acceptance gate.
