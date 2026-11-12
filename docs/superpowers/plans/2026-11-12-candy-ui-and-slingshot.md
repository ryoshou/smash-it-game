# Candy UI, One-Piece Slingshot, Simple Ball Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restyle every screen in the approved "candy buttons" style A, rebuild the slingshot as one carved piece with
matching grip and wraps, and swap the yarn ball for a simple ball, with no frame-rate cost.

**Architecture:** The look lives in the editor: `UiSprites` draws a few small 9-sliced sprites and icons once,
`UiStyle` composes them into candy buttons, round icon buttons, badges and ribbon panels, and the existing scene
builders (`UiSceneParts`, `MetaSceneParts`, `MonetizationSceneParts`) call it. Runtime UI scripts change only where a
label becomes an icon or a new label appears. Models come from the Blender scripts.

**Tech Stack:** Unity 6000.6 URP, uGUI (legacy `Text`, `Image` sliced, `Shadow`/`Outline`), Lilita One, Blender
Python (`tools/blender`), NUnit EditMode/PlayMode.

**Spec:** `docs/superpowers/specs/2026-11-12-candy-ui-and-slingshot-design.md`

## Global Constraints

- Clean-room: nothing copied from Royal Smash; icons are drawn by our editor code; no real emoji glyphs in the game.
- No packages, SDKs or downloads. Never hand-edit `.unity` / `.prefab` / `.asset` / `.meta`; rebuild with
  `tools/unity/run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`, then revert churn:
  `git checkout -- Assets/_Game/Animation/Cat.controller Assets/_Game/Prefabs/CafeBackdrop.prefab Assets/_Game/Prefabs/LibraryBackdrop.prefab`.
- Every new file under `Assets/_Game/Art/Generated/` gets a row in `docs/asset-licenses.md` (`LicenseLogTests`).
- Player-visible text lives in `UiStrings`. GameObject names that tests find (`Play`, `Rooms`, `Settings`,
  `Remove Ads`, `Pause`, `Primary`, `Secondary`, `Home`, `Back`, `Room N`, …) stay unchanged.
- Outlines thin: rim ≈ 2–2.5% of a button's height (≈ 3 px on a 130-px button at 1080×1920 reference), lip shadow
  ≈ 8 px, text stroke ≤ 2 px. No stars.
- Performance: no new canvases, cameras or post effects; sprites ≤ 128×128; layout static after Show.
- Slingshot ≤ 1,500 triangles; `BandAnchorL/R` at (±0.13, 0, 0.192). Ball model diameter 0.18, pink family.
- Approved feel frozen (ShotTuning, camera, pouch, ball physics, Room 1 layouts, back desk).
- Commits: no `-n`-style tokens on the command line (hook); footer lines from the session.

## Review Focus

1. **Long labels in a smaller button** (e.g. No Ads with a price in another locale, "Restore purchases"): text must
   shrink to fit, never overflow the rim → `resizeTextForBestFit` with a min size on every candy label; Task 2 test.
2. **No Ads hidden after purchase**: the gear must stay where it is and nothing else shifts → Task 3 test.
3. **Feast / Hard level names in the HUD badge** ("Level 37 · Feast!"): must fit the badge → Task 4 best-fit test.
4. **Locked room cards** keep reading as locked (alpha) in the new card look → Task 5 keeps `CanvasGroup` and its test.
5. **Pressed state**: `ButtonPress` must still sink the face onto the lip for every candy and icon button → Task 2 test.

---

### Task 1: Candy sprites and icons (`UiSprites`)

**Files:**
- Create: `Assets/_Game/Scripts/Editor/UiSprites.cs`
- Modify: `Assets/_Game/Scripts/Editor/UiStyle.cs` (move `EnsureSprite`/`RoundedRect` into `UiSprites`)
- Modify: `docs/asset-licenses.md`
- Test: `Assets/Tests/EditMode/UiSpritesTests.cs`

**Interfaces:**
- Produces (all `internal static Sprite` properties on `UiSprites`, generated on first use into
  `Assets/_Game/Art/Generated/Ui/`): `Rounded`, `Circle` (unchanged files), `CandyFace` (white rounded rect, vertical
  gradient alpha-free: top 100% → bottom ~82% brightness, a soft highlight band in the top third), `CandyRim` (rounded
  outline ring, transparent inside, ring width 3 px of 128, 9-sliced, same corner radius as `Rounded`), `RibbonFace`
  (like `CandyFace`, radius 24), and icons `IconGear`, `IconPause`, `IconMap`, `IconHome`, `IconReplay`, `IconNext`,
  `IconNoAds` (white shapes on transparent, 128×128, drawn from circles/rects/polygons in code).
