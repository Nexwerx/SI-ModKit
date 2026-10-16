# SI ModKit — Public UI API Reference

This reference documents the strongly typed facade used by the builder and by standalone source-kit consumers.

## Create and destroy

```csharp
SIModKit.UIWindow window = SIModKit.UI.CreateWindow("Settings", 600, 420);
SIModKit.UI.DestroyAll();
```

`CreateWindow` requires Software Inc.'s `WindowManager` to be initialized.

## Window behavior

```csharp
window.SetTitle("Settings");
window.SetSize(600, 420);
window.SetContentMargin(14);
window.SetToolbarMode(false);
window.SetMovable(true);
window.SetResizable(false);
window.SetClosable(true);
```

Presentation and behavior are independent.

## Layout

```csharp
window.BeginSection("General");
window.BeginRow(2);
window.AddButton("Cancel", Cancel);
window.AddButton("Save", Save);
window.EndRow();
window.EndSection();
```

Aliases:

```csharp
window.BeginHorizontal(2);
window.EndHorizontal();
window.BeginGrid(3);
```

Rows support 1–8 responsive equal-width columns.

## Text controls

```csharp
UnityEngine.UI.Text label = window.AddLabel("Status");
UnityEngine.UI.Text heading = window.AddHeading("Settings");

UnityEngine.UI.InputField text = window.AddTextBox(
    "Enter name",
    value => OnNameEdited(value));

UnityEngine.UI.InputField number = window.AddNumberBox(
    "Amount", 0f, 1000f, 50f,
    value => OnAmountEdited(value));

UnityEngine.UI.InputField notes = window.AddTextArea(
    "Notes", "Initial text", 96f,
    value => OnNotesEdited(value));
```

## Selection controls

```csharp
UnityEngine.UI.Toggle enabled = window.AddCheckbox(
    "Enabled", true,
    value => OnEnabledChanged(value));

GUICombobox priority = window.AddDropdown(
    new[] { "Low", "Normal", "High" }, 1,
    (index, value) => OnPriorityChanged(value));

UnityEngine.UI.Slider volume = window.AddSlider(
    0f, 100f, 50f,
    value => OnVolumeChanged(value));
```

## Lists and tables

```csharp
GUIListView list = window.AddList(
    new[] { "Alpha", "Beta", "Gamma" },
    0, 144f,
    (index, value) => OnItemSelected(index, value));

GUIListView table = window.AddTable(
    new[] { "Name", "Value" },
    new[]
    {
        new[] { "Alpha", "10" },
        new[] { "Beta", "20" }
    },
    180f,
    (index, row) => OnRowSelected(index, row));
```

## Buttons and dialogs

```csharp
window.AddButton("Save", Save);

window.AddConfirmButton(
    "Delete",
    "Delete this item?",
    "Delete",
    DeleteItem);

SIModKit.UI.ShowMessage("Saved.");
SIModKit.UI.ShowConfirmation(
    "Continue?",
    ContinueWork,
    CancelWork,
    "Continue",
    "Cancel");
```

## Display and large text

```csharp
GUIProgressBar progress = window.AddProgressBar(0f, 100f, 35f);

UnityEngine.UI.InputField largeText = window.AddStretchTextArea(
    "Paste text here", 120f, 16f);
```

## Lifecycle

```csharp
window.Show();
window.Hide();
window.ClearContent();
window.Destroy();
```

`ClearContent()` removes facade-owned controls while preserving the native window frame.

## Native escape hatch

`UIWindow.NativeWindow` exposes the native `GUIWindow` for advanced code. Normal usage should prefer the facade because native game internals can change independently of this contract.
