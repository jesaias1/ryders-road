## 0.17.0 existing-world visual pass

The full-game brief authorizes route/art work across the existing five Campaign worlds and Spiral, superseding the earlier art stop. Six small Editor-authored deck kits use seven shared untextured material families. Sky/Windward use ivory architectural panels; Mountain uses cut stone over geology; Abyss uses masonry/cyan seams; Foundry uses paired metal plates and warm stiffeners; Spiral retains celestial ruins and its helix. Existing landmark kits remain the primary world silhouettes.

Cohesive lighting uses world-specific sun/ambient/fog, restrained grading/bloom and distance-limited soft shadows. Readability and device performance remain acceptance criteria. This is not a claim of matching the reference image or physical visual approval. 3DAIStudio was unavailable; no new asset is represented as its output. See the before/after gallery and provenance in `Docs/World170QA`.

## 0.16.2 movement sequences

Windward content 6 replaces the alternating cadence with four coherent movement
sequences and three recovery Restores. Slow/flow/expert use the same 20 supports;
held sequences and imperfect entries are verified without per-hop resets. Shared
movement 5, input, camera, music, fatal scenery and V4 meaning remain unchanged.
See Docs/Decisions/0032-movement-sequence-windward.md and Docs/PRODUCTION_0162.md.

## 0.16.1 route cadence correction

Windward content 5 supersedes the broad 0.16.0 benchmark: a shared sweep with
committed gaps, smaller intermediate landings and sparse recovery anchors.
The separate chord and repeated scaffold braces are removed. Shared movement 5,
fatal scenery semantics, V4 history and other Campaign routes remain unchanged.
See Docs/Decisions/0031-route-cadence-correction.md and Docs/PRODUCTION_0161.md.

## 0.16.0 movement-aware route benchmark

The user's S23 feedback approves the stronger air-strafe feel. Windward now gives
that same motor room: substantial offset landings, long approaches, a broad north
sweep and diagonal shortcuts. Safe intermediate surfaces remain. Dark narrow
underside keels replace false secondary landing slabs, and the lower instrument
and displaced telescope preserve world depth without competing with route decks.
Other Campaign route geometry and all approved branding/music are preserved.
See ADR 0030 and Docs/PRODUCTION_0160.md; physical benchmark feedback is pending.

## 0.15.0 frontend continuation

Preserve the approved poster/logo and existing road art. Home exposes normal Flow
Lab alongside Campaign and Spiral. Settings uses a responsive two-column grid and
separate Music/Master controls; obsolete normal movement-mode choices are removed.
No level geometry, world materials, branding assets or visual movement shortcuts
are authorized by this presentation work. See Docs/PRODUCTION_0150.md.

## 0.13.0 physical gameplay-quality rejection

The user's S23 captures and report supersede assumptions that the slice's routes
or art are nearly accepted. [Evidence](Docs/Quality130Evidence/README.md) identifies
Abyss's flattened Restore marker, opaque Boost clutter and tiled landing field,
and Foundry's near-touching route. The three September concepts remain the
quality direction; none is evidence of achieved runtime quality.

Make route rhythm and useful faster lines visible through shape and composition.
Windward galleries/pressure fins/direct chord and Foundry's intake/transfer/cooling
beats are the present correction targets. New gallery skins are the entire support
surface with modeled panel bevels, dark structure and sparse material inlays;
never put a cap onto an existing pad. Large courts must not become repeated pad
fields. Retain the mechanical Boost art, remove the upright bright bars and use
short activation-only feedback. Restore uses a flat corner signal with an activation pulse, leaving the landing clear.

Three existing skies gain adjustable cloud opacity/ceiling using the existing
continuous directional noise; no volumetric renderer, screen tint or extra lights.
Physical visual quality and sustained mobile cost still need S23 testing. These
changes are a correction candidate, not final-quality acceptance or a new world.

## September 9 visual-reference update — current 0.12.0 milestone

The user supplied three finished-game concepts after delivery of the playable
slice. [Reference index](Docs/VisualReferences/README.md) archives the originals
and their interpretation; ADR 0025 supplements ADR 0024 without restarting work.
Inspect both `Docs/VisualReferences/target_gameplay.png` and the new
`Docs/VisualReferences/2026-09-09_gameplay_quality_target.png` before major visual
work. The newer image is the primary finish/composition benchmark where they
conflict. It is aspirational; the current build has not reached that quality.

Prioritize environmental composition, warm light with readable shadows, coherent
ivory/teal materials, atmospheric depth, visible route flow and huge open sky.
Use architecture, vegetation and waterfalls to frame each world's existing
identity. Translate these qualities through actual Unity assets and the mobile
budget; never trace the route, copy the HUD or change accepted movement/FOV,
collision, Restore, Patch, rank or progression rules to resemble a concept.