- Produces: `internal const int RimPixels = 3;`

- [ ] **Step 1: Write the failing test** `UiSpritesTests`:
  `EverySprite_IsGeneratedAndSmall` (each property non-null, texture ≤ 128×128, no mipmaps);
  `Rim_IsThin` (centre pixel alpha 0; pixel at x = RimPixels+1 on the mid row alpha 0; pixel at x = 1 alpha > 0.5);
  `Face_IsLighterAtTheTop` (top-row-centre brightness > bottom-row-centre brightness);
  `Icons_HaveInkAndClearCorners` (each icon: some pixel alpha > 0.5; corner pixel (0,0) alpha 0).
- [ ] **Step 2: Run** `tools/unity/batch-tests.sh editmode` → FAIL (`UiSprites` missing).
- [ ] **Step 3: Implement `UiSprites`** (one private `Ensure(string path, Func<Texture2D> draw, Vector4 border)` +
  one small draw function per sprite; icons from signed-distance helpers for circle, box, segment and triangle, AA
  by clamping distance). `UiStyle.Rounded/Circle` delegate to `UiSprites`. Add licence rows (generated by
  `UiSprites.cs`, project-owned).
- [ ] **Step 4: Run** EditMode → PASS (all, incl. `LicenseLogTests`).
- [ ] **Step 5: Commit** `feat: candy sprites and drawn icons`.

### Task 2: Candy building blocks in `UiStyle`

**Files:**
- Modify: `Assets/_Game/Scripts/Editor/UiStyle.cs`
- Test: `Assets/Tests/EditMode/UiStyleTests.cs`

