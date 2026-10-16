# SI ModKit — Actions and Events

## Named actions

Interactive builder controls can bind to a named action such as:

```text
SaveSettings
ApplyFilter
DeleteItem
```

Multiple controls may intentionally use the same action name.

Generated source creates reusable `System.Action` delegates or control-specific callbacks and provides references to live controls so the action can read current user-entered values.

## Supported event sources

- button click;
- confirmed confirmation-button click;
- text box edit complete;
- multiline text edit complete;
- numeric edit complete;
- checkbox state change;
- dropdown selection change;
- slider value change;
- list selection change;
- table selection change.

## Safety model

Saved project documents store action identifiers and control settings, not arbitrary executable C# source. Generated diagnostic action bodies are intended to be replaced by the mod author in the copied source.

## Example

```csharp
System.Action saveSettings = () =>
{
    string name = nameInput == null ? "" : nameInput.text;
    bool enabled = enabledToggle != null && enabledToggle.isOn;
    SaveMyModSettings(name, enabled);
};

window.AddButton("Save", saveSettings);
```
