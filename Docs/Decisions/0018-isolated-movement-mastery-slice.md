# ADR 0018 — isolated Movement Mastery candidate

2026-09-07. The user explicitly authorizes a focused playable movement slice,
superseding the earlier wait-before-Flow-Lab restriction. Campaign acceptance
and physical movement approval remain open. No 004–008, character or world pass.

Keep ParkourMotor and its production assets. A serialized MovementMastery flag
defaults false. Only Resources/Training/Movement_Mastery opts in (compatibility
2). Production remains compatibility 1. Flow Lab can switch A/B in place and
reset the attempt; Campaign never loads the training profile.

Candidate air acceleration changes velocity only along wish and bounds added
energy at the existing soft limit. It neither turns velocity nor adds a turn
bonus. Flow throttle scales acceleration, while the wish-speed cap stays fixed:
normalizing a diagonal steering/throttle vector must not suppress useful strafe
acceleration. Yaw alone produces no acceleration. Existing overspeed is retained
without adding energy above that speed; hard safety still applies.

Resolve candidate landing before jump eligibility. Buffered takeoff skips ground
braking; a hop within the existing good window preserves takeoff momentum, while
the existing excess-speed timing retention remains. Ordinary first jumps retain
the existing height/apex/retention. No held auto-hop. Surf uses projected gravity,
projected directional acceleration, authored surfaces and a short jump detachment
window. Candidate wall contact clips inward stored velocity. No rail or route force.

Reuse MovementLab scene/player/touch composition with a one-shot frontend launch
request, keeping legacy lab identity and content available. FlowLabSession owns
training evidence and in-memory per-room/profile bests. JSON owns room identity,
geometry and exercise thresholds. No training data enters V4 or Campaign timing.
The required two-thumb mapping is session-only FlowSteerAutoDirect; manual pitch
remains free. Classic and Editor controls remain available. No preference migration.

Physical S23 feedback determines the next iteration; automation cannot certify
feel, controllability, performance, Gold or production freeze.
