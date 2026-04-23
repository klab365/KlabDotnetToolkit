---
name: update-changelog
description: Update the project CHANGELOG.md with recent changes, following Keep a Changelog format
license: MIT
compatibility: opencode
---

## What I do

- Read the existing `CHANGELOG.md` (or create one if missing)
- Inspect commits since the last git tag using `git log $(git describe --tags --abbrev=0 2>/dev/null || echo origin/main)..HEAD` and diffs to understand what changed
- Add a new entry under `[Unreleased]` or a versioned section using [Keep a Changelog](https://keepachangelog.com) format
- Group changes under: `Added`, `Changed`, `Fixed`, `Removed`, `Deprecated`, `Security`
- Use clear, human-readable descriptions (not raw commit messages)
- Keep entries concise — one line per change

## Format rules

- Follow [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and [Semantic Versioning](https://semver.org/)
- Date format: `YYYY-MM-DD`
- New entries go at the top, under `## [Unreleased]` if no version is specified
- Link comparison URLs at the bottom of the file when a base URL is known

## Example entry

```markdown
## [Unreleased]

### Added
- `DateColumnFilter` component for filtering table columns by year, month, or date range
- Filter support on `Datum Reservation` and `Datum Beurkundet` columns in the Sales table
```

## When to use me

Use this skill whenever the user says "update the changelog", "add to changelog", or "document recent changes".
Ask for a version number if the user wants to cut a release entry.