The main-menu reference establishes a gold Campaign primary action over a calm
world backdrop, with readable secondary actions. Build responsive real Unity UI
with separate visual assets, safe areas and functional buttons. Incidental copy,
version numbers and experiment prominence in the image are not requirements.
The loading reference establishes white Ryder's Road branding on navy for normal
transitions; Jesaias remains secondary. Preserve visible unscaled runtime loading
animation, actual readiness and immediate activation without artificial delays.
Use approved branding sources; no flattened interface or arbitrary font logo.

This update archives direction and production constraints. The delivered 0.12.0
APK and its prior verification remain unchanged; its existing runtime logo is
not yet the new white reference treatment. Completed slice work remains the base.

## 0.12.0 vertical-slice authority

ADR 0024 and Docs/PRODUCTION_0120.md supersede older stop/next-world proposals.
The user selected accepted Campaign controls after S23 testing; preserve that
foundation and isolated trials. Five existing sequential roads form the compact
slice, with redesigned Windward v2 as the finale. Worlds 006–008 remain unbuilt.
Managed loading uses the approved game logo and budgeted world construction;
SceneTransitionHost still owns readiness/recovery. Mountain has a rebuilt, rotated
cascade landmark, and opt-in near shadows/quieter fill support world depth.
V4 and stable IDs are unchanged. No physical approval or Gold designation.

## 0.11.0 Campaign presentation and World 005

ADR 0023 and Docs/PRODUCTION_0110.md supersede historical production stop gates.
Accepted Campaign controls and all isolated 0.10.0 movement trials remain intact.
World 005 is Windward Observatory, using reserved `module.005.foundry-pulse`;
Spiral stays separate. V4 identities and persisted meanings are unchanged.
Continuous directional skies and opt-in profile lighting replace Campaign's
mismatched illustrated cubemap. Loading keeps the existing transition host with
an editable Jesaias emblem and lightweight opacity pulse. Physical S23 approval,
rank calibration, final audio mix and sustained performance remain open.

## 0.9.9 physical readability and shared visual direction

User-reported S23 feedback rejects the Foundry cross-course overhead beam and
stranded decorative ledges. The crane now sits beside the course. Solid non-route
scenery recovers on landing; legitimate route supports and shortcuts remain.
Camera tricks must not hide composition failures. Easier ordinary completion is
acceptable; visible flow and optional mastery matter more than escalating gaps.

Reviewed target_gameplay.png, Sky City 093 opening, Mountain 095 opening and
Foundry 099 gameplay views. The shared ivory landing surfaces already provide a
recognizable route layer, but their dense bevel/vent detail and bright polished
response contrast strongly with flat Foundry masses and coarse faceted geology.
Mountain's fine stone texture over large facets introduces a third detail scale.
A common color filter would leave those differences intact.

Carry the following direction into the next complete world and deliberate local
asset revisions, without silently rewriting physically accepted Sky City:

- Keep clean ivory tops/dark teal undersides as route anchors; cyan remains an
  active/mechanical signal. Use brass sparingly at joints and the Patch destination.
  Each world gets one dominant local accent, rather than unrelated asset palettes.
- Prefer matte ceramic/stone and satin structural metal. Reserve brighter/glossier
  response for glass and small functional accents. Match neighboring asset
  roughness and highlight size before adjusting lighting or grading.
- Use a small number of broad panels, readable edge bevels and structural bands.
  Keep micro-detail subordinate to landing edges at phone resolution. Avoid
  mixing fine noisy texture grain with untextured hero masses at the same scale.
- Repeat restrained paired brackets, inset panels and broken/complete rings as
  construction motifs across distinct world silhouettes. Route blocks should look
  attached to these places, not borrowed from a different game. Do not recreate
  branding; retain approved logo/icon sources.
- Preserve broad sky openings, soft depth separation and clear jump view cones.
  No full-screen tint, heavy bloom, universal floor or effects blanket in this pass.

Only the Foundry composition/recovery changes ship in 0.9.9. This investigation
sets a shared direction; it does not claim that cross-world cohesion is finished.

## 0.9.8 Solar Foundry

0.9.8 Solar Foundry adds original vermilion vessels, dark steel braces, ivory radiator fins and blue solar collectors. Intake, crane, reactor and crown form distinct route landmarks. Shared gameplay surfaces retain their established colors. Near architecture has matching collision; far scenery stays outside traversal. Five actual-FOV views were reviewed, but physical art approval and Gold status remain open. See Docs/PRODUCTION_098.md.

## 0.9.7 autonomous production continuation

The user's 2026-09-08 mission authorizes independent production beyond historical
STOP gates; candidate movement promotion still requires physical approval.
ADR 0019 and Docs/PRODUCTION_097.md record the isolated Flow Lab landing-view
comparison, ordinary precision room, session-best isolation, contact foley and
Ancient Abyss startup preservation. Campaign motor/input/profile/save/content
bytes are preserved. Current verification and artifact: Docs/AI_HANDOFF.md.
No physical approval, final audio mix or production freeze is claimed.
## Flow Lab 0.9.6 prototype boundary

