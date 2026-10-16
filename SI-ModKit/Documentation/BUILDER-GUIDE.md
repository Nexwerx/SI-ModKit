# SI ModKit — Builder Guide

## Main hierarchy

The builder hierarchy is the main design surface. All supported controls are visible in the palette, followed by row/section layout controls. The hierarchy shows up to eight entries at once and uses color cues for layout markers.

Click a hierarchy entry to open its Properties view. Control-specific actions such as Move, Duplicate, Remove, and Apply belong to that selected-control view.

## Undo and redo

Undo and Redo are available from the hierarchy navigation row. SI ModKit keeps a bounded project history for normal editing mistakes.

## Preview

Preview / Refresh updates the live preview from the current project document. SI ModKit reuses the preview frame instead of intentionally destroying and respawning it, which helps preserve player placement.

## Projects and windows

A project contains multiple independent windows. Project / Windows allows switching, creating, duplicating, deleting, saving, exporting, importing, and generating whole-project source.

Saved Projects stores multiple named internal projects. New / Templates can create Blank, Settings, or Toolbar starters. Templates are ordinary projects after creation.

## Properties

Properties vary by control type. Typical properties include caption, placeholder, initial value, range/options, and action name. Apply Properties updates the project document, preview, persistence output, and generated source model.

## Destructive actions

Clear All, window deletion, project replacement/import, and similar operations require confirmation where data loss is possible.
