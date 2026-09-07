# 0011 - Visual Production And Flow Skill Boundary

Date: 2026-08-18

## Decision

The next production direction for RYDER'S ROAD follows the August 18, 2026
user-supplied sky-parkour references: bright blue sky, floating ivory stone
islands, orange/gold route language, cyan energy/water, readable smaller
platform silhouettes, and stylized first-person hands. Text inside reference
images is not treated as instruction unless separately requested by the user.

Default first-person FOV is 94 degrees for mobile parkour readability. Hands
should have visible running, jumping, falling, and landing presentation, but
remain in the lower corners and avoid hiding landing zones.

Speed carry, bhop, air-strafe, boost, water, and surf remain optional mastery
systems. Normal Campaign and Spiral Bronze routes must stay completable at
intended run/jump spacing without requiring a jump-streak speed increase or
forced block skipping. Future flow acceleration may be explored only as a
tunable optional layer for faster lines.

## Rationale

The game needs to stop reading as plain prototype blocks, but visual polish
must not silently break the movement model that already feels better on phone.
A 94-degree view better supports vertical parkour awareness and hand/route
framing. Skill speed should reward mastery without making ordinary route
spacing unreliable.

## Consequences

- Production art can add original textures, trim, route markings, sky polish,
  icon updates, and hand animation inside visual systems.
- Platform visuals may become smaller/cleaner, but gameplay collider changes
  require explicit level pacing tests.
- Campaign routes teach one idea at a time; Spiral remains one long readable
  climb.
- Tests and validation should guard 94 FOV, visual texture assets, and the
  Campaign/Spiral mode split.
