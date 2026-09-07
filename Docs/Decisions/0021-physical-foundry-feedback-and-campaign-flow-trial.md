# ADR 0021 — physical Foundry feedback and Campaign Flow trial

2026-09-08. The user reports testing 0.9.8 on Samsung S23: easier ordinary
completion is acceptable, the overhead beam blocks the jump view, and solid
lower scenery strands the player. The user prefers the Flow Lab candidate over
fully manual controls. This is preference evidence and authorization to evaluate
Flow on Campaign routes, not approval to replace Campaign movement globally.

Move the transfer crane to the outside of the course. Keep the ferry, bracket
shortcut, route spacing and difficulty. Mark Foundry's non-route solid architecture
with explicit AuthoredSurface.RestoreOnLanding. A separate contact relay requests
the existing counted-fall Restore on walkable contact normals; side/underside
contacts do nothing. Default marker value is false, so existing playable
architecture elsewhere retains its meaning. Keep exact collision, checkpoint
semantics, fall yaw preservation, module resets and brief spawn protection.
No ghost scenery, catch floor, universal platform or camera workaround is added.

Foundry advances to content version 2; stable IDs and historical PB metadata stay.
V4 needs no migration. The change is intentional recovery behavior for non-route
Foundry scenery, not a new movement rule or campaign difficulty increase.

Home exposes Campaign Flow Trial, with real roads 001–004 and three comparisons:
A accepted motor/saved controls; B unchanged mastery motor + Flow Direct/manual
pitch; C the same Flow motor with 16-degree initial pitch and bounded descending
landing view. Existing right-thumb vertical drag remains available. Landing view
stays presentation-only: no platform targets, acceleration, optimal strafe,
jumps, landing magnetism or added yaw energy. Classic and Editor paths retain
manual control. No new permanent gameplay buttons or movement tuning.

Trial state is session-only. Normal module selection and home clear it, Retry
retains it. Trial attempts/completions/rewards never write to the save service;
local best times separate stable road ID, content version and comparison mode.
Controls use the existing session override; trial menus omit persistent control
changes. The accepted motor and candidate assets are unchanged and recoverable
from checkpoint 38d222a. A new physical decision is needed before promotion.

Game feel, camera/route readability and skill advantage take precedence over
mechanically escalating difficulty. Compare ordinary jumps, linked jumps, arcs
and optional lines on the real worlds before another motor-number tuning pass.
Art cohesion investigation belongs in ART_DIRECTION.md; no global filter or
full-game visual rewrite is authorized by this implementation decision.
