# ADR 0014 — Scene transitions survive their callers

Accepted 2026-09-06 from physical 0.9.1 S23 frontend failure.

Real Campaign and Spiral button reproduction showed ModuleRunner active, the
correct module and player initialized, but the persistent loading canvas alpha
and raycast interception still at 1/true. Scene activation destroys the menu or
module MonoBehaviour that owns its nested load coroutine. Completion after the
async operation therefore never executes. A test that drives the iterator from
a surviving test runner does not cover this lifecycle.

ILevelLoader keeps its public iterator contract. SceneTransitionHost now owns
requests and Unity operations on a DontDestroyOnLoad object; callers only observe.
Automatic scene activation stays enabled. ModuleSceneController reports readiness
after world, HUD and player construction; DevelopmentModuleSelector reports after
UI creation. Requested scene/module identity must match. Duplicate requests cannot
start competing operations. A profile supplies a 20-second activation/readiness
timeout. Exceptions in presentation subscribers cannot stop the scene operation.

LoadingPresentation only observes events and terminal request state. No video
completion, prepared callback or first frame gates scene readiness. Media failure
uses the poster and logs development diagnostics. Scene failure presents a visible
recovery action; Unity operations cannot be cancelled, so an unfinished timed-out
operation cannot be replaced by a competing load and may require app restart.

Full-screen artwork/video uses COVER (EnvelopeParent), outside safe-area parents.
Interactive foreground UI stays safe-area constrained. Source aspect is preserved
by cropping rather than pillarboxing. Android immersive startup/focus/resume and
both landscape policies remain in force. No movement, camera-control, content-ID,
rank/progression or persistence meaning changes.
