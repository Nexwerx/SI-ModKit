# SI ModKit Portable Project Format

**Format:** `SIMODKIT-PORTABLE-PROJECT-1`  
**Authority:** `docs/SPEC.md` and `docs/ROADMAP.md`

## Purpose

The portable project format is the sandbox-compatible interchange representation for a complete editable SI ModKit project. It is separate from Software Inc.'s internal mod-setting persistence and from generated C# source.

A portable package contains the project identity, active-window state, ID counters, every window document, every control record, layout markers, action bindings, and window behavior/presentation settings required to reconstruct the project.

Generated C# is not the canonical editable format and is not used for project import.

## Envelope

A version-1 package is plain text with these logical records:

```text
SIMODKIT-PORTABLE-PROJECT-1
MANIFEST:<serialized project manifest>
WINDOWS:<window count>
WINDOW:<serialized window record 1>
WINDOW:<serialized window record 2>
...
END-SIMODKIT-PROJECT
```

The existing versioned manifest and window serializers remain authoritative inside the envelope. User-entered strings in those records are encoded by SI ModKit's text encoder, so embedded punctuation and line breaks do not alter envelope boundaries.

## Validation contract

Import is transactional. SI ModKit parses and validates the complete package before changing the active in-memory project.

Validation includes:

- recognized package signature;
- supported manifest record;
- valid positive window count;
- exact agreement between envelope count and manifest window IDs;
- exact package termination with no unexpected trailing records;
- valid versioned window records;
- window order and identity matching the manifest;
- unique window IDs;
- non-empty, unique control IDs across the imported project.

A validation failure leaves the current project untouched.

A successfully imported project is marked dirty and is not written to the internal saved-project slot until the user explicitly saves it.

## Versioning

The package signature owns the outer interchange schema version. The manifest and each window record retain their own existing version signatures so project interchange can migrate independently from individual document records.

Future portable schemas must use a new package signature rather than silently changing the meaning of `SIMODKIT-PORTABLE-PROJECT-1`.

## Complete-project generated source

Milestone 0.22 also adds complete-project generated C# output. It is deliberately separate from the portable project package.

Whole-project source contains every project window and uses explicit logical file markers:

```text
// ============================================================
// FILE: 01_Main_Window.cs
// WINDOW ID: window-1
// ============================================================
```

Generated control and action variable names are window-prefixed in whole-project output so references from separate windows do not collide when copied into a common integration context.
