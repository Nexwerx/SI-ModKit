# Known limitations — 1.0.0

## First preview layout

On some control/window combinations, the first preview may use unexpected native Software Inc. layout geometry, especially lists, tables, and multiline text. Click **Preview / Refresh** once after opening it; this is the documented workaround. Automatic repeated refresh attempts were not retained for the 1.0 release.

## Source compiler and cross-mod references

- The Software Inc. source-mod sandbox restricts reflection, direct arbitrary file access, and some Unity APIs.
- Independently compiled source mods cannot directly reference types from an installed SI: ModKit mod. Copy and compile the standalone [DevKit](../DevKit) sources for generated projects.
- The runtime message bridge is a limited optional interop mechanism, not the whole typed UI API.

## Editing and code generation

- The builder uses row/section flow layout, not a freeform drag-and-drop canvas.
- The hierarchy is a paginated selection list, not a full collapsible tree.
- Generated C# is not round-trippable into an editable project. Export the portable project text to preserve its editable design.

## Compatibility

A Software Inc. game update can alter native UI controls or compiler behavior. Retest your generated code and copied DevKit together after major updates.
