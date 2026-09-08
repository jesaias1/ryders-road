# ADR 0022 — real-route air control candidate

2026-09-08. The user physically tested 0.9.9 on S23 and reports weak deliberate
air correction across camera modes. The two supplied recordings show Flow Lab
landing chains and a completed Foundry trial; successful completion is not feel
approval. This pass is movement-only and must STOP for physical feedback.

Keep accepted compatibility 1 and previous mastery compatibility 2 assets intact.
Add training-only movement.real-route-v1, compatibility 3. Campaign trial A uses
accepted controls, B/C share the new physics with manual/landing view, D retains
0.9.9 Flow. Normal Campaign is unchanged. Flow Lab cycles new/accepted/previous.
Session times stay separate; no candidate attempts or rewards enter V4.

Diagnosis: Flow used x solely for yaw and y for forward acceleration. Wish stayed
on body forward, making a late correction wait for the view to turn. Projected
wish cap suppresses acceleration while velocity already exceeds that projection.
The additional old energy-intersection limit suppressed all useful positive or
perpendicular wish acceleration at the soft ceiling. The broad floor probe also
reported landing before controller contact, enabling braking before touchdown.
Ordinary airborne mastery had no velocity interpolation: the old manual fallback's
RotateTowards/Lerp was bypassed. Hard 18 m/s safety was not the 8–11 m/s cause.

New explicit thumb mapping adds a fixed, profile-owned wish offset of x * 65
degrees to the existing Flow yaw. Forward/backward throttle remains explicit;
pure horizontal input remains yaw-only. This is a fixed input mapping, not an
angle computed from velocity, route or landing targets. It preserves two-thumb
tap/pitch usage. The same mapping applies on the ground and authored surf.

Projected wish acceleration uses 26 m/s²; opposing velocity uses a separate
30 m/s² braking rate. Wish cap stays 8.2, run stays 7.8, soft energy ceiling stays
10.8 and hard safety stays 18. The energy bound is applied after wish acceleration,
preserving heading change at the ceiling with no extra energy. This intentionally
scales the perpendicular component only when limiting energy; below it, wish
acceleration preserves perpendicular velocity. Existing overspeed is not snapped
to 10.8. Weak/high-speed alignment can still be projection-limited: steer more
deliberately or brake. No optimal strafe or yaw-only acceleration is generated.

Ordinary candidate grounding requires controller contact. The sphere probe still
identifies platforms/surf; it does not trigger premature ordinary landing.
Buffered hops continue bypassing braking. Aligned unbuffered landings retain a
55 ms clean-hop grace using the existing perfect-hop setting, followed by normal
ground braking. Landing/takeoff multipliers and jump height/gravity stay intact.

Manual/landing view use identical motor/input assets. No camera force, reticle,
new permanent control, added Campaign geometry, or global camera change.
Development traces use a bounded 9000-sample ring and flush on retry/profile
change/scene exit/pause to MovementTrials under persistentDataPath. They have no
save-service authority. Runtime overlays expose acceleration/contact diagnostics.

During validation, startup overwrote versioned Kenney models from local cache,
including a Windows mapped-file exception. Ensure now imports only missing model
files. Test-generated art changes were restored; authored model preservation is
covered alongside existing startup preservation tests.

Reference reviewed: Valve's AirMove/AirAccelerate/Friction implementation in
https://github.com/ValveSoftware/source-sdk-2013/blob/master/src/game/shared/gamemovement.cpp
uses forward/side wish direction, velocity projection, time-scaled acceleration,
and separate ground friction. No Valve source/assets were copied; this candidate
is an original mobile adaptation, not exact Source physics.
