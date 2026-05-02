# Changelog

## 2026-05-02

### Added

- Added updated README and Steam Workshop description files for the documentation refresh.
- Added optional Outfit Stands Plus Forked integration for assignment preset apparel policy changes.
- Added package-id based compatibility detection for Defensive Positions Forked and Outfit Stands Plus Forked.
- Added optional load ordering after Defensive Positions Forked and Outfit Stands Plus Forked.
- Added Steam Workshop description documentation.

### Changed

- Hardened assignment policy loading so missing saved policies keep the pawn's current valid policy before falling back to BPC defaults.
- Changed Outfit Stands Plus triggering to be conservative: BPC only queues a stand use when the assigned stand is reachable and contains wearable policy-compatible apparel.
- Prevented Outfit Stands Plus triggering while pawn gear-flow jobs are active or queued, including Defensive Positions, Gear Up And Go, wear apparel, and remove apparel jobs.
- Removed the machine-specific post-build copy command from the project file.
- Rewrote the README for the forked 1.6 compatibility work.

### Fixed

- Fixed assignment inventory stock saving to use the provided inventory tracker instead of rereading the saved pawn field.
- Fixed the schedule policy load path so it no longer depends on calling a protected pawn tick method.