Training uses readable plain surfaces, edge/timing marks, a cyan surf ramp and
gold finish markers with the existing sky. This is functional movement-test
content, not a Campaign art pass or a new visual production standard.

<!-- Current follow-up: 0.9.5-startup-course-clearance -->

0.9.5 addresses physical startup/UI and course intersections. See
Docs/S23_095_FIRST_THREE_FEEDBACK.md and ADR 0017. Module 002 v3 / 003 v7 retain
stable IDs and the unchanged motor; whole-path clearance supplements top-ray and
endpoint traversal tests. Native startup uses the approved loading poster.

# RYDER'S ROAD Art Direction

## 0.9.4 Mountain World authority

The user accepted Module 001 on physical S23; freeze its successful direction. Module 002 is the authorized mountain production target: geology dominates, five places climb toward Summit Patch, a split-peak waterfall anchors the landscape, and open sky/deep gaps separate major masses. Module 003 stays unchanged.

## 0.9.3 Sky City authority

The new user brief authorizes Module 001 production. Every level is a memorable place containing a parkour course: readable route, supportive near world, composed mid/far skyline, deep cloud ocean and open sky. Sky City uses ivory civic architecture, teal glazing, restrained brass, planted terraces and a sun gateway. No floor cards or false landable background roofs. See Docs/S23_093_SKY_CITY.md.

## 0.9.2 fullscreen background policy

Supplied menu/loading artwork and video use COVER with modest cropping, never
pillarboxing. Backgrounds fill the display independently of foreground safe-area
layout. This supersedes 0.9.1 aspect-fit presentation. No menu redesign or world
production is authorized before the frontend physical S23 gate passes.

## 0.9.1 physical-QA continuation

Near landable architecture is now solid. Three repeated arches become ruins/collapsed walls, six buttresses connect route to mass, mid/far scenery moves outside the play corridor and Boost relay sits beside its landing. Five-place art is a physical QA candidate, not certified Gold. Preserve bright sky, depth, vegetation, palette and hidden arms. See Docs/S23_091_CONTINUATION.md.


## Current production gate — 0.9.0

The master campaign brief in `Docs/ASTRA_PRODUCTION.md` supersedes historical
fixed-camera, visible-arm, grounding-disc, mist-floor and route-arrow proposals.
Module 003 is still the sole production art target. Its textured foundations
are narrowed and lowered 9 m to separate non-colliding shelves from gameplay.
A low-poly stone replacement was rejected and discarded. The repeated arch/block language is not accepted as final Gold-standard art.
Keep successful sky/depth and clear landings while refining five distinct places.
Do not spread unfinished composition work into additional levels.


## Phase 0.7.9 Module 003 Production Target

Module 003 is organized as places rather than scattered inventory: arrival
sanctuary, motion gallery, instability ruin, and Patch sanctum. The playable
route remains the highest-contrast layer. One colossal broken temple is the
far hero anchor; secondary ruins are quieter mid-depth silhouettes and the
mist ocean stays at least 48 m below gameplay. Ordinary blocks use clean ivory
tops, strong edges, and dark undersides with no route-center helper geometry or
continuous VFX. Only active mechanics communicate through brief pooled effects.
The rejected black grounding disc is absent.

## Phase 0.5.6 Visual Lock

The runtime sky uses the mobile-safe `MAT_RR_Skybox_Seamless` procedural
material. The older flat panoramic image remains a visual reference only and
must not be wrapped as a 360-degree sky because its edge mismatch creates a
visible seam. Future cloud or distant-world layers must also be genuinely
seamless or use deliberate geometry/billboards.

First-person arm acceptance is visual at the exact gameplay hierarchy. The
world camera remains the 94-degree gameplay view; Ryder Arm V2 is rendered by a
dedicated first-person arm overlay camera so arm FOV can be tuned separately.
`HandPoseLab` is the canonical pose review scene. Forearms enter from the lower
corners, hands extend into the scene, fingers point forward, palms face mainly
down, and the central landing view stays clear. Axis/vector checks are
diagnostics only; rendered camera view decides acceptance.

Phase 0.6.9 replaces the visible Meshy arm with Ryder Arm V2. The approved
static target uses full upper arms entering through the lower corners, real
bent elbows, opposed inward palms, upward-reading thumbs, relaxed fingers,
natural wrist continuation, equal visual length, and a clear lower-center
landing view. No synthetic sleeve or cutoff cover is part of the V2 silhouette.
Review remains at the exact production hierarchy and 94-degree FOV.

Phase 0.7.0 keeps Ryder Arm V2 active but moves presentation to the
`FirstPersonArms` overlay layer with a 70-degree arm camera. The target read is
closer to natural first-person locomotion: a calmer base pose, subtle idle,
alternating run swing, jump/rise/fall posture changes, and landing compression.
All of this is presentation-only and must keep landing readability ahead of arm
motion. Physical-device human acceptance remains pending.