**Interfaces:**
- Consumes: `UiSprites.*` (Task 1).
- Produces:
  - `internal readonly struct CandyLook { Color Top; Color Rim; }` with static `Green` (#7BE35A / #1F6B14),
    `Blue` (#5FB8FF / #17457E), `Red` (#FF6B6B / #7A1414), `Yellow` (#FFD84A / #8A4B00), `Purple` (#5B4BD6 / #241A6E),
    `Orange` (#FF9A3C / #7A2E00), `Cream` (#FFF6E6 / #8A4B00). The face is `CandyFace` tinted `Top` (the gradient
    is in the sprite).
  - `(Button, Text) Candy(RectTransform root, CandyLook look, int fontSize)` — face (CandyFace) + rim (CandyRim tinted
    `Rim`) + lip (`Shadow` in `Rim`, distance (0, −8)), white label with `Outline` in `Rim` width 2, best fit
    (min = min(24, fontSize)), `ButtonPress` wired as today. Replaces `Button(root, fontSize, ButtonLook)`; remove
    `ButtonLook` and update all callers in the same task with these mappings: Primary → `Green`, Secondary → `Blue`.
  - `Button IconButton(RectTransform root, CandyLook look, Sprite icon)` — circle face + rim + lip, icon `Image`
    centred at 55% of the size, tinted `Rim`, `raycastTarget = false`.
  - `Text Badge(RectTransform root, CandyLook look, int fontSize)` — non-interactive candy pill with a white
    best-fit label (min 28).
  - `Text RibbonPanel(RectTransform panel, string ribbon)` — Cream candy panel (rim + lip) and a Yellow ribbon
    across its top edge (overhanging by 4% each side) holding a white title label; returns the title `Text`.
  - `Title(Text)` becomes yellow (#FFD84A) with a brown (#5A2A00) `Outline` width 3 and a `Shadow` (0, −8) in the same
    brown.
- [ ] **Step 1: Write the failing test** `UiStyleTests` (EditMode, builds into a temp Canvas GameObject, destroyed in
  TearDown): `Candy_HasFaceRimLipAndBestFitLabel` (child "Face" Image sprite == CandyFace; a child Image with sprite
  CandyRim and color == look.Rim; `Shadow` on face with effectDistance.y == −8; label.resizeTextForBestFit true and
  min ≤ 24; label `Outline` effectDistance.x == 2); `Candy_PressSinksTheFace` (ButtonPress references face and lip);
  `IconButton_ShowsIconNotText` (no Text child; an Image with the given icon sprite); `RibbonPanel_ReturnsRibbonTitle`.
- [ ] **Step 2: Run** EditMode → FAIL.
- [ ] **Step 3: Implement**, then update callers (`UiSceneParts`, `MetaSceneParts`, `MonetizationSceneParts`) to
  compile with the mapping above (screens are restyled properly in Tasks 3–5).
- [ ] **Step 4: Run** `tools/unity/compile.sh`, EditMode → PASS.
- [ ] **Step 5: Commit** `feat: candy buttons, icon buttons, badges and ribbon panels`.

### Task 3: Home screen

**Files:**
- Modify: `Assets/_Game/Scripts/Editor/MetaSceneParts.cs` (`BuildHome`)
- Modify: `Assets/_Game/Scripts/Runtime/UI/HomeScreen.cs`
- Modify: `Assets/_Game/Scripts/Runtime/Meta/UiStrings.cs` (+ regenerate the asset through its builder)
- Modify tests: `Assets/Tests/PlayMode/AppFlowPlayTests.cs`, `Assets/Tests/PlayMode/RoomMapPlayTests.cs`
- Test: `Assets/Tests/PlayMode/HomeLayoutPlayTests.cs`

**Interfaces:**
- Consumes: `UiStyle.Candy`, `IconButton`, `Badge`, `Title`, `CandyLook` (Task 2); `UiSprites.IconGear/IconMap/IconNoAds`.
- Produces: `UiStrings.play = "Play"`, `UiStrings.noAds = "No Ads"`, `UiStrings.subtitle = "cat chaos!"`;
  `HomeScreen` serialized fields `levelText` (badge, `levelFormat`), `subtitleText`; `settingsLabel` removed (gear
  icon); Play label = `strings.play`; Remove Ads label = `strings.noAds` (price stays on the store panel).
- Layout (anchors in the Home root, inside the safe area): level badge top-left (0.03,0.93)–(0.42,0.985) Purple;
  gear `Settings` IconButton top-right, 120×120 at the top-right corner; `Remove Ads` Red candy directly under it,
  right-aligned, (0.66,0.855)–(0.97,0.905), font 36; title (0,0.74)–(1,0.86) rotated −4°; subtitle under it
  (0,0.70)–(1,0.75), white with brown outline; `Play` Green (0.14,0.36)–(0.86,0.45) font 84; `Rooms` Blue centred under
  it (0.27,0.25)–(0.73,0.32) with `IconMap` left of the label.
- [ ] **Step 1: Write the failing test** `HomeLayoutPlayTests` (uses `AppTestKit.Launch`):
  `Home_GearAndNoAdsTopRight_NoAdsUnderGear` (Settings rect's centre x > 0.8·width and has no Text; Remove Ads rect
  top < Settings rect bottom and their right edges within 20 units); `Home_RoomsCentredUnderPlay` (|Rooms.center.x −
  Play.center.x| < 5, Rooms.top < Play.bottom); `Home_LevelOnTheBadge` (ShowsText "Level 4" for save 3, and Play
  shows "Play"); `Home_NoAdsHidden_GearStays` (after `SetRemoveAdsVisible(false)` the Settings rect position is
  unchanged).
- [ ] **Step 2: Update** `AppFlowPlayTests` line 42 → `ShowsText<HomeScreen>("Level 4")`, line 109 →
  `ShowsText<HomeScreen>("No Ads")`; `RoomMapPlayTests` line 97 → `ShowsText<HomeScreen>("Level 31")`.
- [ ] **Step 3: Run** `tools/unity/batch-tests.sh playmode` → the new tests FAIL.
- [ ] **Step 4: Implement**, rebuild the scene, revert churn.
- [ ] **Step 5: Run** both suites → PASS (only the known order-dependent `GameLevels_AreStableAtSpawn` R1_L02 failure
  may remain; note it, don't fix it here). Snapshot `--screen home` and compare with mockup `ui-style-a3.html`.
- [ ] **Step 6: Commit** `feat: candy home screen`.

### Task 4: HUD

**Files:**
- Modify: `Assets/_Game/Scripts/Editor/UiSceneParts.cs` (`BuildHud`, `HudPill` → badges, `PauseBar` removed)
- Test: `Assets/Tests/PlayMode/UiLayoutPlayTests.cs` (extend)

**Interfaces:**
- Consumes: `UiStyle.Badge`, `IconButton`, `UiSprites.IconPause`.
- Layout (unchanged offsets from 3be759a): `Level` badge Purple left; `Shots` badge Orange right of centre (yarn icon
  left of the text is optional; text keeps `shotsFormat`); `Pause` Yellow IconButton with `IconPause` at the right.
- [ ] **Step 1: Write the failing test** `Hud_LongLevelNameFits` (set `Hud` level text to "Level 37 · Feast!" via its
  public setter, `Canvas.ForceUpdateCanvases()`, assert the label's `cachedTextGenerator.characterCountVisible`
  equals the string length) and `Hud_PauseIsAnIcon` (Pause button has no Text child).
- [ ] **Step 2: Run** PlayMode → FAIL.
- [ ] **Step 3: Implement**, rebuild, revert churn.
- [ ] **Step 4: Run** PlayMode → PASS. Snapshot level 5 and check the bar.
- [ ] **Step 5: Commit** `feat: candy HUD`.

### Task 5: Result, pause/settings, room map, store and splash

**Files:**
- Modify: `Assets/_Game/Scripts/Editor/UiSceneParts.cs` (`BuildResult`, `BuildDebugPanel` keeps its look)
- Modify: `Assets/_Game/Scripts/Editor/MetaSceneParts.cs` (`BuildPause`, `BuildRoomMap`, `BuildSplash`)
- Modify: `Assets/_Game/Scripts/Editor/MonetizationSceneParts.cs` (store panel)
- Test: existing `ResultFlowPlayTests`, `RoomMapPlayTests`, `AppFlowPlayTests`, `SwitchViewPlayTests`

**Interfaces:**
- Consumes: `UiStyle.RibbonPanel`, `Candy`, `IconButton`, `CandyLook`, `UiSprites.IconHome/IconReplay/IconNext`.
- Result: dim backdrop behind the panel (Image, black 45%, raycast blocks); `RibbonPanel` with the title
  (win/lose text from `UiStrings`, unchanged); `Primary` Green, `Secondary` Blue; reveal bar fill Orange. No stars.
- Pause/settings: `RibbonPanel` titled `paused`/`settings`; Resume Green; other buttons Blue; switches keep
  `SwitchView`. Room map: `RibbonPanel` titled `rooms`; cards = Cream candy with the room theme colour as a thin
  left stripe, status chip Green (Play), Blue (Replay), grey (Locked); `CanvasGroup` alpha for locked stays. Store:
  `RibbonPanel`, Buy Green, Cancel Blue. Splash: room-colour background, candy `Title`.
- [ ] **Step 1: Run** the existing PlayMode suite before the change and note the result (baseline).
- [ ] **Step 2: Implement**, rebuild, revert churn.
- [ ] **Step 3: Run** PlayMode → same result as baseline (name-based tests prove the buttons survive the restyle,
  `Rooms_ShowsProgress_LaterRoomsLocked` proves the locked state).
- [ ] **Step 4: Snapshot** `--win`, `--screen pause`, `--screen rooms`; outlines thin, nothing clipped.
- [ ] **Step 5: Commit** `feat: candy panels for result, pause, rooms, store and splash`.

### Task 6: One-piece slingshot and simple ball

**Files:**
- Modify: `tools/blender/props/slingshot.py`
- Modify: `tools/blender/props/yarn_ball.py` (becomes the simple ball; file name kept so paths and licence rows stay)
- Regenerate: `Assets/_Game/Art/Generated/Slingshot.fbx`, `YarnBall.fbx` via `tools/blender/build.sh slingshot` /
  `yarn_ball`
- Test: `ArtAssetsTests` (existing anchor test), Blender `tools/blender/build.sh --tests`

**Interfaces:**
- Slingshot: one closed mesh. Build the Y as a single skin: a `Skin` modifier (or metaball body converted to mesh and
  decimated) over the polyline butt → crotch → elbow → tip for both arms, applied, then subdivided once and
  smooth-shaded so the crotch and elbows blend with no joints. Grip and tip wraps are faces of the same mesh (loop
  cuts on the handle/upper arms, inset and pushed in ≤ 2 mm, painted with the atlas cells), not separate tori.
  Colours: wood `tan`, grip a deeper wood/brown, wraps `red` (pick cells so the three sit together with the candy
  palette). `BandAnchorL/R` unchanged. ≤ 1,500 tris.
- Ball: one smooth UV sphere radius 0.09 (segments 20, rings 12), `pink` cell, no strands.
- [ ] **Step 1: Run** `tools/blender/build.sh --tests` (baseline) and EditMode `ArtAssetsTests`.
- [ ] **Step 2: Implement** both scripts; build both models; check the ArtCheck log has no "Art check failed".
- [ ] **Step 3: Run** Blender tests and the full EditMode suite → PASS (anchors at the fork tips).
- [ ] **Step 4: Rebuild** the scene, revert churn, snapshot the slingshot close-up; no visible seams at the crotch.
- [ ] **Step 5: Commit** `feat: one-piece carved slingshot and a simple ball`.

### Task 7: Final check and iOS export

- [ ] **Step 1: Run** both suites; PlayMode may show only the known R1_L02 order-dependent failure.
- [ ] **Step 2: Snapshot** home, play, win, pause, rooms; compare with the mockups.
- [ ] **Step 3: Export** `tools/unity/build.sh ios`; tell the owner to Clean Build Folder (⇧⌘K).
- [ ] **Step 4: Update** `CLAUDE.md` layout note if new files warrant it (`UiSprites`); commit `docs: …`.
