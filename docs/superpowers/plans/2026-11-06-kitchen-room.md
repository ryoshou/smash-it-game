# Kitchen Room Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** a device build with 125 levels. A new Kitchen room (Room 2, levels 26–50) has tables crowded with food and dishes. Food splashes and dishes shatter when they hit the floor. 8 of its levels are easy "feasts" of 25–40 objects. The Café, Library and Gaming Den move to 51–125 unchanged.

**Architecture:**
- **Feasts:** a new `Difficulty.Feast` (appended) with its own bot band (feast 0.60–1.00) and HUD label.
- **Splash:** a new `BreakMode.Splash` (appended) behaves like Shatter (the prop swaps to pooled pieces) and also plays a splash VFX whose child "Splat" particle lies flat where the food landed and shrinks away. The splat is pure feedback: `PropFeedback` plays it from `GameEvents.Broke`; for Splash the event's position is the floor contact point, so a splat on a countertop sits on the countertop.
- **Kitchen:** one more room through the M4a path: `KitchenBuilder` (backdrop, desk look, theme, music), `Room2LevelBuilder` / `Room2Layouts(Later)` for the levels.
- **Renumbering:** today's Rooms 2–4 become 3–5 by moving files and assets (GUIDs kept) and rewriting ids (`R2_L26` → `R3_L51`). Layouts don't change; the level fingerprint (which leaves out the id) proves it, because the moved bot reports, with ids re-keyed, still pass `PlaytestReportTests`.

**Tech Stack:** Unity 6000.6.0f1, URP, PhysX, NUnit EditMode/PlayMode, Blender 5.2 generators. No new packages.

**Spec:**
- `docs/superpowers/specs/2026-11-06-kitchen-room-design.md` (this feature; it wins on conflict)
- `docs/superpowers/specs/2026-10-26-cat-desk-smash-design.md` (the game)

## Global Constraints

- **Branch:** `kitchen-room` off `main`.
  - **Commits:** `<type>: <description>`, each ending with:
    `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`
    `Claude-Session: https://claude.ai/code/session_0114uyL5M4bQzFm2VEtm8nCn`
  - Run each commit as its own Bash command, with the message on stdin (`git commit -F -`).