Phase 0.7.1 is the final planned POV arm tuning pass. The arm camera uses
78-degree FOV, the hands sit farther outward and slightly lower/back, wrist
pronation is less inward, and animation is clamped so the arms frame gameplay
instead of enclosing the center view. Physical-device human acceptance remains
pending; no phone was connected during this pass.

Phase 0.7.2 reference-locks that presentation toward the user-supplied
first-person gameplay image at
`C:\Users\lin4s\Downloads\0193c298-1894-4739-b0e9-fc5c1520a8b2.png`. The
target is lower-corner forearms, hands relaxed in the lower third, inward palms,
strong route visibility through the center, and no arm dominance over the next
landing. The dedicated arm camera uses 68-degree FOV; the world gameplay camera
remains 94 degrees. Physical-device human acceptance remains pending; no phone
was connected during this pass.

Phase 0.7.3 is a recovery pass because the 0.7.2 visual result placed the
hands too low and too hidden to read as natural running arms. The new target is
boring and athletic: lower-corner arm entry, hands visible in the lower
quarter/lower third, comfortable separation, inward palms, upward/outward
thumbs, and a clear landing lane. The dedicated arm camera uses 72-degree FOV;
the world gameplay camera remains 94 degrees. Physical-device human acceptance
remains pending; no phone was connected during this pass.

Phase 0.7.3 campaign recovery gives the first three normal Campaign modules
distinct reusable world identities. Module 001 uses Sky City with huge
non-playable skyscraper silhouettes emerging through a low mist ocean. Module
002 uses Mountain Sky with giant rock peaks and sparse vegetation below the
route. Module 003 uses Ancient Abyss with colossal ruin/tower forms sinking
into cloud depth. These assets are visual-only, collider-free, and subordinate
to route readability; the Spiral is intentionally not the main art target for
this pass.

Phase 0.7.5 makes Module 003 Ancient Abyss the first gold-standard visual
benchmark. The playable lane must feel open: near pieces are modest
below-platform supports, while large architecture lives in authored mid, far,
and lower-abyss depth bands. The hero temple sits far/down across the void,
secondary aqueduct/spire/island beats frame the climb, and a continuous
multi-layer mist ocean provides depth without fog cards, slabs, or landable
cloud shapes. Arms are temporarily hidden by profile flag so the slice can be
judged without hand artifacts, and debug telemetry is hidden by default.
Modules 001, 002, and The Spiral are not visually re-polished in this pass.

Spiral uses five intentional landmarks: a lower tree island, low ruin, mid
arch, upper cyan-energy landmark, and summit pillar/Patch composition.
Automatic distant fragments remain disabled for this production module.

Mr Ryder is a coder and game engineer stuck because his game contains bugs and
unfinished systems. The player exists inside that code and repairs broken
sections by completing first-person block parkour and reaching a Patch Block.
Phase 1 expresses this only through temporary laboratory labels.

The permanent visual reference is
`Docs/VisualReferences/target_gameplay.png`. It is the long-term gameplay
presentation target and must be inspected before visual production decisions.
It was added from the supplied August 11, 2026 concept image and represents the
desired first-person block-parkour readability, color energy, mobile framing,
and sky/void presentation.

Preserve landscape mobile framing, a readable first-person route, expansive sky
and void, stylized floating geometry, energetic color, mechanically distinct
surfaces, unobtrusive gloves/hands, transparent controls, and a minimal polished
timer/rank language. Future water, boost, checkpoint, shortcut, particle, and
set-piece effects should reinforce gameplay readability.

Permanent maps should be memorable places first and obstacle sequences second.
Favor large continuous routes such as vertical spirals, tower climbs,
waterfall ascents, broken bridges, floating archipelagos, shafts, ruins, and
mechanical structures. The player should be able to glance across a map and
understand the destination, the route, and how far they have climbed.

The look should be original and polished-indie. Do not copy Minecraft assets,
textures, branding, characters, UI, sounds, maps, or exact visual language.
Phase 1 uses original colored primitives, translucent controls, simple
placeholder gloves, restrained particles, and generated placeholder tones.
These validate readability and movement presentation only; they are not final
art or narrative delivery.

Phase 1B Visual Prototype Pass 1 uses bright URP-safe primitive materials,
dark undersides, edge lines, a darker readable void, cyan Restore Point
language, warm temporary Patch Block color, and circular translucent mobile
controls. These are still temporary readability aids, not final production
assets.

Phase 2 extends the prototype language with module visual/environment profiles,
mechanically distinct block colors, transparent water-flow volumes, bright
boost surfaces, crumble warnings, sky/void lighting, and decoration blocks for
landmarks. Module 03 is the first compact vertical-ascent proof around a central
floating structure; future production maps should be much larger and more
recognizable while preserving block-parkour readability.

Phase 3 adds temporary rank/HUD presentation. Bronze should feel like a valid
repair, not failure. Silver, Gold, and Diamond are mastery goals. Temporary
badge colors are bronze, cool silver, warm gold, and icy cyan diamond; visuals
must remain data-driven and separate from ranking logic.

## Phase 3B Visual Pillars

