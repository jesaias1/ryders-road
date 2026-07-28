# Known Issues

- Unity is not installed in the authoring environment. Compilation, Unity Test
  Runner results, and Android APK production remain unverified.
- `Docs/VisualReferences/target_gameplay.png` was not supplied with the attached
  brief and could not be inspected. Art guidance records the written target but
  visual comparison is pending.
- URP renderer/pipeline assets are produced by the checked-in Editor setup on
  first open. They must be saved and committed after opening in the pinned
  Editor.
- No physical Android device testing has occurred. Notch/cutout safe areas,
  pause/resume persistence, orientation, touch, thermal behavior, and sustained
  60 FPS remain physical-device checks.
- The FoundationTest environment uses runtime primitives and legacy UI text.
  This is deliberate temporary Phase 0 presentation.
