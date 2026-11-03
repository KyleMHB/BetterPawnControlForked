# Issue tracker: Local Markdown

Issues and specs for this repository live as Markdown files in `.scratch/`.

## Conventions

- One feature per directory: `.scratch/<feature-slug>/`
- The specification is `.scratch/<feature-slug>/spec.md`.
- Implementation issues are one file per ticket at `.scratch/<feature-slug>/<ticket-slug>.md`.
- Triage state is recorded as a `Status:` line near the top of each issue file. See `triage-labels.md` for the status values.
- Comments and conversation history are appended under a `## Comments` heading at the bottom of the file.

Existing non-ticket artifacts under `.scratch/` may keep their current layout. New specifications and issues must follow the paths above.

## When a skill says "publish to the issue tracker"

Create a new Markdown file under `.scratch/<feature-slug>/`, creating the feature directory when needed.

## When a skill says "fetch the relevant ticket"

Read the referenced file. The user will normally provide its path or ticket slug directly.

## Wayfinding operations

Used by `/wayfinder`. The map is one file with one child file per ticket.

- **Map**: `.scratch/<effort>/map.md`, containing Notes, Decisions-so-far, and Fog.
- **Child ticket**: `.scratch/<effort>/<ticket-slug>.md`, with the question in the body. A `Type:` line records `research`, `prototype`, `grilling`, or `task`; a `Status:` line records its current state.
- **Blocking**: a `Blocked by: <ticket-slug>, <ticket-slug>` line near the top. A ticket is unblocked when every listed file is resolved.
- **Frontier**: scan the effort directory for ticket files that are open, unblocked, and unclaimed; the first listed ticket wins.
- **Claim**: set `Status: claimed` and save before starting work.
- **Resolve**: append the answer under an `## Answer` heading, set `Status: resolved`, then append a context pointer to the map's Decisions-so-far in `map.md`.