1. Readability before detail. The next landing must always be obvious.
2. Simple shapes, strong composition. Macro route design matters more than
   tiny texture detail.
3. Color communicates gameplay. Restore, Patch, Boost, Moving, Crumbling, and
   Water must be recognizable while moving quickly.
4. Huge sky, huge depth. RYDER'S ROAD should feel vertically enormous.
5. Polished indie, not Minecraft copy. Blocks are the gameplay medium, not the
   brand identity.
6. Flow must remain visible. No effect may hide the route.
7. Mobile first. Every visual effect must justify its cost.
8. Levels are places. Modules are memorable environments that happen to be
   parkour courses.

## Phase 3B Direction

`0.3.5-visual-slice` is the first true visual vertical slice, not final art.
It uses a deep blue zenith, saturated cyan mid-sky, violet/pink horizon haze,
warm sun, cool ambient fill, and purple-blue Null Space depth. Clouds are
simple low-cost cuboid clusters. Distant fragments and tower silhouettes create
scale without route clutter.

Parkour blocks use light cool stone tops, darker side faces, dark undersides,
and small edge trims. Decorative islands use irregular stacked chunks with dark
tapered undersides. Vegetation is low-poly and chunky, placed away from jump
edges. Water is bright cyan with transparent surfaces and foam streaks that
show flow direction.

Mechanic color language:

- Restore Point: icy cyan glow, simple frame, pulse, and split feedback.
- Patch Block: gold/orange shell with a bright repair core and small floating
  fragments.
- Boost: warm orange/yellow surface with directional arrows and energy streaks.
- Moving block: cooler blue body with cyan accent.
- Crumbling block: unstable warm stone with red/orange fault lines.
- Corruption/code accents: subtle violet marks or broken fragments only.

First-person gloves are compact, chunky, dark navy/charcoal with warm orange
plates and a small cyan technical pin. They stay in the lower corners and must
not hide landing surfaces.

Phase 0.6.8B rigged hands use the same navy/orange material family in a clean
mobile-lit static pose. The target read is true source-right/baked-left
handedness, relaxed fingers, visible hand backs, no runtime negative-scale
mirror, no dark/scuffed metallic-roughness artifact, and no locomotion
animation while the pose is under approval. The flatter palm/thumb diagnostics
are acceptable only insofar as the rendered hand-back pose reads natural in
gameplay.

HUD and controls should feel like a clean mobile game overlay: top-center
timer/rank text, subtle translucent backing, white/cyan text, rank colors from
data, and low-opacity joystick/jump controls. Debug panels remain separate.

Performance rules: use URP-safe materials only, prefer shared runtime
materials, avoid expensive custom shaders, volumetric effects, realtime GI,
large transparent layers, many realtime lights, and excessive particle counts.
Any magenta gameplay material is a validation failure.

## Phase 0.4.0 Visual Identity Direction

`0.4.0-visual-identity` strengthens the target direction without changing
gameplay feel. The current identity is bright sky-parkour over deep Null Space:
deep blue zenith, cyan mid-sky, violet haze, warm peach horizon, cool stone
tops, dark undersides, saturated cyan water/restore accents, and orange/gold
boost/patch accents.

The reusable runtime block kit now treats route blocks as gameplay colliders
with visual-only child detail: top highlights, inset panels, trims, side shade,
undersides, and small original circuit ticks. Future art may replace these
temporary primitives, but must preserve the same route readability and never
make decorative pieces look like required landings.

Module 03 is the hero-map preview for this phase. It adds more surrounding
islands, towers, trees, clouds, water, boost, and Patch direction around the
existing compact vertical ascent. It is still not a final large map; it is the
current benchmark for the intended RYDERS BLOCK feel.

Visual work must stay independent from movement. Colors, materials, decorative
meshes, benchmark anchors, and VFX are presentation only; they must not change
GoldSrc-style acceleration, air control, bhop, surf, camera input, checkpoint,
restore, rank, score, progression, or save behavior.

## Phase 0.4.5 Spiral Showcase Direction

`0.4.5-spiral-showcase` makes `THE SPIRAL` the first large map reference. The
visual priority is visible physical progress: the player should see the broken
tower, the lower route falling away below, future sections above, and the
summit Patch Block payoff.

The standard route should be readable in motion and support ordinary Bronze
completion. Optional inner gaps, water speed, boost overshoot, surf, and
high-risk drop lines may look tempting, but they must remain faster expressive
routes rather than required progression.

The map remains prototype art. It uses the current runtime block kit,
profile-driven colors, and mobile-balanced density; future production art may
replace these primitives only if route readability and mechanic identity stay
clear.

## Phase 0.4.6 Alpha Vertical Slice Lock

`0.4.6-alpha-vertical-slice` keeps THE SPIRAL as the single alpha focus. The
visual work is limited to clarity and presentation hygiene: normal play should
not be crowded by development shortcuts, benchmark views should cover the
water and boost sections, and the HUD/menu should stay compact on mobile.

