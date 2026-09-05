# Changelog

## 2.9.1 - 2026-09-05

### Fixed

- Fixed the Assign tab reopening path so it preserves policy values changed through other interfaces until a BPC policy is explicitly selected.

### Validation

- Release build and local runtime deployment passed on 2026-09-05.
- Source and runtime DLL versions are `2.9.1.0`; SHA-256 hashes match, runtime `About/Version.xml` reports `2.9.1`, required folders are present, and no runtime PDB files were deployed.
- User-confirmed RimWorld Dev Quicktest passed for this change set.
- The full in-game compatibility and profiling matrix remains pending.

## 2.9.0 - 2026-08-28

### Added

- Added schema 2 migration for original Better Pawn Control and pre-2.9 fork saves, with one deterministic migration summary.
- Added dependency-free core tests, pinned build dependencies, Windows validation workflow, and deterministic local ZIP packaging.
- Added original-mod incompatibility metadata and runtime duplicate-patch protection.

### Changed

- Moved persistent policy, link, default, emergency, clipboard, and active-map state into the current world component.
- Stored work types and Work Tab work givers by `defName`, retaining unresolved optional-mod records across saves.
- Stored emergency selections by policy ID and fall back once to policy 0 when a selected policy was deleted.
- Hardened Combat Extended, Compositable Loadouts, Work Tab, and Progression: Education reflection failures so only the affected integration disables.
- Reduced pawn-table roster enumeration and cached immutable UI textures.

### Fixed

- Fixed null pawn links crashing guest, recruitment, faction-change, and slave lifecycle patches.
- Fixed gravship and single-colony map transitions so links and active policy selections move together and replace stale destination defaults.
- Fixed schedule interruption tracking so area and timetable changes accumulate.
- Preserved weapons, robots, loadouts, and other optional feature data while integrations are unavailable.

### Removed

- Removed the stale root assembly, forced garbage collection, static persistent manager collections, and unused `Pawn.Tick` publicizer configuration.


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
