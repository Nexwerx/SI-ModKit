# SI ModKit — Export and Import

## Portable project export

Export Project creates a complete text package for the current editable project. The package includes project identity, all windows, all controls, layout markers, window policies, and action bindings.

Copy the complete text field with Ctrl+A and Ctrl+C and store or share it as plain text.

## Portable project import

Import Project accepts one complete SI ModKit portable package. SI ModKit validates the package into temporary state before replacing the active in-memory project. Invalid or truncated text does not destroy the current project.

Imported projects are marked dirty. Internal saved-project data is not replaced until the imported project is explicitly saved.

## Why copy/paste is used

Direct arbitrary file writing is blocked by the Software Inc. source sandbox. Portable text is therefore the supported backup/share mechanism and does not depend on filesystem access.

See `PROJECT-FORMAT.md` for the versioned interchange contract.
