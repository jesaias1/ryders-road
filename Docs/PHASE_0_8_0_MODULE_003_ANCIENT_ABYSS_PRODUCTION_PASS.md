# Phase 0.8.0 Module 003 Ancient Abyss Production Pass

## Root Cause And Permanent Removal

The giant gray circular floor was runtime geometry created by
`ModuleSceneController.CreateBiomeMistOcean`. Ancient Abyss supplied six
`BiomeMistLayer` entries, including radial `420 x 420`, `560 x 560`, and
`620 x 420` meshes. The runtime generated `Mesh_RR_RadialMistDisc` surfaces
and assigned mist materials. These broad horizontal discs were the exact arena
floor visible beneath Module 003.

The radial-disc generator, mesh cache, mist-ocean material path, and all
Ancient Abyss mist-layer data are removed. Ancient Abyss now requires zero
planar mist layers. No replacement receiver, cloud plane, or billboard field
exists. The disabled `PlayerGroundingCue` remains disabled with no substitute.

## Gameplay

Module 003 content version 4 is a five-act route: Arrival Sanctuary, Destroyed
Bridge, Collapsed Temple, Boost Ascent, and Patch Sanctum. The ordinary route
has 27 truthful solid supports, three approximately one-second Crumble blocks,
one short-cycle diagonal Moving ferry, one directional Boost with a broad
destination, three Restore Points, and one Patch. Every ordinary edge gap is at
most `3.40 m`; the Moving and Boost transitions are intentionally assisted.
Water and Surf are not used.

Two authored mastery lines remain optional: an opening momentum cut and a
moving-platform bypass. They save time without gating Bronze. Rank thresholds
are provisional and require physical S23 timing; every valid completion still
earns at least Bronze.

### S23 Gameplay Correction

The previous Moving block travelled sideways from `(7, 2.4, 32.5)` to
`(1.5, 2.4, 32.5)`. It neither followed the setup-to-exit direction nor
transported the player across the authored gap. The replacement travels
diagonally from `(7, 2.4, 31.5)` to `(-1.5, 2.6, 33.5)`, covers `8.73 m` in
about `2.73 s`, boards within `0.75 m` of the setup edge, and arrives within
`0.05 m` of the exit edge. The direct gap remains over `6 m`, so the ferry now
has a clear Bronze-route purpose. A collider-free broken-bridge rail/tie
composition makes its path readable; the two precision blocks remain an
optional faster bypass.

The previous Boost asked the player to reach a `5 x 5 m` landing centered at
`(3, 7.2, 72.5)` from `(6.5, 5, 61)` with `12.5 m/s` vertical and `7.8 m/s`
horizontal boost. With the shipped asymmetric gravity, the player descended
below that elevated target before covering its required horizontal distance at
ordinary approach speed. The corrected Boost uses `13.8 m/s` vertical and
`8.0 m/s` horizontal velocity toward `(-0.285, 0, 0.959)`, with a
`6.5 x 6.5 m` landing centered at `(3.8, 6.8, 70.1)`. The calculated descending
flight time is `0.730 s`: a nominal `5 m/s` approach reaches within `0.01 m` of
landing center, while slow `3 m/s` and full `7.8 m/s` approaches retain more
than `1.5 m` of projected front/back landing margin. Physical S23 confirmation
is still required.

## Curated World Envelope

The route is framed as a specific Ancient Abyss place:

- Near world: five route-local compositions connect gameplay to a reclaimed
  arrival island, paired moving-bridge heads, a split collapsed court, Boost
  foundations/pylons, and a final sanctum foundation.
- Mid world: a broken monumental arch and reclaimed sanctuary flank progress.
- Hero: a colossal three-tower temple island, great arch, gold crown, and
  restrained cyan beacon remain visible across the route and frame the Patch.
- Lower world: a simplified drowned tower city rises partially through depth.
- Far/below: `Skybox_Ryders_Road.png` provides a continuous cloud ocean,
  distant city silhouettes, and open sky without physical geometry.

The old huge tower, generic decoration islands, radial discs, particle mist
cards, stretched stock wall spans, and mismatched tech-ruin towers are absent
from the Module 003 composition. Existing Kenney castle/nature assets and
project-authored arch/energy prefabs were recomposed; no new third-party pack
was imported.

The route-local pass was hand-authored and then pruned against all five
representative gameplay views. Oversized crowns, wall fragments, independent
watchtowers, and redundant Patch scenery were deleted after they obscured
route flow. The retained pieces are low foundation masses and sparse place
identifiers rather than a field of unrelated props.

## Presentation And Budget

Module 003 uses bright daylight, warm sun, cool depth, ivory/sandstone near
stone, slate/navy structure, small gold accents, restrained cyan energy, and
natural green vegetation. Ordinary blocks remain quiet. Existing pooled
landing, Crumble, Boost, Restore, and Patch feedback is unchanged and bounded
to four prewarmed emitters with an eight-emitter ceiling. World scenery has no
colliders, shadows, reflection probes, or motion vectors. The panorama replaces
transparent mist overdraw and broad mesh geometry.

Five production captures are written to `Logs/Phase080VisualQA`: opening,
early route, middle, high/exposed, and Patch approach. They are the visible
quality gate for this phase.

## Verification

- Source validation: `Logs/phase080-curated-source-validation-final.log`.
- Foundation validation: `Logs/phase080-curated-foundation-validation.log`.
- EditMode: `Logs/phase080-curated-editmode-final-results.xml`.
- PlayMode: `Logs/phase080-curated-playmode-final-results.xml`.
- Visual capture: `Logs/phase080-curated-visual-pruned-results.xml`.
- Android build: `Logs/phase080-curated-android-build-retry.log`.
- APK: `Builds/Android/RYDERS-ROAD-0.8.0-module003-ancient-abyss-production-pass-dev.apk`.
- APK size: `172,114,186` bytes.
- SHA-256: `06F1664D3CDC26B5AF3EE4D9811005F0E0C976117D68C4B2B1F6F1D82780B5EA`.
- Automated result: source/foundation passed, EditMode `196/196`, PlayMode
  `8/8`, and Android ARM64 IL2CPP succeeded with zero errors. The clean build
  reported one aggregate warning for obsolete test-only Unity object-query
  overloads.

Physical S23 gameplay, fullscreen, thermal, FPS, and rank calibration remain
required. No physical-device result is claimed by this phase record.
