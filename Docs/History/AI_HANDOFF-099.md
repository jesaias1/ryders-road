# Current authority — 0.9.9 Campaign Flow trial

Private GitHub sync is active at `origin`
(`https://github.com/jesaias1/ryders-road.git`). The remote repository is
private and tracks `main`. The initial push required an approved Git LFS history
migration because the inherited history contained a 108 MB source archive that
GitHub rejects as a normal blob. The pre-migration history is preserved locally
as `GitBackups/ryders-road-pre-lfs-migration.bundle`; do not commit or upload
that backup. `.gitignore` covers local Codex caches, temporary QA result
XML/log files, loose S23 capture PNGs and `GitBackups/`. `.gitattributes` tracks
large source asset formats (`.zip`, `.fbx`, `.glb`, `.mp4`) through Git LFS.
The ChatGPT/Codex GitHub connector may still require GitHub App access to this
new private repository before connector-based review can read it.

Updated 2026-09-08. Current user feedback supersedes old manual-camera assumptions:
the user physically tested 0.9.8 on S23, accepts Foundry's easier normal route,
reports the overhead sightline obstruction and stranded lower-scenery landings,
and prefers the Flow Lab candidate over fully manual controls. This is not yet
approval to replace Campaign movement globally. Continue authorized independent
production; do not repeatedly ask permission for ordinary changes.

0.9.9 addresses the crane through composition and non-route scenery landings
through explicit counted-fall Restore. Route spacing, shortcuts and both motor
assets remain unchanged. Foundry content v2, stable IDs and V4 preserved.

Home → CAMPAIGN FLOW TRIAL provides real roads 001–004 in three modes:
A accepted motor/saved controls; B unchanged Flow candidate/manual pitch;
C the same Flow candidate with 16-degree default and bounded landing framing.
Right vertical drag remains available. No new permanent run buttons. Trial
attempts/completions/rewards never write to saves or persist the control override.
Times are session-only and separate road/content/mode. Retry retains the mode;
home/normal selection clears it. Campaign itself still uses accepted behavior.

Verification: 240/240 EditMode, 57/57 PlayMode and source validation pass. Actual
renders reviewed. Both normal/candidate Flow standard Foundry jumps, scenery
falls, side-contact rejection, view cone, ferry/carry, recovery, all 12 trial
road/mode loads, real touch and save/preference isolation are covered. No new
physical test was performed by the agent; adb checked once, no device attached.
The current fixes, landing view and replacement decision need physical feedback.
Full evidence, APK metadata and short S23 checklist: PRODUCTION_099.md.

Verified ARM64 APK: Builds/Android/RYDERS-ROAD-0.9.9-campaign-flow-trial-dev.apk.
Build succeeded with 0 errors and one existing legacy-icon warning; package/hash
record: Logs/production099-package.json. It has not been installed on a phone.

Recoverable accepted baseline: 38d222a (0.9.8), earlier db7f01a (0.9.7). The
commit containing this handoff is the 0.9.9 checkpoint. No intentional uncommitted
source/content changes remain at delivery. Ignored logs/builds/caches and older
unrelated untracked captures are retained.

Next: get B versus C comparison on real roads and use that to choose the game's
control foundation. Do not assume fully manual must win and do not endlessly
tune numbers without this test. Independent work remains: event audio/listening,
then a complete fifth world with normal flowing routes and optional skill lines,
using ART_DIRECTION.md's shared roughness/palette/detail/motif rules. Do not raise
normal difficulty solely to make later worlds harder. Worlds 005–008 are unbuilt.
No PC/multiplayer/economy diversion. Nothing is labeled Gold or production-frozen.

Authority: PRODUCTION_099.md, ADR 0021, current ART_DIRECTION.md.
Historical handoffs: History/AI_HANDOFF-098.md, -097.md, -096.md.
