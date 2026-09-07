# ADR 0008: Lock default mobile movement to flow steering and auto camera

Status: Superseded for default controls by ADR 0009 - 2026-08-12

## Decision

Default mobile play uses left-thumb movement plus steering, a large right-side
jump zone that fires on touch begin, and automatic first-person camera
presentation. Manual right-thumb look, right-region tap-release jump, and Flick
Jump remain available only through development or legacy profiles.

Left-stick horizontal input is steering intent. The motor rotates player
heading through profile-driven yaw-rate, response-curve, acceleration,
deceleration, ground, air, and surf strength values, then applies throttle
along the current heading. Jump buffering, coyote time, bhop retention,
air-control bounds, surf, restore, and progression rules stay independent.

## Consequences

The default Android experience becomes closer to one-thumb movement plus
anywhere-right jump, with fewer simultaneous camera/jump conflicts. Camera
presentation follows heading directly with automatic pitch/FOV assistance.
Future map design should assume the player can complete normal routes without
manual camera drag, but Editor and legacy manual camera controls must remain
available for development and comparison.

## Supersession

Physical-device feedback after this decision preferred left-thumb movement,
right-thumb camera, and right-side Tap Jump. ADR 0009 restores that as the
default. The flow-steer and auto-camera work remains a development experiment
only.
