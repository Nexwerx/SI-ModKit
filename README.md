# SI: ModKit

**In-game UI builder and native UI source toolkit for Software Inc.**

SI: ModKit lets Software Inc. mod creators design game-style windows in an in-game builder, preview their work, save editable projects, and generate C# source for their own source mods. Developers can also use the included `SIModKit.UI` and `SIModKit.UIWindow` facade directly.

**Version:** 1.0.0 (as identified by the provided mod metadata)  
**Author:** Nexwerx  
**Platform:** Software Inc. source-mod environment (game version compatibility should be verified on your installation)

## What you can do

- Design multiple windows within a named project using controls and row/section layouts.
- Configure titles, size, resize/move/close behavior, toolbar presentation, and margins.
- Add labels, headings, text/numeric input, text areas, checkboxes, buttons, confirmation buttons, dropdowns, sliders, progress bars, lists, and tables.
- Preview or refresh windows, use templates, and undo or redo changes.
- Save projects within the game, copy/paste portable project data, and generate C# for a window or entire project.
- Bundle the provided source facade with another mod; no compile-time link to the installed ModKit mod is required for this standalone path.

## Install and open the builder

1. Download the release package or this repository's files.
2. Locate `Source/SI-ModKit/`. Its contents (`Main.cs`, `MODBehaviors.cs`, `meta.tyd`, `UI/`, `Builder/`, `Interop/`, and `Diagnostics/`, plus the thumbnail) are the *mod package*.
3. Place that folder in your Software Inc. installation's `DLLMods` directory, so the result is `DLLMods/SI-ModKit/Main.cs` (not `DLLMods/SI-ModKit/Source/...`).
4. Start Software Inc., enable the mod if necessary, enter a game, and use the **ModKit** launcher button.
5. If the mod fails to load, consult [Troubleshooting](Documentation/TROUBLESHOOTING.md) and confirm the game's current mod-installation conventions.

No Visual Studio solution or separately built DLL is supplied or required for this source-mod package. SI: ModKit is also available through Steam Workshop if you prefer Workshop installation.

## Your first window

1. Open the builder and create or select a project.
2. Add a window, then add a heading, textbox, and button from the palette.
3. Select elements in the hierarchy to edit properties and actions.
4. Click **Preview / Refresh** to inspect the window. If initial native layout is off, click it again.
5. **Save** the editable in-game project and **Export Project** to keep an external text backup.
6. Generate source for a window or the entire project. Copy the generated code into your own source mod together with the [DevKit](DevKit/README.md) source files.

## Choose an integration method

**Standalone source kit (recommended):** Copy `DevKit/UI.cs` and `DevKit/UIWindow.cs` into your own source mod and compile alongside generated or handwritten UI code. This does **not** require ModKit to be installed for your mod to use the copied facade.

**Runtime interop bridge (advanced):** The installed ModKit exposes the Unity object `SIModKit.Bridge` for a limited message-based integration. This is not a general compiled API and is not a substitute for the source kit. Read [Interop](Documentation/INTEROP.md) before attempting it.

## Repository guide

| Folder | Purpose |
| --- | --- |
| [`Source/SI-ModKit`](Source/SI-ModKit) | Complete source mod to install / inspect |
| [`DevKit`](DevKit) | Copyable UI facade source for third-party source mods |
| [`Examples`](Examples) | Source examples and integration patterns |
| [`Documentation`](Documentation) | User and developer guides, API, compatibility, limitations |
| [`Documentation/Development-History`](Documentation/Development-History) | Historical internal milestone/test notes; **not** current user instructions |
| [`Assets/Screenshots`](Assets/Screenshots) | Screenshots from provided archive |

Start with [Getting Started](Documentation/GETTING-STARTED.md), [Builder Guide](Documentation/BUILDER-GUIDE.md), [Public API](Documentation/PUBLIC-API.md), and [Integrating with your mod](Documentation/INTEGRATION.md).

## Known behavior

The first preview of some native controls can display unexpected geometry; **Preview / Refresh** is the supported workaround. The game compiler sandbox limits direct file access/reflection. A separately compiled source mod cannot simply reference the installed `SIModKit` namespace. See [Known Limitations](Documentation/KNOWN-LIMITATIONS.md).

## License

Copyright (c) 2026 Nexwerx Systems. All rights reserved.

This software is free for public use, can be transmitted, rebroadcast, and used in dirivative works.
Please give credit where credit is due. I owe much of mine to hrkrx, whom I believe put out one of the first DLLMODS
back in the day. I don't reserve any rights over the knowledge, I just put it out there because I thought someone might
be able to use it, and if they did, and it helped, we are all winners.

Software Inc., Unity and other third-party materials remain owned by their respective
rights holders; no rights in third-party software or assets are granted here.**A license decision is required before publication.** The provided archive did not contain a usable license or confirmed license instructions. See [`LICENSE`](LICENSE); it is a placeholder, not an open-source grant. Choose and replace it with your intended license before publishing if you want others to reuse/modify/distribute the code. Third-party game and Unity assets remain subject to their own terms.

## Releases

The repository contains browsable source and docs. Use GitHub **Releases** for versioned ZIP downloads. The source of truth for changes in this package is [CHANGELOG.md](CHANGELOG.md).
