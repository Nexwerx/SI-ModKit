# SI ModKit — Getting Started

SI ModKit is an in-game UI designer and source generator for Software Inc. source mods. It is designed so a mod author can build native-looking windows without learning Unity UI internals.

## Basic workflow

1. Open SI ModKit in-game.
2. Create or open a project.
3. Add one or more windows.
4. Add controls from the palette.
5. Click a hierarchy item to edit its properties.
6. Use Preview / Refresh to test the window.
7. Save the project for in-game persistence.
8. Export the portable project text for backup or sharing.
9. Generate source for one window or the entire project.
10. For a standalone mod, copy the source kit into the mod and compile the generated source alongside it.

## Projects vs generated source

The SI ModKit project is the editable design. Generated source is output derived from that design. Keep a portable project export if the design matters; generated source is not used to reconstruct the visual project.

## Integration choices

### Standalone source kit — recommended for generated projects

Copy these supplied files into the target source mod and use them as `.cs` source files:

```text
DevKit/UI.cs
DevKit/UIWindow.cs
```

Keep their `SIModKit` namespace. The generated project source can then use the same `SIModKit.UI` / `UIWindow` facade inside that mod's own compilation context.

### Runtime bridge — for cross-mod communication

Software Inc. does not expose one independently compiled mod's types to another mod's compiler. SI ModKit therefore exposes a sandbox-safe runtime bridge object named `SIModKit.Bridge` for cases where two installed mods intentionally need to communicate at runtime. See `INTEROP.md`.

## Important sandbox facts

The source sandbox has rejected direct use of `System.Reflection`, `UnityEngine.Application`, and `System.IO.File`. SI ModKit does not bypass those restrictions. Project persistence uses Software Inc.'s supported mod-setting API, and portable project/source transfer uses copy/paste text.
