# Integrate ModKit-generated UI into your source mod

## Recommended: standalone source kit

Each Software Inc. source mod is compiled in its own context. Merely installing SI: ModKit does **not** make `SIModKit.UI` compile-time types available to your separate mod.

1. Copy [`../DevKit/UI.cs`](../DevKit/UI.cs) and [`../DevKit/UIWindow.cs`](../DevKit/UIWindow.cs) into your mod source directory (or an included source subdirectory). Keep their `SIModKit` namespace intact.
2. Generate C# in the ModKit builder and copy the output into your mod. For multi-window projects, split the output at its `// FILE:` markers as appropriate.
3. Adapt generated entry points and action handlers to your mod logic; generated diagnostics are not your finished behavior.
4. Create windows only after Software Inc.'s `WindowManager` is ready (normally while in the game scene).
5. Hide/reuse windows as appropriate and clean up facade windows using `SIModKit.UI.DestroyAll()` at your mod's shutdown.
6. Compile and test your mod with Software Inc.'s in-game source compiler.

For a simple handwritten example, see [`../Examples/StandaloneWindow.cs.txt`](../Examples/StandaloneWindow.cs.txt). This example is intentionally a `.cs.txt` snippet so it is not accidentally compiled when someone copies the examples folder wholesale.

## Folder example

```text
My-Mod/
  Main.cs
  MODBehaviors.cs
  meta.tyd
  UI.cs
  UIWindow.cs
  ...your generated or handwritten windows...
```

Use your game's supported mod metadata and behavior entry points; these are illustrative filenames, not a complete distributable sample mod.

## Advanced: optional installed-ModKit bridge

For runtime communication with the separately installed toolkit, the included bridge uses `GameObject.Find("SIModKit.Bridge")` and `SendMessage`. This interface currently supports diagnostic methods `Ping`, `CreateProbeWindow`, and `AcknowledgeProbe`; it does **not** expose the entire typed UI facade. Read [Interop](INTEROP.md).

## Persistence and file access

Portable project import/export is copy/paste text. The game's source-mod sandbox restricts arbitrary `System.IO.File`, reflection, and some Unity APIs. Preserve an exported project to keep a design editable; generated C# is not an editable design project.

## Deployment

For local installation, copy the contents of `Source/SI-ModKit` as one mod folder beneath your Software Inc. installation's `DLLMods` folder. There is no requirement to use external IDE build products or any particular drive-letter path. Verify current game mod installation conventions for your version.
