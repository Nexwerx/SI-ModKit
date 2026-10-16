# Standalone ModKit source kit

The two files here are exact copies of the UI facade sources in `Source/SI-ModKit/UI` from this release:

- `UI.cs`
- `UIWindow.cs`

Copy both `.cs` files into **your own Software Inc. source mod**, keep their `SIModKit` namespace, and compile them together with your generated/handwritten UI. No compile-time reference to a separately installed SI: ModKit is needed.

**Do not use** `using SIModKit;` alone in a separate source mod without copying these files; the game's compiler cannot resolve an unrelated mod's source assembly just because it is installed.

Create UI after `WindowManager` is initialized. Provide mod-specific lifecycle code to clean up on deactivation. See [integration](../Documentation/INTEGRATION.md) and [API](../Documentation/PUBLIC-API.md).

When upgrading your generated UI code, update the **pair** of facade files together to match the release. Third-party integration still requires testing in the target Software Inc. game version.
