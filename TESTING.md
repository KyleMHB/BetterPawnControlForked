# Testing

## Automated release validation

From the repository root, run:

```powershell
.\deploy.ps1 -Version 2.9.2 -Configuration Release
```

The command restores pinned dependencies, builds the RimWorld 1.6 assembly, runs the dependency-free xUnit suite and production-source regression scripts, parses all XML, checks English translation key uniqueness, verifies version agreement, stages only allowed package files, strips PDBs, and creates a deterministic ZIP in `artifacts/`. It requires the .NET 8 SDK and PowerShell 7.

Expected result:

- Release build: zero warnings and zero errors
- Core tests: all pass
- Package: `artifacts/BetterPawnControlForked-2.9.2.zip`
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

## Workshop regression validation for 2.9.2

Observed on 2026-10-06:

- Release build: zero warnings and zero errors.
- xUnit: 20 passed, zero failed or skipped, including nine new policy-default tests.
- Portable tab opening: passed for vanilla Animals, Mechs, Animal Tab and Numbers animal windows.
- WVC: 16 production application scenarios passed, plus settings and package-detection checks.
- Policy-default wiring: 44 checks passed.
- Local compiled tab routes and the existing Assign regression: passed.
- XML, translation uniqueness, version agreement and package-content checks: passed.
- Package SHA-256: `0766B2FBF55F7B7844A89A769BC16D9B0197698252C620EE63BE6D4D0BC5698F`.
- Local deployment on 2026-10-06: installed to `E:/Steam/steamapps/common/RimWorld/Mods/BetterPawnControlForked`; all 27 runtime files match the validated package. DLL version is `2.9.2.0`, metadata is `2.9.2`, and no PDBs are present. The previous installation was backed up under `.scratch/workshop-comments-2026-10-06/deployment-20261006-070646-ac39fdc4/previous-runtime.zip`. RimWorld was running at verification, so a restart is needed to load the new DLL. In-game validation remains pending.

The packaging command runs these portable checks in separate PowerShell processes:

```powershell
pwsh -NoProfile -File .\Tests\TabStateOpenRegressionTests.ps1 -SkipCompiledChecks
pwsh -NoProfile -File .\Tests\MechWorkModeRegressionTests.ps1
pwsh -NoProfile -File .\Tests\PolicyDefaultsRegressionTests.ps1
```

The tab script executes the actual opening patch and session helper with game doubles. The mech script checks WVC detection, settings and the production preset application body. The default script checks the actual manager properties, database patch callbacks and capture/restore methods. The xUnit suite also checks the shared policy-default logic.

With the local game installed, also run the tab script without `-SkipCompiledChecks` to check the compiled routes for explicit selection, outgoing state capture, emergency mode, paste and deleted-preset fallback:

```powershell
pwsh -NoProfile -File .\Tests\TabStateOpenRegressionTests.ps1
```

These checks do not execute Unity UI, Harmony dispatch, WVC jobs or Scribe save/reload. In-game validation for this release remains pending:

- [ ] Change an animal's area outside Animals, reopen the table, then switch presets and back. Repeat with Animal Tab and Numbers.
- [ ] Change a mech's area, mode and group through the inspector/gizmos with Mechs closed, then reopen. Verify those edits survive. Test explicit presets, emergency, paste and deletion fallback separately.
- [ ] With WVC active, verify the enabled-by-default work-mode option skips direct mode restoration. Uncheck it and verify BPC mode control returns. Check destination-group mode behavior and save/reload.
- [ ] Change colonist apparel, food, drug and reading defaults through BPC and vanilla Make default. Check agreement, newly joined pawns, save/reload and deletion fallback. Browsing a policy must not change the default.
- [ ] Confirm colonist default changes preserve existing prisoner, slave and animal defaults.

## Schedule opening regression

The release command also runs the portable Schedule regression:

```powershell
pwsh -NoProfile -File .\Tests\ScheduleTabOpenRegressionTests.ps1 -SkipCompiledChecks
```

With the local game installed, run it without `-SkipCompiledChecks` after building:

```powershell
pwsh -NoProfile -File .\Tests\ScheduleTabOpenRegressionTests.ps1
```

Observed on 2026-10-06:

- Before the fix, the new regression failed because opening Schedule replaced external area B with saved area A.
- After the fix, production opening preserves area B, all 24 live timetable assignments and the current job. It establishes the table session and resets paste UI.
- Production close captures live areas and timetables. Selection round trips restore captured values, and explicit paste applies copied values.
- Compiled routes retain selection, capture before showing the policy menu, close, paste, emergency application, deleted-preset fallback and arrival application.
- Assign, Animals, Mechs, Animal Tab and Numbers opening checks passed.
- Release validation passed with zero build warnings/errors, 20 passing core tests, all portable regressions and package checks. The local 2.9.2 package containing this unreleased fix has SHA-256 `9B6DC9ABF7F7DB658736D789FF4719B087323E3CBC2836E876E27E1B00E3FA21`. It has not been deployed or published.

The script compiles the actual patch, session helper, selection-menu and manager method bodies against game doubles. Outgoing selection capture is executed through the production manager body; its UI wiring is checked in the compiled DLL. Emergency, deletion and arrival checks establish compiled call routes, not gameplay behavior. Unity UI, Harmony dispatch, optional integrations and Scribe save/reload remain unexecuted.

Required in-game checks for this fix remain pending. Record the exact RimWorld build, DLC and mod versions, scenario and relevant `Player.log` evidence:

- [ ] Change a colonist's area through the inspector with Schedule closed, then open Schedule and verify the area remains unchanged. Repeat with live timetable edits.
- [ ] Close Schedule, switch presets and back, and verify areas and timetables were captured. Repeat after external edits while Schedule was closed.
- [ ] Save and reload, reopen Schedule, and verify areas, timetables and active presets.
- [ ] Repeat on another home map and verify the first map's state is preserved.
- [ ] Exercise explicit selection, paste, emergency transitions, active-preset deletion fallback and arrival application.
- [ ] With Progression: Education active, verify class timetable edits survive opening and explicit presets still synchronize classes. Repeat with other affected optional Schedule interfaces in the active mod set.
- [ ] Confirm no new BPC errors or repeated warnings in `Player.log`.
