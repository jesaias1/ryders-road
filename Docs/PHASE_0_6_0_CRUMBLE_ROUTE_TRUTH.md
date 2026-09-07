# RYDER'S ROAD Phase 0.6.0 Report

Target: `0.6.0-crumble-route-truth-and-hand-refinement`

## Delivered

- Replaced 19 ordinary Spiral supports with six mandatory short/medium crumble
  chains, separated by normal blocks and Restore decks.
- Added reliable CharacterController floor-contact activation while preserving
  the trigger fallback, staged Meshy visuals, stationary collision, and
  deterministic reset.
- Replaced two ordinary supports with one required moving crossing into Restore
  2. Removed path-marker cubes and the broad cyan top plate.
- Removed the temporary 11m production Surf panel and its stale route metadata.
- Balanced both arms to equal height, depth, scale, and apparent length; angled
  palms inward and reduced run, jump, fall, landing, and boost motion.
- No movement physics, camera rules, FOV, controls, saves, ranks, or progression
  contracts changed.

## Verification

- Source validation passed.
- EditMode `176/176`; PlayMode `6/6`, including real CharacterController
  floor contact, collapse, and reset on a runtime Spiral crumble block.
- Android IL2CPP ARM64 build succeeded with zero errors and one legacy-icon
  warning.
- APK size: 164,472,892 bytes. SHA-256:
  `8036FB1B4FFAC83992400EBEF49FC9FAF3545873419958AEA3735F88588AA5CF`.
- Samsung Galaxy S23 install/launch passed at 2340x1080 fullscreen. Title and
  grounded Spiral start rendered at 60 FPS without a fatal exception.
- Physical crumble, moving-crossing, full-route, and in-motion hand acceptance
  were not performed autonomously and remain pending.

## Acceptance Gate

Stop here. The user should test crumble activation/timing/reset, mandatory route
placement, the moving crossing, hand pose/motion, route artifacts, complete
Spiral playability, fullscreen, FPS, heat, and pause/resume.
