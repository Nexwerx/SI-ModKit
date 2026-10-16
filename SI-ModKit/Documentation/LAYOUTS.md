# SI ModKit Layouts

## Flow model

SI ModKit uses a vertical flow by default. Controls added at the window root or inside a section occupy the available width between the configured content margins. Full-width toolkit controls are horizontally anchored so they stretch with a resizable native window.

## Horizontal rows and grids

`BeginRow(columns)`, `BeginHorizontal(columns)`, and `BeginGrid(columns)` create the same responsive equal-column container. The column count is clamped to 1–8. Each cell uses fractional horizontal anchors, while the outer content margin and inter-cell spacing remain fixed pixel offsets.

```csharp
window.BeginGrid(3);
window.AddButton("A", OnA);
window.AddButton("B", OnB);
window.AddButton("C", OnC);
window.EndRow();
```

In the visual builder, a Row marker defaults to two columns. The marker's caption may contain another column count, such as `3 columns` or `4 columns`; preview and generated source use the parsed value.

## Sections

Sections are vertical groups with a native heading and indentation. Rows/grids may be placed inside sections. Sections and rows must be explicitly closed in the document model so layout validation can identify malformed nesting.

## Resize behavior

Horizontal resize changes the width of full-width controls and responsive row/grid cells. Vertical positions and control heights remain flow-based. Controls with intrinsic taller heights, such as multiline fields, lists, and tables, retain those heights while their width changes.

## Builder productivity

The builder includes document-level undo/redo history, named internal project persistence, and starter project templates. Portable Export/Import remains the independent backup and sharing mechanism.
