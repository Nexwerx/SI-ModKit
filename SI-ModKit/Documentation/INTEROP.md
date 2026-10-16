# SI ModKit — External Integration and Interoperability

## Proven compiler behavior

An independently compiled source mod cannot resolve `SIModKit` types merely because SI ModKit is installed. A clean direct-reference test failed because `SIModKit` was not present in the consumer mod's compiler context.

## Recommended generated-project path

For normal builder-generated UI, use the copy-ready source kit in `DevKit`. The target mod compiles `UI.cs`, `UIWindow.cs`, and its generated project source together in its own compilation context. This provides the same wrapper API without a compile-time dependency on the installed SI ModKit.

## Proven runtime bridge

Two independently compiled mods successfully completed a two-way runtime message round trip through the Unity object:

```text
SIModKit.Bridge
```

The verified transport requires no reflection and no direct filesystem access. The bridge is appropriate when two installed mods intentionally need to exchange runtime messages.

The bridge transport is separate from the standalone source-kit model. A generated standalone project does not need the installed SI ModKit if it carries the source kit in its own source tree.

## Discovery pattern

```csharp
GameObject bridge = GameObject.Find("SIModKit.Bridge");
if (bridge == null)
{
    // SI ModKit is not installed/active. Degrade gracefully.
    return;
}
```

Consumers must treat bridge absence as a normal optional-dependency condition. Do not assume load order or throw an unrecoverable error merely because the bridge is unavailable.

## Why normal type references are not documented

The Software Inc. compiler reference list does not expose previously compiled mod assemblies as normal references to another source mod. External documentation must therefore not instruct consumers to add a normal `using SIModKit;` dependency on the separately installed mod.
