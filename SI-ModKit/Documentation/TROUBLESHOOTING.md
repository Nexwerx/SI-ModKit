# SI ModKit — Troubleshooting

## Window opens at an unexpected native size

Software Inc. native windows can perform first-registration layout work. SI ModKit preserves requested document sizing and reuses preview frames, but some native controls can still visually settle after the first refresh.

## A row or section warns about layout

Rows and sections require matching end markers. The builder's Current layout line indicates which closing marker is required.

## Generated code does not compile in another mod

A separate source mod cannot compile against the installed SI ModKit namespace directly. Copy the two files from `DevKit` into the target mod and compile the generated source alongside them.

## Project cannot be restored from generated source

Generated source is not the editable project format. Use Export Project / Import Project for design backups and transfers.

## Direct file export is unavailable

The source sandbox rejects direct `System.IO.File` use. This is expected. Use portable copy/paste export.

## SI ModKit bridge is not found

Treat `GameObject.Find("SIModKit.Bridge") == null` as an optional-dependency condition. The installed SI ModKit may be absent or inactive. Standalone source-kit projects do not need the bridge.