This pass does not approve final art. Physical Android review still needs to
judge readability, safe areas, fullscreen behavior, and performance before the
look can be treated as alpha-approved.

## Phase 0.4.7 Brand Direction

`0.4.7-brand-alpha-presentation` locks the player-facing product identity as
`RYDER'S ROAD`. The approved wide logo and app icon sources live under
`Assets/Branding/Source`; runtime UI and Android derivatives live under
`Assets/Branding/Runtime`, `Assets/Branding/Resources`, and
`Assets/Branding/Android`.

The black-backed supplied logo source was preserved separately, while the
runtime UI logo removes the black backing into alpha. The Android icon uses a
legacy square export plus adaptive foreground/background layers, with mask
previews in `Docs/Branding`. These previews are desktop/artifact checks only;
physical launcher appearance must still be verified on Android hardware.

The title screen should feel like the app-store art direction: bright sky,
floating stone route, cyan technical energy, warm orange/gold route markings,
and a clean logo-first front door. This phase does not approve final art and
does not redesign Smart Camera, GoldSrc-style movement, scoring, progression,
or map rules.

After Samsung Galaxy S23 feedback on 2026-08-16, the first follow-up visual
pass leaned away from a flat blue prototype read: warmer horizon, deeper Null
Space, stronger route-side contrast, warm/cyan landing accents, and a
visual-only Patch beacon. These are still temporary production-direction cues,
not final art assets.

A later Spiral readability follow-up also clears the opening composition:
the player and `Benchmark_Spiral_Start` face slightly into the first route
curve, and nearby decorative slabs/clouds are kept out of the immediate
first-view ceiling. This is presentation only and does not alter movement,
camera input, Restore, rank, or completion rules.

## Forward Production Direction

RYDER'S ROAD should split cleanly into two player-facing modes:

- Campaign: short, direct modules that teach one idea at a time and grow more
  difficult across a normal level sequence.
- Spiral: one long continuous climb to the top, built around macro landmarks,
  fair Restore spacing, and optional mastery lines.

The visual identity should move away from plain colored blocks toward original
textured stone-tech platforms, a beautiful sky/void backdrop, clearer macro
composition, and animated first-person presentation. Generated project-owned
texture concepts may be used as temporary production-direction assets, but
they must stay readable, mobile-friendly, and free of copied game styling.

The route-readability follow-up adds visual-only warm/cyan flow chevrons to
normal, precision, and crumbling route blocks. They should make the standard
Bronze path readable during thumb play without changing colliders, movement,
camera, Restore, Patch, rank, score, or rewards. Mechanic blocks with their own
direction language, such as boost, water, surf, and moving platforms, should
not also receive generic route chevrons.

The Spiral start cleanup moves the player onto a clearer runway before the
vertical route mass, keeps the decorative tower as an off-axis landmark, and
uses a procedural blue/violet gameplay skybox instead of rectangular sky
backdrop planes. This improves the first read, but it is not final level art:
the Spiral still needs a deeper production rebuild with stronger silhouettes,
more altitude separation, authored set pieces, and fewer stacked-route overlaps.

## August 18 Production Target

The August 18, 2026 user-supplied art references define the next production
target direction: bright blue sky, floating ivory stone islands, orange/gold
route strips, cyan energy/water, chunky but polished stone-tech towers, small
green vegetation caps, and stylized first-person hands with orange wrist
plates. Text inside reference images is not instruction unless separately
requested.

The first implementation pass uses original project-owned generated texture
assets for stone, gold route plates, and cyan energy. These are temporary
production-direction assets, not final outsourced art, but the game should now
move away from flat prototype color blocks.

Imported Meshy or bespoke platform models should enter through
`ModuleVisualPrefabLibrary` as visual-only role skins. Keep them collider-free
after runtime preparation, sized to preserve the authored gameplay block, and
readable from the 94 degree first-person camera before spending time on fine
surface detail.

Default first-person FOV is 94 degrees. Hand animation should include running
pump, jump reach, falling brace, landing recoil, boost reaction, and water
pulse while keeping landing zones visible.

Skill speed remains optional. Normal Campaign and Spiral Bronze routes should
use fair one-jump spacing; bhop, air-strafe, surf, water, boost, and future
flow acceleration should create faster optional lines rather than mandatory
block skips.

## Phase 0.5.0 Meshy Art Kit Direction

`0.5.0-meshy-art-kit` begins replacing prototype block presentation with the
real Meshy environment kit. Imported models must stay original project-owned
visual skins over the existing gameplay colliders through
`ModuleVisualPrefabLibrary`; art never changes GoldSrc-style movement,
Restore, Patch, rank, score, save, or authored jump spacing.

The environment kit should read as bright sky parkour: ivory carved stone,
orange/gold route faces, cyan energy/water accents, floating ruins, small
vegetation caps, and clear macro landmarks. Detail is secondary to mobile
readability at 94 degree FOV.

