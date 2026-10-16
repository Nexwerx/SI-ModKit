# SI ModKit — Examples

## Settings window

```csharp
SIModKit.UIWindow window = SIModKit.UI.CreateWindow("Settings", 600, 420);
window.SetResizable(false);
window.SetContentMargin(14);

window.BeginSection("General");
UnityEngine.UI.Toggle enabled = window.AddCheckbox("Enabled", true);
GUICombobox mode = window.AddDropdown(new[] { "Basic", "Advanced" }, 0);
window.EndSection();

window.BeginRow(2);
window.AddButton("Cancel", () => window.Hide());
window.AddButton("Save", () => SaveSettings(enabled.isOn, mode.SelectedItemString));
window.EndRow();
window.Show();
```

## Compact toolbar

```csharp
SIModKit.UIWindow toolbar = SIModKit.UI.CreateWindow("Quick Tools", 320, 130);
toolbar.SetToolbarMode(true);
toolbar.SetMovable(true);
toolbar.SetResizable(false);
toolbar.SetClosable(false);
toolbar.SetContentMargin(8);

toolbar.BeginRow(3);
toolbar.AddButton("A", RunA);
toolbar.AddButton("B", RunB);
toolbar.AddButton("C", RunC);
toolbar.EndRow();
toolbar.Show();
```

## List selection

```csharp
window.AddList(
    new[] { "Alpha", "Beta", "Gamma" },
    0,
    144f,
    (index, value) =>
    {
        HandleSelection(index, value);
    });
```
