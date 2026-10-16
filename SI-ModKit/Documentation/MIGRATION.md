# SI ModKit — Migration Notes

## Older saved projects

The project/window serializers are versioned. Current builds retain compatibility readers for earlier window document formats used during development. Loading is transactional where replacement could discard active work.

## From the original single saved project

The multi-project manager merges the older saved-project record into the named project list when it is still present.

## From older generated source

Generated source may change as the facade evolves. Preserve the portable editable project whenever possible and regenerate from the current SI ModKit version rather than treating old generated output as the canonical design.

## External integration

Do not migrate an external mod by adding a normal compile-time reference to the installed SI ModKit. Use either the standalone source kit or the proven runtime bridge model depending on the use case.
