# SI ModKit — Projects and Windows

## Projects

A project is the editable top-level document. It owns a stable project ID, project name, window order, active-window selection, and ID counters used to keep windows and controls stable across persistence/export.

Saved Projects stores multiple named projects through Software Inc.'s supported settings API. New / Templates starts a new in-memory project from Blank, Settings, or Toolbar scaffolding.

## Windows

Each project window independently stores:

- stable window ID;
- title;
- width and height;
- content margin;
- Standard or Toolbar presentation;
- Movable, Resizable, and Closable policies;
- ordered control/layout records.

Switching windows captures the active window document before another window is loaded into the builder.

## Templates

Templates are ordinary editable projects after creation:

- **Blank** — one empty standard window;
- **Settings** — starter preferences form;
- **Toolbar** — compact starter window with toolbar presentation.

They do not create a different project type or lock the design into the initial structure.

## Persistence and backups

Internal saved projects are convenient working storage. Portable Export remains the independent backup/share path and should be used for important designs.
