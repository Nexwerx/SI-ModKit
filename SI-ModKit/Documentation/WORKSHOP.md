# Publishing or installing the source mod

## Distributing SI: ModKit

The installable folder is [`../Source/SI-ModKit`](../Source/SI-ModKit). Keep its `Main.cs`, `MODBehaviors.cs`, `meta.tyd`, subdirectories, and `Thumbnail.png` together when packaging the source mod. Do not place the repository's `Documentation`, `Examples`, or `DevKit` folders inside the game mod unless they are intentionally used by your own separate mod.

## Publishing a mod built with the DevKit

Copy `DevKit/UI.cs` and `DevKit/UIWindow.cs` to your own source mod, alongside your generated C# and mod-specific entry points/metadata. The copied facade is compiled with your mod; consumers need not install SI: ModKit as a runtime dependency for this path.

## GitHub release assets

The repository itself is for browsable source and docs. When creating a release, attach a versioned ZIP of the entire public repository, or a separate **install-only** ZIP containing just the `SI-ModKit` mod folder. Label each ZIP so users can distinguish repository/developer files from the game-installable package. GitHub's automatic source archive is not the same thing as the install-only package.
