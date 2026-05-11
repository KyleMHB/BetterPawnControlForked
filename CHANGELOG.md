# Changelog

## 2026-05-11

### Fixed

- Prevented Better Pawn Control from logging repeated `Could not find player faction.` errors while RimWorld has no player faction during startup or early map setup.

## 2026-05-02

### Added

- Added runtime pawn capability checks so DLC and modded humanlike pawns can participate only in the policy systems they actually support.
- Added optional Progression: Education schedule syncing, based on Machado's BetterPawnControl ProgressionEducation Patch, so class timetable updates are mirrored into BPC schedule links when that mod is installed.
- Added optional load ordering after FSF Complex Jobs and Progression: Education.
- Added updated README and Steam Workshop description files for the documentation refresh.
- Added optional Outfit Stands Plus Forked integration for assignment preset apparel policy changes.
- Added package-id based compatibility detection for Defensive Positions Forked and Outfit Stands Plus Forked.
- Added optional load ordering after Defensive Positions Forked and Outfit Stands Plus Forked.
- Added Steam Workshop description documentation.

### Changed

- Hardened assign, schedule, work, and weapons policy save/load paths against missing trackers, non-vanilla pawns, removed defs, and custom worktype mods.
- Improved Work Tab integration failure handling so reflection or custom workgiver failures skip the affected inner priorities instead of breaking policy application.
- Hardened assignment policy loading so missing saved policies keep the pawn's current valid policy before falling back to BPC defaults.
- Changed Outfit Stands Plus triggering to be conservative: BPC only queues a stand use when the assigned stand is reachable and contains wearable policy-compatible apparel.
- Prevented Outfit Stands Plus triggering while pawn gear-flow jobs are active or queued, including Defensive Positions, Gear Up And Go, wear apparel, and remove apparel jobs.
- Removed the machine-specific post-build copy command from the project file.
- Rewrote the README for the forked 1.6 compatibility work.

### Fixed

- Fixed humanlike policy cleanup so valid modded or DLC-controlled pawns are not removed just because they are not vanilla colonists.
- Fixed schedule serialization to resolve time assignment defs by name and repair missing or short schedules when modded defs are unavailable.
- Fixed assignment inventory stock saving to use the provided inventory tracker instead of rereading the saved pawn field.
- Fixed the schedule policy load path so it no longer depends on calling a protected pawn tick method.
