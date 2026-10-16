# SI ModKit — Controls Reference

## Label / wrapped text

Static wrapped text. Height expands for longer content.

## Text box

Single-line input with optional caption, placeholder, initial value, and edit-complete action.

## Numeric input

Numeric text input with minimum, maximum, initial value, and edit-complete action. Invalid/out-of-range values are normalized to the configured range.

Builder range format:

```text
0,100
```

## Multiline text area

Wrapped multi-line input for notes, descriptions, and larger user-entered text.

## Checkbox

Boolean control with optional initial state and change action.

## Button

Runs a named action when clicked.

## Confirmation button

Opens a native confirmation dialog and runs the action only when confirmed.

## Dropdown

String option list with initial selection and selection-change action.

## Slider

Interactive numeric range control with live change action.

## Progress bar

Display-only ranged progress indicator.

## List

Native one-column `GUIListView`. Builder items are comma-separated. Selection can fire an action.

## Table

Native multi-column string `GUIListView`. Builder columns are comma-separated. Builder rows use one line per row and `|` between cells:

```text
Alpha|10|Active
Beta|20|Waiting
```

## Builder palette

All current controls are visible without palette paging:

```text
Txt  I_  [ ]  Btn  v   <->
#    ¶   List Tbl  ?   %
```

Rows and sections are shown on the layout palette row.
