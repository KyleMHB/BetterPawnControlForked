# Better Pawn Control Forked

Better Pawn Control Forked is a RimWorld quality-of-life mod for switching colony policies in bulk. It lets you save and swap policy presets for colonists, animals, mechs, work, schedules, assignments, and supported weapon or loadout integrations.

This fork focuses on RimWorld 1.6 compatibility and safer interaction with other pawn-control and gear-management mods.

## Features

- **Policy presets** for outfits, food, drugs, reading, medicine, hostility response, and optional medicine inventory stock.
- **Schedule presets** including area restrictions.
- **Work presets** with optional Work Tab inner-priority support.
- **Animal, mech, robot, and loadout presets** for supported integrations.
- **Default policies** for new colonists, prisoners, and slaves.
- **Emergency toggle** for quickly applying configured policy sets.
- **Integration-aware apparel handling** for Outfit Stands Plus Forked.
- **Capability-based compatibility checks** for DLC and modded pawn setups, including nonstandard humanlike pawns with partial trackers.
- **Optional schedule compatibility** for Progression: Education class assignments.
- **Worktype-safe work policy handling** for Work Tab, FSF Complex Jobs, and other worktype overhauls.

## Installation

### Steam Workshop

Subscribe on Steam Workshop and enable **Harmony** and **Better Pawn Control Forked** in RimWorld's mod list.

### Manual Installation

1. Download or clone the repository.
2. Place the mod folder in your RimWorld `Mods` directory.
3. Enable **Harmony** and **Better Pawn Control Forked**.

## Usage

1. Open the relevant RimWorld tab, such as Assign, Schedule, Work, Animals, Mechs, or Weapons.
2. Click the BPC cog button to create or manage policies.
3. Select a policy from the BPC policy button.
4. Configure the normal RimWorld policy controls while that BPC policy is active.
5. Switch between BPC policies when you want those saved settings reapplied.

## Configuration

Configuration is stored in RimWorld's normal mod settings and the in-game policy editors.

Key settings and supported behaviors include:

- default policies for new pawns
- emergency policy switching
- optional Work Tab integration
- optional animal, mech, robot, and loadout integrations
- optional Progression: Education schedule syncing
- capability-aware handling for DLC and modded pawns
- Outfit Stands Plus Forked apparel policy triggering

## Building and Packaging

Prerequisite: the .NET 8 SDK. All RimWorld, Harmony, and .NET Framework reference dependencies are restored at pinned versions.

From the repository root:

```powershell
.\deploy.ps1 -Version 2.9.0 -Configuration Release
```

The script restores, builds, tests, validates metadata and XML, and creates `artifacts/BetterPawnControlForked-2.9.0.zip`. It does not publish, tag, upload, or install the package.

## Testing and Validation

Automated core and package checks run as part of `deploy.ps1`. The required in-game compatibility and profiling matrix is maintained in [TESTING.md](TESTING.md).

Version 2.9 automatically migrates original Better Pawn Control and pre-2.9 fork save data to schema 2. Keep only the original or the fork active, never both. Temporarily unavailable optional-mod records are retained and become usable again when their definitions return.

## Contributing & Forking Policy

> Contributions, issues, and feature requests are welcome.
>
> **Forking Policy:** If your fork primarily consists of bug fixes or feature additions that align with the core vision of this project, I reserve the right to request that your changes be submitted as a Pull Request to this existing codebase rather than being published as a completely separate standalone release, package, listing, or distribution.

## Links

- **Steam Workshop:** <http://steamcommunity.com/sharedfiles/filedetails/?id=3724294345>

## License

> This project is a fork and inherits the original project's license. See the original project for license terms: <https://github.com/voult2/BetterPawnControl>.

## Credits

- VouLT for the original Better Pawn Control mod.
- Fluffy for Animal Tab integration help.
- Skyarkhangel for Combat Realism integration support.
- Marnador for the logo font.
- Lauri7x3, Coldmoon, boundir, Proxyer, 53N4, Crusader, Deno226, Ionfrigate, and others for translations and updates.
- debugzxcv, muggenhor, TheLonerD, DomB, and other contributors for integration and compatibility work.
- Machado for the BetterPawnControl ProgressionEducation Patch: <https://steamcommunity.com/sharedfiles/filedetails/?id=3673605975>.