The production `RR_FP_Arm_Right` candidate is the Meshy Neon Vanguard Gauntlet
remesh at 15,400 triangles. The legacy approximately 1.98M-triangle mech
gauntlet source remains reference-only and must stay ignored for runtime.
Runtime arms are presentation-only prefabs driven by `FirstPersonArmProfile`;
they must never affect movement, collision, Restore, Patch, ranks, or saves.

## Phase 0.5.3 Hand Presentation & World Cleanup Direction

`0.5.3-hand-presentation-and-world-cleanup` establishes presentation rules for first-person hands, void/sky atmosphere, and landmark cleanup:
1. **Natural First-Person Hand Pose**: Hands enter from bottom screen corners (`±0.34, -0.34, 0.42`) with wrist leveled (`54°` pitch) so fingers point forward into the world along the player's run vector. Palms face downward toward the ground, with bright orange armor plates sitting squarely on the top surface facing the player. Scale has substantial screen presence (`0.40`).
2. **Bottomless Null Space**: The dark flat bottom box (`Null Space Depth Plane`) and primitive fragments are eliminated. Null Space is an airy, bottomless void backed by the panoramic skybox. Fall detection remains strictly mathematical.
3. **Debug Primitive Removal**: Giant vertical beacon beams, horizontal halo slabs, bright green core faces, and loose floating cubes are removed from Patch and Restore landmarks. Internal technical billboard labels (`PATCHBLOCK`, `RESTORE`) are gated behind debug mode.
4. **Natural Composition Over Primitives**: Primitive cloud clusters and pink underglow slabs are replaced with production Meshy stone islands, ruined pillars, and arches framing the player's ascent.

## Phase 0.6.1 Parkour Feel Direction

`0.6.1-parkour-feel-and-route-rhythm` keeps the 0.6.0 route truth but makes
Crumble a real one-second pressure beat. Spiral should read as safe block,
fast Crumble chain, safe breather, normal parkour, and repeat, with no
decorative Crumble blocks beside the road.

First-person arms remain presentation-only camera-space objects. Their base
pose should favor inward-angled palms, relaxed wrists, equivalent left/right
geometry, and a visible but unobtrusive running pump. Jump, fall, and landing
motion may be stronger than camera motion, but hands must stay below the
important center/lower-center landing view.

The high far `m04.world.far.spire-north` decoration was traced as a likely
source of the unexplained rectangle in the sky and removed. Lower-void depth
should come from distant cloud banks and faint ruin silhouettes below the
route, never an obvious flat floor or reachable platform.

## Phase 0.6.2 S23 Feedback Polish Direction

`0.6.2-s23-feedback-polish` responds to S23 feedback that the 0.6.1 hands were
still too small, not inward enough, and a little stiff in a "zombie" pose.
Hands should read as relaxed, mirrored, larger camera-space gauntlets that stay
below landing targets and never imply gameplay collision.

Spiral's center composition should avoid stacked structures on the same middle
axis. The visual core can have several vertical beats, but lower, mid, and
upper masses should be offset enough to read as separate ruins wrapped by the
route.

The lower void should feel more ominous than 0.6.1: distant fog and red glow
may sit far below the playable path, but they remain visual-only atmosphere
with no route, collision, Restore, Patch, rank, or save meaning.

## Phase 0.6.3 Hand Pose, Collision Truth, Abyss Direction

`0.6.3-hand-pose-collision-truth-abyss-depth` corrects the 0.6.2 hand pose
without redesigning the first-person presentation. Fingers should point forward
along the route, palms should stay only slightly inward/downward, both hands
should sit farther inside the screen, and the right hand should remain visible.
The hands are still camera-space presentation only.

Anything that looks landable must either have truthful collision or stop
looking like a platform. Water/flow volumes and visual-only accents should read
as energy, mist, ribbons, filaments, or vertical signals rather than broad blue
jump surfaces.

Spiral abyss atmosphere should be ominous and deep, with layered fog and red
glow far below the route. It must not sit near gameplay, hide landing reads, or
create the impression of a reachable lower platform.

## Phase 0.6.4 Natural Hand And World Composition Direction

`0.6.4-natural-hand-pose-and-world-composition-pass` moves the hands toward a
standard athletic parkour-ready pose: lower, closer to the body, thumbs angled
up, palms facing each other, fingers generally forward, and only a slight
inward cant. Motion should support the pose without stretching the hands flat
into the route.

World polish should make each module feel like a place, not a route suspended
in empty blue. Use clustered islands, distant silhouettes, vertical haze, and
far-below ruins sparingly to frame the route while keeping landing reads clean.

Water/flow and abyss atmosphere must avoid platform-shaped slabs. If a visual
is not landable, it should read as segmented energy, mist, vertical haze,
filaments, sparks, silhouettes, or deep background mass.

## Phase 0.6.5 Surgical Recovery Direction

`0.6.5-surgical-recovery-and-correct-hand-pose` treats the 0.6.4 hands and
abyss composition as regressions to recover, not a base for more filler. Hands
must stay visible in the lower left/right of the 94 FOV frame, with clear
left/right ownership, inward-facing palms, upward thumbs, forward fingers, and
no wrist crossing or interlock.

