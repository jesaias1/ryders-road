# ADR 0032 - Windward movement sequences

2026-09-12. S23 feedback supersedes the fixed-gap cadence of ADR 0031. The player
must be able to read several jumps as one idea; imperfect held-hop entries should
not require an unrelated direction correction after every landing.

Windward content version 6 has four movement sequences: straight acceleration,
sustained left sweep with pressure fins, shallow rising diagonal, and straight
final carry. Three stable Restore identities separate them. All intermediate
landings are shared by slow, flow and expert play. Old arc.07 and arc.08 are
removed; the retired separate chord stays removed. No additional route or motor.

Lateral catch width and longitudinal depth are independent. Actual controller
flights, not a uniform spacing target, inform intermediate catches. Sparse broad
anchors allow releasing hold, stopping and reorienting. Within each sequence,
adjacent centreline heading changes stay below 15 degrees. Recovery turns may
straighten the next sequence. No route guidance is added to player code.

Only Editor-authored route/world data changes. Shared movement compatibility 5,
physics, input/camera, soundtrack and all-face fatal scenery remain unchanged.
V4 history uses the existing content-version buckets; ranks remain provisional.
Wind sails are moved out of route sightlines. Intermediate underside housings
are removed; five dark anchor foundations and the displaced telescope/instrument
remain fatal scenery. No 3DAIStudio production work is authorized in this pass.

Verification adds conservative stop-and-jump, continuous held flow and expert
sequence fixtures, each with left/right offset, early/late and heading variations.
Incoming speed is set only at fixture entry. Subsequent jumps carry real landing
position and velocity; no per-hop state reset or velocity correction. Held pilots
supply a broad centreline stick direction; slow pilots hold one direction in air.
These are explicit test inputs, not runtime assistance or physical feel evidence.
Release-and-stop is tested at every recovery. Deliver APK then stop for S23.
