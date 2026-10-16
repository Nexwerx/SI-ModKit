# SI ModKit — Source Sandbox Restrictions

The following restrictions were verified while developing SI ModKit:

- `System.Reflection` references are rejected by the source security checker.
- `UnityEngine.Application` references are rejected.
- `System.IO.File` references are rejected.
- one independently compiled source mod cannot directly resolve another mod's `SIModKit` types at compile time.

SI ModKit does not attempt to bypass these restrictions.

## Supported alternatives

- Project persistence: `ModBehaviour.SaveSetting` / `LoadSetting` through Software Inc.'s trusted persistence path.
- Portable backup/share: copy/paste project text.
- Standalone generated UI: copy the supplied source kit into the target mod.
- Cross-mod runtime communication: `SIModKit.Bridge` through shared Unity runtime messaging.