Depth must be subtle, authored, and clean. Do not use giant transparent cubes,
planes, gradient slabs, or special veil geometry for abyss mood; prefer fewer
standard cloud masses and small red glow accents placed far below the route.

## Phase 0.6.6 First-Person Hand Pose Lock

`0.6.6-first-person-hand-pose-lock` establishes the canonical static neutral
pose at the exact production hierarchy and 94-degree FOV. Fingers point into
the world, palms face each other, thumbs point up, and equal forearms rise
diagonally from below the lower corners so bent elbows are implied while the
landing center stays clear. The profile is static until physical approval;
running, jump, fall, and landing arm animation are deferred.

## Phase 0.6.7 Safe Rig Integration Direction

`0.6.7-safe-rig-integration-and-locomotion` may use the rigged Meshy arm only
if it preserves the 0.6.6 camera-space read: separated lower-corner forearms,
forward fingers, inward/opposing palms, upward thumbs, and a clear landing
center at 94 FOV. The rig is allowed to add subtle life to idle, run, jump,
fall, and landing states, but it remains presentation only. Any deformation,
finger curl, wrist motion, or arm pump that harms landing readability or
Classic movement mastery should be tuned down or reverted to the legacy
approved prefabs.

The mirrored left mesh is baked geometry, not a negative-scale runtime copy.
Hand orientation must be judged from the composed prefab/profile hierarchy and
the rendered HandPoseLab image, with axis measurements used as supporting
regression proof.

## Phase 0.6.8 Rigged Hand Pose Recovery Direction

`0.6.8-rigged-hand-pose-recovery` treats the 0.6.7 rigged animation read as a
pose regression and returns to a static approval baseline. The rigged hands
should sit low and outward in the bottom corners, read as relaxed running hands
rather than spears, show slightly curled fingers, keep palms facing each other,
and keep thumbs angled upward. The arm bases should enter naturally from below
the frame without a harsh cutoff.

Until the recovered still pose is physically accepted, rigged idle/run/jump/
fall/landing animation should remain disabled or negligible. Do not use
animation, camera changes, route edits, or world polish to hide a bad hand
pose.

## Phase 0.7.6 Readability And Ancient Abyss Direction

Normal gameplay frames the route at a fixed `-9` degree pitch and `94` vertical
FOV. The subtle Ryder contact shadow is world shading, not UI: dark, soft,
height-faded, surface-clipped, and absent when no receiving surface exists.
Gameplay platforms must expose a bright or role-readable top, a restrained
edge band, and a darker navy underside at the true collider footprint.

Module 003 is the sole authored art target. Its composition is one memorable
vertical broken temple plus three secondary groups, with monumental forms in
mid, far, and lower depth bands. Large architecture stays away from the route;
open sky, platform contrast, and negative space remain dominant. The lower
world is a continuous soft mist ocean with structures entering and vanishing
through atmosphere, never a flat cyan floor or visible card stack.

## Phase 0.7.7 Platform Visual Truth Recovery

Canonical Ryder blocks must read through their own modeled/textured top,
silhouette, edge, underside, and lighting. Do not place a second slab, cap, or
surface overlay on ordinary gameplay blocks. The authored root collider remains
the exact landable footprint; presentation children carry no collision.

Module 003 keeps open sky, one hero temple, and a few lower/far secondary
groups. The rejected near-summit tower is removed, secondary ruins sit farther
and lower, and denser slate-blue mist makes structures disappear into depth
without creating a pale cyan floor or importing new art.

## Phase 0.8.0 Curated World Rule

Every Ryder's Road level is a memorable place that happens to contain a
parkour course. Open air must coexist with an authored world envelope: clean
gameplay, sparse near-world anchors, two or three coherent mid-world places,
monumental far silhouettes, and a deep atmospheric layer below. Do not build a
generic Ryder void and swap isolated props.

Module 003 proves this with an Ancient Abyss temple island, broken procession,
reclaimed sanctuary, drowned lower city, cloud ocean, and distant civilization
silhouettes. The route remains the sharpest and most readable layer. No visible
world floor, fog slab, cloud card field, stretched blockout wall, or random
asset scatter is acceptable. Negative space and open sky remain deliberate.

## Phase 0.8.1 Hand-Authored Place Standard

Module 003 uses five connected architectural places rather than one route with
dressing. Overlapping ruin-island foundations visually support the route,
while towers, arches, vegetation, and energy accents establish orientation and
place identity. Monumental arches are sparse framing elements, not a repeated
prop pattern. The Shattered Crown remains one asymmetric far-world hero.

Delete flat stock walls, gates, cliffs, or towers when their scale makes them
read as blockout geometry. Improve a world through mass, silhouette, depth,
and relationships, not asset count or VFX. Sky assets must survive a full
360-degree camera inspection without a visible wrap seam.
