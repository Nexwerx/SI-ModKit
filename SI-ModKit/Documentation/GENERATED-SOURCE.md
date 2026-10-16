# SI ModKit — Generated Source

Generated source can target the active window or the entire project. Whole-project output uses clear logical file boundaries and stable window/control identifiers.

## Recommended standalone integration

Software Inc. compiles source mods independently, so another mod cannot compile against the separately installed SI ModKit namespace. To use builder-generated source in a standalone mod:

1. Copy `DevKit/UI.cs` into the target mod and rename it `UI.cs`.
2. Copy `DevKit/UIWindow.cs` into the target mod and rename it `UIWindow.cs`.
3. Keep the `SIModKit` namespace in those files.
4. Copy the generated window/project source from SI ModKit into the target mod.
5. Call the generated window-construction code at a point where Software Inc.'s `WindowManager` is available.
6. Replace generated diagnostic action bodies with the mod's real logic.

The copied source kit is self-contained with respect to SI ModKit: it is compiled as part of the target mod rather than relying on another mod's compile-time types.

## Example

```csharp
private SIModKit.UIWindow settingsWindow;

private void OpenSettings()
{
    if (settingsWindow != null && settingsWindow.NativeWindow != null)
    {
        settingsWindow.Show();
        return;
    }

    settingsWindow = SIModKit.UI.CreateWindow("Settings", 600, 420);
    settingsWindow.SetMovable(true);
    settingsWindow.SetResizable(false);
    settingsWindow.SetClosable(true);

    UnityEngine.UI.InputField nameInput =
        settingsWindow.AddTextBox("Company name");

    settingsWindow.AddButton("Apply", () =>
    {
        ApplySettings(nameInput == null ? "" : nameInput.text);
    });

    settingsWindow.Show();
}
```

## Lifetime

A mod should keep references to windows it expects to reopen and destroy/clean them during its own deactivation lifecycle. `SIModKit.UI.DestroyAll()` destroys all windows created through that copied facade in the target compilation context.

## Generated source is not project storage

Use portable project export to preserve an editable design. Generated source is meant for integration, review, and modification by the mod author.
