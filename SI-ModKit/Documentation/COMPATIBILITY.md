# SI ModKit — Compatibility

SI ModKit is built around Software Inc.'s current native UI primitives and source-mod compiler behavior established during development.

## Stable assumptions

- `WindowManager` supplies the native window/control factories used by the facade.
- `ModBehaviour.SaveSetting` / `LoadSetting` supplies supported project persistence.
- independently compiled source mods do not receive each other's types as compile references;
- shared Unity runtime objects can support cross-mod messaging;
- source-kit consumers compile the wrapper source with their own mod.

## Game updates

A Software Inc. update can change native UI classes, object paths, compiler restrictions, or control behavior. SI ModKit centralizes those dependencies so facade/source-kit consumers should prefer documented wrapper methods rather than direct game UI internals.

If a game update breaks the wrapper, update the source kit together with the main toolkit so generated projects and documentation remain aligned.