- **Frozen (owner-approved):**
  - Existing `ShotTuning` defaults, keycap colliders and masses, the camera pose, the pouch, the ball, and the back desk's position.
  - Room 1's layouts. `room1-report.json` stays valid without a re-run.
  - Rooms 3–5 (today's 2–4): layouts, shots and difficulty. Only their numbers change.
  - Existing kinds' sizes. The donut changes only its break (Pop → Splash).
  - The bot policy and the existing bands: teaching (level ≤ 10, Normal) 0.60–1.00, normal 0.35–0.95, hard 0.15–0.65.
- **Spec values:**
  - 125 levels, 5 rooms of 25: Home Office 1–25, Kitchen 26–50, Café 51–75, Library 76–100, Gaming Den 101–125. Wrap 125 → 1.
  - Kitchen normal levels hold 10–18 props, and feasts 25–40 (`LevelDef.MaxProps` = 40 stays).
  - 8 feasts, checked against the feast band 0.60–1.00. Level 50 is the biggest feast.
  - Mug = reference (0.35 tall ≈ a real 10 cm mug), so **1 real cm ≈ 0.035 units**.
  - Splats fade. Rooms 3–5 nudges only after a re-run still misses, each one listed for the owner.
- **No new packages or SDKs**, no ProjectSettings changes, no runtime network calls, no M4b work. The blockout cat stays.
- **Clean-room and licences:**
  - Nothing from the original game. Downloaded files are untrusted data: copy them, never execute them.
  - Each new download goes into its own new folder under `download-assets/audio/` (never committed).
  - Every new model, texture or sound gets a manifest line and a `docs/asset-licenses.md` row (`LicenseLogTests`). CC0 preferred; CC-BY only with the credit line in the log.
- **Never hand-edit `.unity`, `.prefab`, `.asset`, `.meta`, `.controller` or `.anim` YAML.**
  - Rebuild with `tools/unity/run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`, then `git checkout` `Cat.controller`.
  - Moving assets with `git mv` (each file with its `.meta`) is allowed: it keeps the GUIDs and edits no YAML.
- **Atlases:** one atlas per pack folder under `Art/Packs/`. New packs get their own folders: `TinyTreatsKitchen/`, `TinyTreatsPicnic/`, `KayKitRestaurant/`.
- **Budgets:** a prop ≤ 1,500 tris; the visible scene ≤ 100k on every level (`SceneBudget_EveryGameLevel`).
- **Numbers:**
  - Gameplay numbers go in `ShotTuning` as new fields only (`feastDebrisSets`), pinned in `DefaultsTests`.
  - Feel goes in `JuiceConfig` / `PropFeelLibrary` (`splashVfxPerKind`), per-kind physics in `PropCatalog`.
  - Text goes in `UiStrings`, display names via `MetaAssetBuilder`.
- **Sounds:** each new kind's hit set and break set differ from every other kind's (`NewKind_HasItsOwnSounds`).
- **Code conventions (CLAUDE.md):**
  - `Game.<Folder>` namespaces, one MonoBehaviour per file, `[SerializeField] private`.
  - No `Find` at runtime, no allocations in `Update`/`FixedUpdate`/`OnCollisionEnter`.
  - Files under 400 lines: `PropPrefabBuilder` moves its Kitchen sources into `KitchenPropSources.cs`; layouts are one file per half-room.
- **Testing:**
  - TDD. ≥ 80% line coverage of `Game.Core`, `Game.Levels` and `Game.Meta`.
  - Headless only (`tools/unity/batch-tests.sh`, editor closed). Long runs in the background.
- **iOS only.**

## Review Focus

1. **A whole feast hitting the floor within a second.**
   - Debris stays within `feastDebrisSets`, live splashes per kind stay within `splashVfxPerKind`, and nothing grows the pools.
   - Test: `Feast_AllLandAtOnce_StaysWithinPools` (Task 2).
2. **Food landing on backdrop furniture** (a `Solid` counter counts as floor).
   - The splat lies on the counter's top, not at floor height and not in mid-air.
   - Test: `Food_LandsOnFurniture_SplatsOnItsTop` (Task 2).
3. **Food that leaves the play volume** (breaks with no event).
   - No splash and no splat anywhere.
   - Test: `Food_LeavesPlayVolume_NoSplat` (Task 2).
4. **Restart, Next or Home while splats are fading.**
   - No splat from the old round is visible in the new one.
   - Test: `Restart_WhileSplatsFade_ClearsThem` (Task 2).
5. **Room starts after the renumber** (debug "Next room", wrap, reveal bar).
   - Next room goes 1 → 26 → 51 → 76 → 101 → 1, and after level 125 play wraps to level 1 in the Home Office.
   - Tests: `GameList_RoomStartsAt_1_26_51_76_101` and the renamed `NextAfterLastLevel_PlaysLevel1InHomeOffice` (Task 6).

---

### Task 1: Feast levels: difficulty, bot band, HUD label

**Files:**
- Modify:
  - `Runtime/Levels/Difficulty.cs`: append `Feast`.
  - `Runtime/Levels/ClearRateBand.cs`: Feast → `"feast"`, 0.60–1.00, whatever the level number.
  - `Runtime/Meta/UiStrings.cs`: `levelFeastFormat = "Level {0} · Feast!"`.
  - `Runtime/UI/Hud.cs`: picks the format by difficulty.
  - `Runtime/Core/ShotTuning.cs`: `feastDebrisSets = 12`.
  - `Runtime/World/DebrisPool.cs`: `Prewarm` uses `feastDebrisSets` on a Feast level, `maxDebrisSets` otherwise.
- Test: `ClearRateBandTests`, `DefaultsTests`, new PlayMode `FeastPlayTests`.

**Interfaces:**
- Produces:
  - `Difficulty.Feast` (= 2)
  - `ClearRateBand.For(Difficulty.Feast, n)` named `"feast"`
  - `UiStrings.levelFeastFormat`
  - `ShotTuning.feastDebrisSets`
  - `DebrisPool.Capacity` (int, read-only)

- [ ] **Step 1: Write the failing tests:**
  - `Feast_UsesTheFeastBand_AtAnyLevel` with `[TestCase(27)] [TestCase(50)] [TestCase(5)]`: `Name == "feast"`, `Min == 0.60f`, `Max == 1.00f`.
  - `ShotTuning_FeastDebrisSets_Is12`: a fresh instance has 12.
  - `ShotTuning_MaxDebrisSets_StillSix`, in `DefaultsTests`: the default is still 6.
  - PlayMode `FeastLevel_PrewarmsFeastDebrisSets`: `DebrisPool.Capacity` is 12 after loading a Feast level and 6 after a Normal one.
  - PlayMode `Hud_FeastLevel_ShowsFeastLabel`: the level text equals `string.Format(strings.levelFeastFormat, n)`.
- [ ] **Step 2: Run** `tools/unity/batch-tests.sh editmode` and `tools/unity/batch-tests.sh playmode`. Expected: the new tests FAIL (`Feast` undefined).
- [ ] **Step 3: Implement** the listed changes.
- [ ] **Step 4: Run** both suites. Expected: all pass.
- [ ] **Step 5: Commit** `feat: feast levels — difficulty, bot band and HUD label`.

### Task 2: Splash break: chunks, juicy burst, a splat that fades

**Files:**
- Modify:
  - `Runtime/Core/BreakMode.cs`: append `Splash` ("bursts into its pieces with a splash; a flat splat shrinks away").
  - `Runtime/Core/BreakRule.cs`: `HasPieces(BreakMode)`, true for Shatter and Splash.
  - `Runtime/World/DebrisPool.cs` and `LevelSpawner.cs`: use `BreakRule.HasPieces` instead of `== Shatter`.
  - `Runtime/World/DeskObject.cs`: on a Splash floor break, raise `Broke` with the first contact point instead of `Body.position`.
  - `Runtime/Juice/JuiceConfig.cs`: `splashVfxPerKind = 8`, `splatLift = 0.005f` (metres above the contact point).
  - `Runtime/Juice/PropFeedback.cs`:
    - Splash plays at `position + up * splatLift` and gets a Light haptic.
    - Splash kinds prewarm `splashVfxPerKind` copies.
  - `Runtime/Juice/VfxPool.cs`: `StopAll()`, called by `PropFeedback` when a level loads.
  - `Editor/VfxBuilder.cs`: `Splash(name, colour)`.
    - A droplet burst in the colour (like `Burst`), plus a child system named `Splat`: 1 particle, `ParticleSystemRenderMode.HorizontalBillboard`, speed 0, lifetime 1.8 s, and a size-over-lifetime curve 1 until 0.55, then 0 at 1.
  - `Editor/PropPrefabBuilder.cs`: the donut becomes Splash with a broken set per variant.
  - `tools/blender/fracture_sets.py`: `Donut_broken` (4–6 chunks, one set per variant's colour).
- Test: `BreakRulesTests`, `CatalogTests`, `ArtAssetsTests`, new PlayMode `SplashPlayTests`.

**Interfaces:**
- Produces:
  - `BreakMode.Splash` (= 4)
  - `BreakRule.HasPieces(BreakMode) → bool`
  - `VfxBuilder.Splash(string name, Color colour) → ParticleSystem`, whose prefab has a child `Splat`
  - `JuiceConfig.splashVfxPerKind`, `JuiceConfig.splatLift`

- [ ] **Step 1: Write the failing tests:**
  - EditMode:
    - `HasPieces_ShatterAndSplash_Only` covers all five modes.
    - `ShouldBreak_Splash_OnFloor`.
    - The `NewKinds_HaveTheirPersonality` case for Donut becomes `BreakMode.Splash, PropColliderShape.Box`.
    - `ShatterSpecs_HaveBrokenPrefabForEveryVariant` now covers Splash specs too, and its name becomes `PieceSpecs_HaveBrokenPrefabForEveryVariant`.
    - `SplashVfx_HasAFlatSplatThatShrinksToNothing`: for each Splash kind's `breakVfx` there is a child `Splat` with horizontal billboard rendering, lifetime ≤ 2.5 s, and a size curve evaluating to 0 at 1.
    - `BrokenSet_LinesUpWithIntactModel` cases for `Donut_broken`.
  - PlayMode (`SplashPlayTests`):
    - `Donut_HitsFloor_SplatsAndChunksThenFades`: a donut dropped from 1 m swaps to pieces, and an active `Splat` lies within 0.02 of y 0. After 2.5 s no `Splat` is alive.
    - `Food_LandsOnFurniture_SplatsOnItsTop`: a donut dropped onto a box at y 0.6 that has a `FloorSurface` gets its `Splat` within 0.02 of y 0.6.
    - `Food_LeavesPlayVolume_NoSplat`: a donut moved outside the play volume leaves no live `Splat`.
    - `Restart_WhileSplatsFade_ClearsThem`: break a donut, call `bootstrap.Retry()` within 0.5 s, and no `Splat` is alive on the next frame.
    - `Feast_AllLandAtOnce_StaysWithinPools`: 30 donuts dropped together leave `DebrisPool.ActiveSets` ≤ `feastDebrisSets` and live donut splashes ≤ `splashVfxPerKind`.
- [ ] **Step 2: Run** both suites. Expected: the new tests FAIL.
- [ ] **Step 3: Implement.**
  - Run `tools/blender/build.sh fracture`, then `run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`.
  - The donut's splash colour is pink icing; its sound stays as-is until Task 3.
- [ ] **Step 4: Run** both suites. Expected: all pass. Snapshot a donut mid-splash (`snapshot.sh out.png 0 --break`).
- [ ] **Step 5: Commit** `feat: splash break — food bursts into chunks and leaves a fading splat`.

### Task 3: New audio: the Kitchen loop and splat sounds

**Files:**
- Create: `download-assets/audio/<source-name>/` (not committed) for each new download.
- Modify:
  - `tools/assets/audio-manifest.txt`, `docs/asset-licenses.md`.
  - `Editor/SoundSetBuilder.cs`: `Break_Donut` becomes a splat set.
- Test: `LicenseLogTests`, `SoundSetTests`, `PropFeelLibraryTests`.

Requirements:
- **Music:** one cozy, loopable instrumental track, 85–115 bpm, in the mintodog loops' spirit (soft, playful, no vocals). CC0 preferred, CC-BY acceptable with credit. Ogg, at most 3 MB after import.
- **Splats:** at least 9 short wet "splat", "squish" or "squelch" one-shots, CC0, enough for 7 food kinds to each get a distinct set of 3.
- **Search:** OpenGameArt (CC0 filter), Kenney, Freesound (CC0 filter), in that order. Read each licence page and record its URL in the log.
- **Fallback, ledgered:** if fewer than 9 CC0 splats are found, make the rest from `rubberduck-breaking` `misc_*` and `wubitog-pops` clips with low `pitchAtSoft`/`pitchAtHard` (0.75/0.6) so they read wet.

- [ ] **Step 1: Write the failing tests:**
  - `MusicClip_Imported` case for the Kitchen file.
  - `Break_Donut_IsASplatSet`: no clip shared with `Break_Pop`.
- [ ] **Step 2: Run** `batch-tests.sh editmode`. Expected: FAIL.
- [ ] **Step 3: Source the files, add the manifest lines and licence rows, and run** `tools/assets/import-packs.sh` and `run-method.sh Game.Editor.SoundSetBuilder.BuildAll`.
- [ ] **Step 4: Run** `batch-tests.sh editmode`. Expected: all pass.
- [ ] **Step 5: Commit** `feat: Kitchen music and splat sounds (CC0)`.

### Task 4: Kitchen look: backdrop, desk, shell, music

**Files:**
- Create: `Editor/Rooms/KitchenBuilder.cs`
  - `ThemePath = ConfigFolder + "/Rooms/Kitchen.asset"`, `Build() → RoomTheme`, menu `Smash/Build Kitchen Room`.
  - Not yet in `GameLevelListBuilder.Rooms`; Task 6 adds it.
- Modify:
  - `tools/blender/props/office_desk.py` and `tools/blender/room_shell.py`: a `kitchen` STYLE.
    - Desk: a farmhouse table, warm oak top.
    - Shell: soft mint walls, a terracotta tiled floor.
  - `tools/assets/pack-manifest.txt`: Charming Kitchen pieces into `TinyTreatsKitchen/`.
    - Decor: countertop run, sink, stove, extractor hood, fridge, wall cabinets, dish rack, kettle, toaster.
  - `docs/asset-licenses.md`.
- Test: `RoomThemeTests`, `SceneBudgetTests`, `ArtAssetsTests`.

Rules:
- Decor goes against the walls via `RoomKit.FlushSideWall` / `FlushBackWall`. Furniture a falling prop can reach gets `RoomKit.Solid`.
- Nothing enters the shot corridor or covers the table in the camera view.
- The background colour comes from the shell's wall tint.

- [ ] **Step 1: Write the failing tests:**
  - `KitchenTheme_HasBackdropMusicAndBackground`: loads `KitchenBuilder.ThemePath`. The music is Task 3's track, and no other room theme uses it.
  - `KitchenBackdrop_HasItsDeskAtTheOrigin`: the same assertions as `EveryRoomBackdrop_HasItsDeskAtTheOrigin`, on the Kitchen theme.
  - `KitchenBackdrop_UnderTriangleBudget`: backdrop + scene ≤ 60k, leaving room for 40 props.
  - `PackProp_ImportsUpright` cases for the new decor.
- [ ] **Step 2: Run.** Expected: FAIL.
- [ ] **Step 3: Implement.** Run `tools/blender/build.sh` (desk and shell), `import-packs.sh` and `run-method.sh Game.Editor.KitchenBuilder.Build`.
- [ ] **Step 4: Run** `batch-tests.sh editmode`. Expected: all pass. The Kitchen snapshot waits for Task 6, because `snapshot.sh --room` finds a room by its index, which Task 6 wires up.
- [ ] **Step 5: Commit** `feat: Kitchen backdrop, table and music`.

### Task 5: Kitchen kinds: 14 foods and dishes, true to size

**Files:**
- Modify:
  - `Runtime/Levels/PropKind.cs`: append under `// Kitchen (levels 26–50)` in this order: `DinnerPlate, Bowl, Teapot, WineGlass, JamJar, KetchupBottle, DrinkCan, Apple, Tomato, Cheese, Burger, Cupcake, Croissant, Sandwich`.
  - `Editor/PropPrefabBuilder.cs`: makes its `Source` type internal and pulls sources from the new file.
  - `Editor/SoundSetBuilder.cs`, `Editor/VfxBuilder.cs`, `Editor/MetaAssetBuilder.cs` (display names, e.g. "Dinner Plate", "Ketchup").
  - `Editor/Levels/LayoutKit.cs`: a height constant per kind.
  - `tools/blender/fracture_sets.py`: a broken set per Shatter and Splash kind. A food's set may be assembled from its pack's cut pieces (apple pieces, tomato slices, chopped cheese, bun halves, lettuce and patty); otherwise it is fractured.
  - Manifests (new folders `TinyTreatsPicnic/`, `KayKitRestaurant/`; `TinyTreatsBakedGoods/` exists), `docs/asset-licenses.md`.
- Create: `Editor/KitchenPropSources.cs`, `internal static IEnumerable<PropPrefabBuilder.Source> All()`.
- Test: `CatalogTests`, `PropFeelLibraryTests`, `ArtAssetsTests`, `SoundSetTests`, `BreakPlayTests`, `WorldPlayTests`.

Starting values. Size is the spec's real size × 0.035: set `visualScale` so the measured mesh hits the target ±10%. Mass and physics are Claude's call; tune on device.

| Kind | Pack model | Target size (units) | Mass | Break | Pieces | Splash/VFX colour |
|---|---|---|---|---|---|---|
| DinnerPlate | Picnic `plate_A` | 0.80 wide, ~0.07 tall | 0.3 | Shatter | 8 | white chips |
| Bowl | Picnic `bowl` | 0.50 wide, 0.20 tall | 0.25 | Shatter | 8 | sky chips |
| Teapot | Picnic `teapot` | 0.50 tall | 0.6 | Shatter | 10 | cream chips |
| WineGlass | Picnic `wine_glass` | 0.63 tall | 0.15 | Shatter | 8 | glass sparkle |
| JamJar | Picnic `jam` | 0.35 tall | 0.3 | Splash | 6 | berry red |
| KetchupBottle | KayKit `ketchup` | 0.70 tall | 0.35 | Splash | 6 | ketchup red |
| DrinkCan | Picnic `drink_can` | 0.42 tall | 0.15 | Pop | — | silver fizz |
| Apple | Picnic `apple` | 0.27 across | 0.12 | Splash | 4 | apple red/green |
| Tomato | KayKit `food_ingredient_tomato` | 0.25 across | 0.1 | Splash | 4 | tomato red |
| Cheese | Picnic `cheese_A` | 0.35 wide | 0.15 | Splash | 4 | cheese yellow |
| Burger | KayKit `food_burger` | 0.35 wide, 0.28 tall | 0.15 | Splash | 5 | brown and red |
| Cupcake | Baked Goods `cupcake` | 0.25 tall | 0.06 | Splash | 4 | pink |
| Croissant | Baked Goods `croissant` | 0.42 long | 0.05 | Splash | 4 | golden |
| Sandwich | Picnic `sandwich` | 0.35 wide | 0.1 | Splash | 4 | lettuce and tomato |

- **Colliders:** ConvexMesh. Use Box where a flat food must stack (Cheese, Sandwich), as M4a did for the donut.
- **Effects:** every kind has its own hit set, break set and break VFX. Food break sets come from Task 3's splats. Splash kinds use `VfxBuilder.Splash`.
- **Variants:** one per kind unless the pack has colour variants. Apple may use `apple` red and a green variant only if the pack has one.
- **Reuse ruling (spec §5):** the Kitchen reuses only the mug (and the paper cup, which needs no resize). It doesn't use the Café's donut, so the Café keeps introducing the plate, donut and milk bottle.
- **Jam jar and ketchup** are glass and plastic, but what you see is their contents, so they splash rather than shatter (Claude's call).

- [ ] **Step 1: Write the failing tests:**
  - `EveryKind_HasSpecWithPrefab` now covers 32 kinds.
  - `NewKinds_HaveTheirPersonality` cases for the 14 kinds (modes and colliders as in the table).
  - `KitchenKind_IsTrueToSize` with `[TestCase(kind, target)]` for every row: the measured max dimension is within ±10% of the target.
  - `KitchenKinds_KeepRealOrder`:
    - Apple height < Mug height < Teapot height < KetchupBottle height.
    - DinnerPlate width > Bowl width > Mug height.
    - DrinkCan height > SodaCan height.
  - `NewKind_HasItsOwnSounds` and `NewKind_HasItsOwnBreakEffect` cases for the 14 kinds.
  - `PackProp_ImportsUpright` and `BrokenSet_LinesUpWithIntactModel` cases.
  - `EveryProp_UnderTriangleBudget` (existing) covers them.
  - PlayMode:
    - `KitchenKinds_AreStableAtSpawn`: all 14 kinds spaced on the desk, asleep and upright after 3 s.
    - `Teapot_HitsFloor_Shatters`.
    - `Apple_HitsFloor_Splashes`.
- [ ] **Step 2: Run.** Expected: FAIL.
- [ ] **Step 3: Implement.** Run `import-packs.sh`, `tools/blender/build.sh fracture` and `run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`.
- [ ] **Step 4: Run** both suites. Expected: all pass. Snapshot all 14 on the Kitchen table beside a mug and check by eye that nothing looks oversized.
- [ ] **Step 5: Commit** `feat: 14 kitchen props, true to size`.

### Task 6: Kitchen levels 26–50 and the 125-level game

**Files:**
- Move, by `git mv` with `.meta` files, **in this order** (4→5, then 3→4, then 2→3):
  - `Editor/Levels/Room<N>LevelBuilder.cs`, `Room<N>Layouts.cs` and `Room<N>LayoutsLater.cs` → `Room<N+1>…`. Rename the classes, `Room` constants and menu items.
  - `Assets/_Game/Levels/Room<N>/` → `Room<N+1>/`, each `R<N>_L<k>.asset` → `R<N+1>_L<k+25>.asset`, and `Room<N>LevelList.asset` → `Room<N+1>LevelList.asset`.
  - `docs/playtest/room<N>-report.{json,md}` → `room<N+1>-report.*`. A scratchpad script re-keys each row's id the same way (`R2_L26` → `R3_L51`) and leaves every fingerprint and rate untouched.
- Rewrite: every `"R<N>_L<k>"` literal in the moved builders → `"R<N+1>_L<k+25>"` (scripted).
- Create: `Editor/Levels/Room2LevelBuilder.cs`, `Room2Layouts.cs` (L26–37), `Room2LayoutsLater.cs` (L38–50).
- Modify:
  - `GameLevelListBuilder`: `RoomCount = 5`; add `KitchenBuilder` at room 2 in `Rooms`; add the five level builders in order. Fix the comment (125 levels).
  - `Tests/EditMode`: `RoomLevelTests`, `RoomThemeTests`, `SceneBudgetTests` and `PlaytestReportTests` gain room 5 cases, and the Kitchen gets its own rules (below).
  - `Tests/PlayMode/PlaytestBot.cs`: `[Values(1, 2, 3, 4, 5)]`.
  - `RoomFlowPlayTests`: rename `NextAfterLevel100…` to `NextAfterLastLevel_PlaysLevel1InHomeOffice`.
  - `tools/unity/playtest-bot.sh`: default rooms `1 2 3 4 5`.
  - `WorldPlayTests.GameLevels_AreStableAtSpawn`: timeout 3,600,000.
  - Every "100 levels" or "levels 26–50 Café" comment and doc line, including `CLAUDE.md`.

**Room rules for the Kitchen.**
- `Room_Has25Levels_WithHardEveryFifth` and `Room_ShotsWithinFiveToTen` run for rooms 1, 3, 4 and 5.
- Room 2 has its own `Kitchen_…` tests instead.
- Every other per-room test runs for all five rooms.

| Rule | Kitchen | Other rooms (unchanged) |
|---|---|---|
| Feasts | 27, 31, 34, 37, 41, 44, 47, 50 | none |
| Hard | 30, 35, 40, 45 | every 5th global level |
| Shots | normal 5–10, feast 8–12 | 5–10 |
| Props | normal 10–18, feast 25–40 | as today |
| Back desk | 29, 33, 38, 43, 48, 50 (light props only up there; tall heavy props outside x ±0.6) | as today, numbers +25 |

**Kitchen level table.** Ideas, kinds and shots are decided; positions are the builder's, proven by validation and the bot. **New** marks a kind's introduction. Kinds: M mug, P paper cup, DP dinner plate, Bw bowl, T teapot, WG wine glass, J jam jar, KB ketchup, DC drink can, Ap apple, To tomato, Ch cheese, Bu burger, Cc cupcake, Cr croissant, Sw sandwich.

| # | Idea | Kinds | Shots | Diff | Props |
|---|---|---|---|---|---|
| 26 | **New: dinner plate, apple**: apples heaped on two plates | DP, Ap | 5 | N | 12 |
| 27 | **Feast:** a fruit-and-plate spread end to end | DP, Ap, M | 8 | Feast | 26 |
| 28 | **New: bowl, tomato**: tomato-filled bowls in a row | Bw, To | 6 | N | 14 |
| 29 | Plates of apples on the back desk, bowls in front | DP, Ap, Bw, To | 7 | N | 14 |
| 30 | A plate-and-bowl tower crowned with fruit | DP, Bw, Ap, To | 8 | Hard | 12 |
| 31 | **Feast:** salad bar, bowls and tomatoes everywhere | Bw, To, Ap, DP | 9 | Feast | 30 |
| 32 | **New: teapot, cheese**: a tea table with cheese plates | T, Ch, DP, M | 6 | N | 13 |
| 33 | Teapots on the back desk behind cheese stacks | T, Ch, M | 7 | N | 12 |
| 34 | **Feast:** afternoon tea, mugs, plates and cheese | T, M, DP, Ch, Ap | 9 | Feast | 32 |
| 35 | Wide cheese-and-tomato spread, few shots | Ch, To, Bw | 7 | Hard | 16 |
| 36 | **New: jam jar, cupcake**: cupcake rows with jars | J, Cc, DP | 6 | N | 15 |
| 37 | **Feast:** the bake sale, cupcakes on every plate | Cc, DP, J, M | 9 | Feast | 34 |
| 38 | **New: wine glass, burger**: burgers and glasses, two desks | WG, Bu, DP | 7 | N | 14 |
| 39 | Burger stacks on plate plinths | Bu, DP, Bw | 7 | N | 15 |
| 40 | A glass pyramid guarding the burgers | WG, Bu, J | 9 | Hard | 14 |
| 41 | **Feast:** the diner, burgers, tomatoes and cheese | Bu, To, Ch, DP, M | 10 | Feast | 35 |
| 42 | **New: ketchup, croissant**: ketchup bottles over croissants | KB, Cr, DP | 6 | N | 14 |
| 43 | Ketchup on the back desk, croissant piles in front | KB, Cr, Bw | 8 | N | 14 |
| 44 | **Feast:** breakfast, croissants, jam, cupcakes and teapots | Cr, J, Cc, T, DP | 10 | Feast | 36 |
| 45 | Tall condiment towers among glasses | KB, J, WG, DP | 8 | Hard | 16 |
| 46 | **New: drink can, sandwich**: a picnic row | DC, Sw, Ap | 6 | N | 16 |
| 47 | **Feast:** the picnic, sandwiches, cans and fruit | Sw, DC, Ap, To, Ch | 10 | Feast | 38 |
| 48 | Cans on the back desk behind sandwich stacks | DC, Sw, Bw | 8 | N | 16 |
| 49 | Every dish stacked into a leaning dish tower | DP, Bw, M, T, WG | 8 | N | 18 |
| 50 | **Grand feast finale:** every kitchen kind, two desks | all Kitchen + M | 12 | Feast | 40 |

Where physics forbids an idea, adapt it and ledger the ruling. Feasts are heaped but never tall towers: a feast is a reward.

- [ ] **Step 1: Do the moves and rewrites** for rooms 4→5, 3→4, 2→3.
  - Add `KitchenBuilder` at room 2 in `Rooms`.
  - Leave `Room2LevelBuilder` out of `RoomLevels` for now (it doesn't exist yet).
  - Run `run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`.
  - Expected: `PlaytestReportTests` passes for rooms 3–5 on the re-keyed reports, which proves the layouts didn't change.
  - Record this in the ledger.
- [ ] **Step 2: Write the failing tests:**
  - `RoomLevelTests` gains room 2 and room 5 cases. The Kitchen rows:
    - `Kitchen_FeastsAndHardOnSchedule`
    - `Kitchen_ShotsWithinBands`
    - `Kitchen_PropCountsWithinBands`
  - `Room_TwoDeskLevels_UseTheBackDesk` cases: room 2 `{29, 33, 38, 43, 48, 50}`; rooms 3–5 the old numbers + 25.
  - `GameList_IsTheRoomsInOrder`: 125 levels, ids `R<room>_L<number>`.
  - `GameList_RoomStartsAt_1_26_51_76_101`: `NextRoomStart` chains 0 → 25 → 50 → 75 → 100 → 0.
  - `GameList_NewKindsArriveOnSchedule`, comparing per-level sets of introductions in PropKind order:
    - `1:Keycap1u`, `3:Mug`, `4:Book`, `5:CactusPot`, `12:PaperCup`, `21:DeskLamp`
    - `26:DinnerPlate,Apple`, `28:Bowl,Tomato`, `32:Teapot,Cheese`, `36:JamJar,Cupcake`, `38:WineGlass,Burger`, `42:KetchupBottle,Croissant`, `46:DrinkCan,Sandwich`
    - `51:Plate`, `61:Donut`, `71:MilkBottle`
    - `76:Candle`, `86:Hourglass`, `96:PencilCup`
    - `101:SodaCan`, `111:GameController`, `116:FizzySoda`
  - `EveryRoomBackdrop_HasItsDeskAtTheOrigin` with `[TestCase(5)]`.
  - `PlaytestReport_CoversEveryLevelWithCurrentLayout` with `[TestCase(5)]`.
- [ ] **Step 3: Run** `batch-tests.sh editmode`. Expected: FAIL (no Room 2 list).
- [ ] **Step 4: Build the layouts,** add `Room2LevelBuilder.BuildAll` at room 2 in `RoomLevels`, and run `run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`. Then run the stability test (scratchpad `stable.sh`), and fix spawn instability until all 125 levels pass.
- [ ] **Step 5: Run** `SMASH_BOT_ROOMS="2 3" tools/unity/playtest-bot.sh` in the background.
  - Room 3 (Café) re-runs because the donut's break changed.
  - Re-shape any Kitchen level outside its band, re-running with `SMASH_BOT_LEVELS`, and ledger each re-shape.
  - For a Café level out of band: re-run it first. Only if it misses again, nudge it minimally and ledger the nudge for the owner.
- [ ] **Step 6: Run** both suites. Expected: all pass. Snapshot levels 26, 27, 34, 41 and 50 (`snapshot.sh out.png <index> --room 2`).
- [ ] **Step 7: Commit** `feat: Kitchen levels 26–50; the game grows to 125 levels`.

### Task 7: Full run, device build and owner hand-off

- [ ] **Step 1: Run** both suites with `--coverage`. Expected:
  - All pass.
  - Coverage of `Game.Core`, `Game.Levels` and `Game.Meta` ≥ 80% each, excluding the data-only ScriptableObjects, as in M4a.
- [ ] **Step 2: Run** `tools/unity/build.sh ios`. Expected: 0 errors.
- [ ] **Step 3: Update docs:**
  - `CLAUDE.md`: the goal line and the room list.
  - The memory index.
  - Commit `docs: Kitchen status`.
- [ ] **Step 4: Hand-off checklist for the owner:**
  - Play L26–50 on the phone.
  - Do feasts feel satisfying and easy?
  - Do the sizes look real next to the mug?
  - Do splats fade, and is there no stutter when a feast lands (watch the frame-rate meter in a development build)?
  - Check the Kitchen's look and music.
  - Check the donut's new splash in the Café.
  - List every nudge made to Rooms 3–5.

## Out of scope

M4b (MAX, IAP, consent/ATT, Firebase, release build), the final cat, resizing existing kinds, and the back-desk wedge fix.
