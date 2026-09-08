# ADR 0023 — Campaign presentation and Windward Observatory

2026-09-08. The user's new milestone authorizes production beyond 0.10.0's
movement-only stop. Their physical 0.9.9 preference is the accepted Campaign
controls: predictable movement and natural manual looking. Keep compatibility 1
and saved control choices as normal; retain 0.10.0 and earlier experiments without
promotion. This is provisional, not a movement freeze or physical Gold approval.

Keep SceneTransitionHost, readiness, timeouts, recovery and caller-independent
loading. Replace only LoadingPresentation's video/poster with the supplied
Jesaias emblem traced to an editable SVG, a transparent raster and a dark native
startup derivative. Pulse opacity in unscaled time; never wait for animation.
Native splash has no intentional logo hold. Missing emblem falls back to text.
Old media remains recoverable and is no longer decoded during loading.

Diagnosis: the active illustrated Ancient Abyss sky is imported as an automatic
cubemap from a 2:1 image. The source's left/right cloud and island silhouettes do
not meet. Seamless cubemap filtering fixes face sampling, not incompatible source
content. Sky City, Mountain and Foundry cloned that source. Replace Campaign sky
sampling with a continuous unit-direction sky shader: smooth height gradients,
three bounded noise octaves and a small sun glow, no longitude UV seam or horizon
geometry. Per-world materials encode distinct palettes. Existing environment
profiles own fog and lighting, including sun direction. An opt-in authored-lighting
flag bypasses the legacy 003 intensity/ambient override; old content keeps defaults.
No route, collider, camera or movement changes are justified by this art pass.

World 005 retains reserved `module.005.foundry-pulse` identity. Its design-only
foundry concept evolves into Windward Observatory because shipped Solar Foundry
already owns industrial heat. The observatory keeps the large instrument concept
but uses porcelain, ink, brass and slate sails against a lavender evening sky.
Arrival → west anchorage → broad wind arc → east anchorage → lens ascent →
telescope terrace. Five narrower inner-chord pads bypass eleven arc pads.
Three Restore Points bound recovery; normal completion has no special input or
mastery requirement. Bronze always unlocks progression. Existing ranking/replay
and V4 records handle the additive road; no migration or reward-system change.
Spiral retains `module.004.the-spiral` outside Campaign. Remaining reserved IDs
006–008 remain design-only.

World content is explicitly authored and serialized, never regenerated at startup.
Shared WorldMeshBake exports original low-poly structures with matching collision
and explicit scenery-landing recovery. Far sails are noncolliding and outside the
route. Original filtered wind is baked offline; ground travel reuses licensed
Kenney contact foley. Audio observes state without movement or save authority.

Automated evidence and remaining physical checks belong in PRODUCTION_0110.md.
STOP after the APK, documentation and private Git checkpoint for S23 feedback.
