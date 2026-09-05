# Testing

## Automated release validation

From the repository root, run:

```powershell
.\deploy.ps1 -Version 2.9.1 -Configuration Release
```

The command restores pinned dependencies, builds the RimWorld 1.6 assembly, runs the dependency-free xUnit suite, parses all XML, checks English translation key uniqueness, verifies version agreement, stages only allowed package files, strips PDBs, and creates a deterministic ZIP in `artifacts/`.

Expected result:

- Release build: zero warnings and zero errors
- Core tests: all pass
- Package: `artifacts/BetterPawnControlForked-2.9.1.zip`
- ZIP roots: `About`, `Common`, `v1.6`, and `LoadFolders.xml`
- Assemblies: only `v1.6/Assemblies/BetterPawnControlForked.dll`

Observed on 2026-08-28:

- Release build passed with 0 warnings and 0 errors.
- 11 core tests passed.
- XML, translation key, metadata, DLL version, and package-content validation passed.
- Two consecutive package runs produced SHA-256 `16591E0BD855D3638D5B9E540D983F464BF2FEA31369A666339FA9B2F1AD29DC`.

## In-game smoke validation

- User-confirmed RimWorld Dev Quicktest: passed for the 2.9.1 change set.

This smoke test does not replace the compatibility and profiling matrix below.

## Required in-game 1.6 matrix

Record the RimWorld build, DLC set, mod versions, result, and relevant `Player.log` excerpt for each run.

- [ ] New vanilla 1.6 colony with Harmony only.
- [ ] Load, save, and reload a pre-2.9 fork save.
- [ ] Replace original Better Pawn Control with the fork and verify automatic preset import.
- [ ] Start a second colony without restarting RimWorld and confirm no state leakage.
- [ ] Exercise guest departure, refugee completion, faction change, slave conversion, and slave purchase.
- [ ] Create non-default policies in every tab, perform gravship takeoff and landing, and confirm active selections and emergency state survive.
- [ ] Test two home maps plus a temporary encounter and confirm no unrelated state migration.
- [ ] Remove an optional integration, save, restore it, and verify its presets return.
- [ ] Delete presets assigned to both emergency levels and confirm fallback to policy 0.
- [ ] Work Tab with FSF Complex Jobs.
- [ ] Outfit Stands Plus Forked with Defensive Positions Forked.
- [ ] Combat Extended loadout save and apply.
- [ ] Compositable Loadouts save and apply.
- [ ] Progression: Education class schedule synchronization.
- [ ] Smoke-test Animal Tab, Numbers, WeaponsTabReborn, Misc. Robots, Assign Animal Food, and Children, School and Learning.
- [ ] Confirm no new BPC errors, repeated warnings, stuck quests, reset priorities, or lost policies in `Player.log`.

## Profiling matrix

Capture before and after samples for policy switching and closing a relevant pawn table. Use the same colony, camera position, active mods, and policy data for each comparison.

| Pawns | Policy switch before | Policy switch 2.9.0 | UI close before | UI close 2.9.0 | Result |
| ---: | ---: | ---: | ---: | ---: | --- |
| 20 | Pending | Pending | Pending | Pending | Pending |
| 50 | Pending | Pending | Pending | Pending | Pending |
| 100 | Pending | Pending | Pending | Pending | Pending |

The release gate remains open until every required in-game row passes and the profiler rows contain measured values.

## Targeted compiled-assembly regression

After building the mod assembly, run:

```powershell
.\Tests\AssignTabOpenRegressionTests.ps1
```

Expected result:

- `PASS: opening Assign does not apply a saved policy.`
- `PASS: explicit Assign policy selection still applies saved state.`

The script loads RimWorld's managed assemblies to resolve the compiled patch method. Pass `-AssemblyPath` and `-ManagedPath` when validating a non-default installation.
