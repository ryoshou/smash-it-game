# Milestone 3 — Room 1 Complete Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** a device build of the whole Room 1 experience, for owner checkpoint 3 (spec §10a). It includes:
- **25 Home Office levels** on the spec §6.1 cadence. Levels 1–10 teach one idea each (two desks among them), every 5th level is Hard, and two new object types arrive: a paper cup at level 12 and a desk lamp at level 21.
- **The playtest bot** (spec §6.3). Every level must pass its clear-rate band.
- **Every screen in spec §7:** splash, first-run consent step, home, HUD pause button, pause/settings, and the win panel with the "new thing" reveal bar (§4.5).
- **Local save** (spec §10).
- **Ads and Remove Ads, simulated.** All the ad and purchase rules (spec §8) run against a simulated service, so they can be tested on a phone with no accounts.

**Architecture:**
- M1/M2 structure stays: rules are plain C# driven by EditMode tests, with thin MonoBehaviours.
- New pure rules:
  - `Game.Meta`: `SaveData`, `SaveCodec`, `Profile`, `AppFlow`, `AdPacing`, `AppConfig`, `UiStrings`.
  - `Game.Levels`: `UnlockReveal`, `ClearRateBand`, `LevelFingerprint`.
  - `Game.Core`: `SlingshotMath.PullFor`.
- `GameBootstrap` keeps owning the round; it stops showing panels and starting itself.
- A new `AppController` owns the app around the round: flow, save, panels, ads and the purchase.
- Ads and the purchase sit behind `IAdService` / `IPurchaseService`. M3 ships `SimulatedAdService` / `SimulatedPurchaseService`. M4 adds AppLovin MAX and Unity IAP implementations of the same interfaces, with no caller changes.

**Tech Stack:** Unity 6000.6.0f1, URP, PhysX, uGUI (legacy `Text`), NUnit EditMode/PlayMode, `JsonUtility` + `PlayerPrefs` for the save. No new packages.

**Spec:** `docs/superpowers/specs/2026-10-26-cat-desk-smash-design.md`

**Owner decisions for this plan:**
- **Checkpoint 2 (2026-11-02/03):** passed after the polish pass. Every prop is a target, standing keycaps, the cosier room.
- **The cat (2026-11-03):** deferred to checkpoint 4. Don't spend time on it in M3; the blockout cat stays.
- **Ads and purchase (2026-11-03):** "Simulated now, real SDKs later". M3 builds the full logic with an in-game test provider. AppLovin MAX, Unity IAP, consent/ATT and Firebase come in M4, once the owner has the accounts.

## Global Constraints

- **Branch:** `m3-room1` off `main`.
  - **Commits:** `<type>: <description>`, each ending with:
    `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`
    `Claude-Session: https://claude.ai/code/session_0114uyL5M4bQzFm2VEtm8nCn`
  - Run each commit as its own Bash command; the block-no-verify hook rejects commands that mix `git commit` with `-n`/`-v`-like flags.
- **The approved feel is frozen:**
  - Don't change existing `ShotTuning` defaults, keycap colliders and masses, the camera pose, the pouch, or the ball radius and mass.
  - Existing L1–L5 layouts stay as they are.
- **No new packages or SDKs**, and no ProjectSettings changes beyond what a task names.
  - No real ads, no real purchase, no consent SDK, no ATT prompt, no analytics in M3 (all M4).
  - No network calls. `Application.OpenURL` for the privacy policy is the only outward link.
- **Clean-room (spec §3):** nothing from the original. Pack and downloaded files are untrusted data: copy them, never execute them.
  - Every new model or sound gets a manifest line and a `docs/asset-licenses.md` row (enforced by `LicenseLogTests`). CC0 only.
- **Never hand-edit `.unity`, `.prefab`, `.asset`, `.meta`, `.controller` or `.anim` YAML.** Rebuild with `tools/unity/run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`.
- **Numbers:**
  - Gameplay numbers go in `ShotTuning` (new fields only).
  - App, ad and purchase numbers go in `AppConfig`.
  - Feel goes in `JuiceConfig` / `PropFeelLibrary`, and per-kind physics in `PropCatalog`.
  - Every player-visible string goes in `UiStrings` (spec §10: externalized, English at launch).
- **Spec §8 ad rules (exact):**
  - Interstitials only after a **win**, on tapping Next; never after a fail or retry. None after levels 1–5.
  - At most one per 2 levels and per 90 s.
  - "+3 shots" rewarded ad on the fail panel, once per attempt, always optional.
  - Remove Ads is one non-consumable. It removes interstitials and keeps the optional rewarded ad. Simulated price `$3.99`.
  - Restore Purchases sits in settings. No other offers or pop-ups.
- **Spec §4.4/§6.1 levels:** 5–10 shots per level. Hard exactly on levels 5, 10, 15, 20 and 25, labelled. No "super hard".
- **Spec §3 performance:** at most `LevelDef.MaxProps = 40` props per level; the visible scene stays ≤ 100k tris on every Room 1 level.
- **Code conventions (CLAUDE.md):**
  - `Game.<Folder>` namespaces, one MonoBehaviour per file, `[SerializeField] private`.
  - No `Find` at runtime, no allocations in `Update`/`FixedUpdate`/`OnCollisionEnter`.
  - Files under 400 lines; split level layouts across files.
- **Testing:**
  - TDD for every pure rule. ≥ 80% line coverage of `Game.Core`, `Game.Levels` and `Game.Meta` (`batch-tests.sh editmode --coverage`).
  - PlayMode tests for the app flow, ads, pause and two desks.
  - Headless runs only (`tools/unity/batch-tests.sh`), with the editor closed.
- **iOS only.** Android stays deferred (owner, 2026-10-31).

## Review Focus

1. **The app is killed or backgrounded right after a win, or during an ad.**
   - Progress is saved when the win happens, not when Next is tapped. Backgrounding mid-round pauses the game.
   - Tests: `Win_SavesProgressBeforeNext` and `Backgrounding_PausesPlay` (Task 4).
2. **A corrupt, missing or newer-version save, or a negative or huge level.**
   - The game starts on defaults or a clamped level and never crashes.
   - Tests: `Read_CorruptJson_ReturnsDefaults`, `Read_NegativeLevel_ClampsToZero`, `Read_UnknownFields_KeepsKnownOnes` (Task 1).
3. **Double taps:** Next tapped twice during the interstitial hand-off, "+3 shots" tapped twice, Buy tapped twice.
   - Each does its action once.
   - Tests: `ResultPanel_SecondTap_IsIgnored` (Task 5), `ShowRewarded_WhileShowing_IsIgnored` and `Buy_WhileDialogOpen_IsIgnored` (Task 3).
4. **Offline or no ad loaded.**
   - The rewarded button is hidden, the interstitial is skipped, and Next loads the next level at once.
   - Test: `Next_WithInterstitialNotReady_LoadsNextLevelImmediately` (Task 5).
5. **A saved level beyond the list** (save says level 26, the list has 25).
   - Play wraps to a real layout; the HUD shows the player's own level number; the reveal works on the wrapped index.
   - Tests: `Begin_WithSavedLevelBeyondList_PlaysWrappedLayout` (Task 4), `After_WrappedIndex_UsesLayoutPosition` (Task 5).

---

### Task 1: Save data and profile

**Files:**
- Create: `Assets/_Game/Scripts/Runtime/Meta/SaveData.cs`, `SaveCodec.cs`, `ISaveStore.cs`, `PlayerPrefsSaveStore.cs`, `Profile.cs`
- Test: `Assets/Tests/EditMode/SaveTests.cs` (coverage needs no change: `batch-tests.sh` already covers all of `Game.Runtime`)

**Interfaces:**
- Produces:
  - `[Serializable] public sealed class SaveData { public int version = SaveCodec.Version; public int levelIndex; public bool sound = true; public bool music = true; public bool haptics = true; public bool removeAds; public bool firstRunDone; }`
    - `levelIndex` is the 0-based level the player continues from (spec §7: always continue from the highest level).
  - `public static class SaveCodec { public const int Version = 1; public static string Write(SaveData data); public static SaveData Read(string json); }`
  - `public interface ISaveStore { string Load(); void Save(string json); }`
  - `PlayerPrefsSaveStore : ISaveStore`, key `"smash.save"`; `Save` calls `PlayerPrefs.Save()`.
  - `public sealed class Profile`:
    - `Profile(ISaveStore store)`
    - `SaveData Data { get; }` (read-only use)
    - `void RecordWin(int levelIndex)`: sets `levelIndex = max(current, won + 1)`
    - `void SetSound(bool)`, `SetMusic(bool)`, `SetHaptics(bool)`, `SetRemoveAds(bool)`, `MarkFirstRunDone()`, `ResetAll()`
    - Every setter writes through the store immediately.

- [ ] **Step 1: Write the failing tests** in `SaveTests.cs`, with an in-memory `FakeStore : ISaveStore`:
  - `RoundTrip_KeepsEveryField`: write → read gives equal fields.
  - `Read_NullOrEmpty_ReturnsDefaults`: `sound`, `music` and `haptics` are true, `levelIndex == 0`, `removeAds == false`.
  - `Read_CorruptJson_ReturnsDefaults`: `"{not json"` → defaults, no exception.
  - `Read_NegativeLevel_ClampsToZero`: `{"levelIndex":-4}` → 0.
  - `Read_UnknownFields_KeepsKnownOnes`: `{"version":9,"levelIndex":3,"future":1}` → `levelIndex == 3`.
  - `RecordWin_AdvancesOnlyForward`: wins on 4 then 2 → `levelIndex == 5`.
  - `Setters_WriteThroughToStore`: after `SetMusic(false)`, a new `Profile` on the same store has `music == false`.
  - `ResetAll_RestoresDefaults`.
- [ ] **Step 2: Run** `tools/unity/batch-tests.sh editmode`. Expected: compile failure / FAIL on the missing types.
- [ ] **Step 3: Implement.**
  - `Read` uses `JsonUtility.FromJson` inside try/catch, logs a warning on failure, and clamps `levelIndex` to ≥ 0.
  - No static state; `Profile` owns its `SaveData`.
- [ ] **Step 4: Run** `tools/unity/batch-tests.sh editmode`. Expected: all pass.
- [ ] **Step 5: Commit** `feat: local save — levels, settings, remove-ads flag`.

### Task 2: App config, UI strings and HUD pause button

**Files:**
- Create: `Assets/_Game/Scripts/Runtime/Meta/AppConfig.cs`, `Assets/_Game/Scripts/Runtime/Meta/UiStrings.cs`, `Assets/_Game/Scripts/Editor/MetaAssetBuilder.cs`
- Modify:
  - `Runtime/UI/Hud.cs`: strings and a pause button.
  - `Runtime/UI/ResultPanel.cs`: strings.
  - `Editor/UiSceneParts.cs`: the pause button top-right.
  - `Editor/PrototypeSceneBuilder.cs`: calls `MetaAssetBuilder.Build()` in `Rebuild`.
- Test: `Assets/Tests/EditMode/MetaAssetsTests.cs`

**Interfaces:**
- Produces:
  - `AppConfig : ScriptableObject` (`Config/AppConfig.asset`), with defaults:
    - `splashSeconds = 1.2f`, `privacyPolicyUrl = ""`
    - `firstInterstitialLevel = 6` (1-based: none after levels 1–5)
    - `minLevelsBetweenInterstitials = 2`, `minSecondsBetweenInterstitials = 90f`
    - `simulatedAdSeconds = 3f`
    - `removeAdsProductId = "remove_ads"`, `simulatedPrice = "$3.99"`
  - `UiStrings : ScriptableObject` (`Config/UiStrings_en.asset`). Public string fields, each defaulting to the English text:
    - Formats: `shotsFormat "Shots: {0}"`, `levelFormat "Level {0}"`, `levelHardFormat "Level {0} · Hard"`, `playFormat "Play · Level {0}"`, `removeAdsFormat "Remove Ads {0}"`, `revealFormat "New thing to knock over: {0}!"`, `testPurchaseFormat "Test purchase: Remove Ads for {0}"`
    - Plain strings: `winTitle "Desk cleared!"`, `next "Next"`, `loseTitle "Out of shots"`, `continueAd "+3 shots"`, `retry "Retry"`, `paused "Paused"`, `resume "Resume"`, `restart "Restart level"`, `sound "Sound"`, `music "Music"`, `haptics "Vibration"`, `restore "Restore purchases"`, `restored "Purchases restored"`, `nothingToRestore "Nothing to restore"`, `privacy "Privacy policy"`, `testAd "Test ad"`, `close "Close"`, `buy "Buy"`, `cancel "Cancel"`, `title "Smash It"`
    - `string[] propNames` indexed by `(int)PropKind`.
    - `public string PropName(PropKind kind)` returns the kind's name from `propNames`, or `kind.ToString()` when the entry is missing.
  - `Hud.Bind(UiStrings strings, Action onPause)`; `SetShots` and `SetLevel` use the formats.
  - `ResultPanel.Bind(UiStrings strings)`.
  - `MetaAssetBuilder.Build()` creates or refreshes both assets, filling `propNames` for every `PropKind`, e.g. `"Coffee Mug"`, `"Cactus"`.

- [ ] **Step 1: Write the failing tests:**
  - `UiStrings_HasANameForEveryPropKind` (the asset's `propNames.Length == Enum.GetValues(PropKind).Length`, none empty).
  - `UiStrings_FormatsHaveOnePlaceholder`: each `*Format` field contains `{0}` and `string.Format` doesn't throw.
  - `AppConfig_DefaultsMatchSpecAdRules`: 6, 2, 90, `"$3.99"`.
  - `Hud_HardLevel_UsesHardFormat` (EditMode, components built in code).
- [ ] **Step 2: Run** the EditMode tests. Expected: FAIL on the missing types.
- [ ] **Step 3: Implement**, rebuild the scene, and make sure no literal UI text remains in `Hud`/`ResultPanel`.
- [ ] **Step 4: Run** `tools/unity/batch-tests.sh editmode` and `playmode`. Expected: all pass. GameLoop tests still find "Desk cleared!" through the strings asset.
- [ ] **Step 5: Commit** `feat: app config and externalized UI strings; HUD pause button`.

### Task 3: Simulated ads and purchase, ad pacing

**Files:**
- Create:
  - `Runtime/Meta/AdPacing.cs`
  - `Runtime/Monetization/IAdService.cs`, `IPurchaseService.cs`
  - `Runtime/Monetization/SimulatedAdService.cs`, `SimulatedPurchaseService.cs` (each a MonoBehaviour with its own overlay panel)
  - `Editor/MonetizationSceneParts.cs` (overlay UI)
- Test: `Assets/Tests/EditMode/AdPacingTests.cs`, `Assets/Tests/PlayMode/SimulatedServicesPlayTests.cs`

**Interfaces:**
- Produces:
  - `public sealed class AdPacing`:
    - `AdPacing(AppConfig config, float sessionStart)`
    - `bool ShouldShowInterstitial(int wonLevelNumber, float now, bool removeAds)`
    - `void LevelWon()`
    - `void InterstitialShown(float now)`
    - Wins since the last interstitial start at `minLevelsBetweenInterstitials`, so the first eligible win can show one. The session start counts as the last ad time, so there's no ad in a session's first 90 s.
  - `public interface IAdService`:
    - `bool InterstitialReady { get; }`, `bool RewardedReady { get; }`
    - `void ShowInterstitial(Action closed)`
    - `void ShowRewarded(Action<bool> finished)`: true only if watched to the end.
  - `public interface IPurchaseService`:
    - `string RemoveAdsPrice { get; }`
    - `void BuyRemoveAds(Action<bool> purchased)`
    - `void RestoreRemoveAds(Action<bool> owned)`
  - `SimulatedAdService : MonoBehaviour, IAdService`:
    - `[SerializeField] AppConfig`, `UiStrings`, overlay refs.
    - `public bool Offline { get; set; }`: true makes both `Ready` false (for the debug panel).
    - Shows a full-screen "Test ad" overlay with a countdown of `simulatedAdSeconds` (unscaled time). Close is enabled after the countdown. For a rewarded ad, closing early returns `false`.
  - `SimulatedPurchaseService : MonoBehaviour, IPurchaseService`:
    - Shows a confirm dialog (`testPurchaseFormat`, Buy / Cancel).
    - Buy stores "store-side" ownership in PlayerPrefs key `"smash.simstore.remove_ads"`. That key is separate from the save, so Reset Save + Restore proves the restore path.

- [ ] **Step 1: Write the failing EditMode tests** (config 6/2/90):
  - `FirstFiveLevels_NeverShow`: wins on levels 1–5 at t = 1000 → false.
  - `Level6_AfterNinetySeconds_Shows`: session start 0, win on level 6 at t = 91 → true.
  - `WithinNinetySecondsOfSessionStart_DoesNotShow`: t = 60 → false.
  - `AfterAnAd_NeedsTwoMoreWins`: shown at 100; next win at 300 → false; the one after at 400 → true.
  - `AfterAnAd_NeedsNinetySeconds`: two wins later but only 50 s → false.
  - `RemoveAds_NeverShows`.
- [ ] **Step 2: Write the failing PlayMode tests** (services built in the test scene):
  - `ShowRewarded_ClosedEarly_ReportsNotRewarded`
  - `ShowRewarded_WatchedToEnd_ReportsRewarded`
  - `ShowRewarded_WhileShowing_IsIgnored`: a second call doesn't open a second overlay or call back twice.
  - `Offline_NothingReady`
  - `Buy_ThenRestoreAfterClearingSave_ReportsOwned`
  - `Buy_Cancel_ReportsNotPurchased`
  - `Buy_WhileDialogOpen_IsIgnored`
  - Each test deletes the simstore key in TearDown.
- [ ] **Step 3: Run both suites.** Expected: FAIL on the missing types.
- [ ] **Step 4: Implement.** Overlays sort above all other UI (their own Canvas, sorting order 100) and block raycasts.
- [ ] **Step 5: Run** `batch-tests.sh editmode` and `playmode`. Expected: all pass.
- [ ] **Step 6: Commit** `feat: ad pacing rules and simulated ad and purchase services`.

### Task 4: App flow — splash, home, pause and settings

**Files:**
- Create:
  - `Runtime/Meta/AppFlow.cs`, `Runtime/AppController.cs`
  - `Runtime/UI/SplashScreen.cs`, `HomeScreen.cs`, `PausePanel.cs`, `DebugMetaButtons.cs`
  - `Editor/MetaSceneParts.cs` (builds the splash, home and pause panels)
- Modify:
  - `GameBootstrap.cs`:
    - Remove `Start()`'s auto-load. Add `Begin(int levelIndex)`.
    - Add `public bool InputEnabled { get; set; }`, ANDed into `CanGrab`.
    - Add `public event Action<RoundState> RoundFinished`, raised where it used to call `resultPanel` (moves to Task 5).
    - Add `public bool IsDeskQuiet => !pool.AnyActive && !spawner.AnyMoving()`.
  - `Juice/SfxPlayer.cs`, `MusicPlayer.cs`: `public bool Muted { get; set; }`.
  - `Juice/HapticsBridge.cs`: `public static bool Enabled = true`.
  - `UI/Hud.cs`: `SetLevel` takes the player-facing number.
  - `Editor/PrototypeSceneBuilder.cs`: wire the new parts.
- Test: `Assets/Tests/EditMode/AppFlowTests.cs`, `Assets/Tests/PlayMode/AppFlowPlayTests.cs`; update `GameLoopPlayTests`, `BreakPlayTests`, `Room1LevelsPlayTests` and `CatPlayTests` to call `Begin`/`LoadLevel` and set `InputEnabled` where they relied on `Start`.

**Interfaces:**
- Consumes: `Profile` (Task 1); `AppConfig` / `UiStrings` (Task 2); `IAdService` / `IPurchaseService` (Task 3).
- Produces:
  - `public enum AppScreen { Splash, Consent, Home, Playing, Paused }`
  - `public sealed class AppFlow`:
    - `AppFlow(bool firstRun)`, `AppScreen Screen { get; }`, `event Action<AppScreen> Changed`
    - `bool SplashDone()`: goes to Consent on the first run, otherwise Home.
    - `bool ConsentDone()`, `bool Play()`, `bool Pause()`, `bool Resume()`
    - Illegal moves return false and change nothing.
  - `AppController : MonoBehaviour`:
    - Owns `Profile` (`PlayerPrefsSaveStore`), `AppFlow`, `AdPacing`.
    - Casts its serialized `MonoBehaviour adService` / `purchaseService` to the interfaces in `Awake`, and logs an error naming the field if a cast fails.
    - Behaviour per screen:
      - Splash: waits `splashSeconds`.
      - Consent: in M3 completes at once and marks the first run done (M4 plugs MAX's consent flow in here).
      - Home: loads the saved level behind the home panel with input off.
      - Play: hides home and enables input.
      - Pause: `Time.timeScale = 0`. Resume restores 1.
    - `OnApplicationPause(true)` while Playing → Pause.
    - Settings toggles call `Profile` and apply to `SfxPlayer.Muted`, `MusicPlayer.Muted` and `HapticsBridge.Enabled`. They're applied at start too.
    - Restart: Resume + `Retry()`.
    - Restore: calls `RestoreRemoveAds`, sets the flag if owned, and shows `restored` / `nothingToRestore`.
    - The privacy button opens `privacyPolicyUrl` and is hidden when that is empty.
  - `HomeScreen.Show(int levelNumber, bool showRemoveAds, string price, Action onPlay, Action onRemoveAds, Action onSettings)`. A successful purchase hides Remove Ads and sets the flag.
  - `DebugMetaButtons` lives on the existing triple-tap debug panel: "Reset save" (`Profile.ResetAll` + reload Home) and "Ads offline" (toggles `SimulatedAdService.Offline`).

- [ ] **Step 1: Write the failing EditMode tests:**
  - `FirstRun_GoesSplashConsentHome`
  - `LaterRun_SkipsConsent`
  - `Pause_OnlyFromPlaying`
  - `Resume_ReturnsToPlaying`
  - `Play_FromSplash_IsRejected`
  - `Changed_FiresOncePerMove`
- [ ] **Step 2: Write the failing PlayMode tests** (load `Game`, clear `smash.save` in SetUp and TearDown):
  - `Launch_ShowsHomeWithSavedLevel`: save `levelIndex = 3` → the home label reads `Play · Level 4`, and the desk shows R1_L04.
  - `Play_EnablesInput`
  - `Backgrounding_PausesPlay`: calling `OnApplicationPause(true)` via SendMessage gives Paused with `timeScale == 0`.
  - `SoundToggle_MutesSfxAndPersists`
  - `Begin_WithSavedLevelBeyondList_PlaysWrappedLayout`: save index 26 → the HUD reads "Level 27" and the layout is `At(26)`.
  - `RemoveAds_Purchase_HidesButtonAndSetsFlag`
- [ ] **Step 3: Run both suites.** Expected: FAIL.
- [ ] **Step 4: Implement**, rebuild the scene and fix the updated tests.
- [ ] **Step 5: Run** `batch-tests.sh editmode` and `playmode`. Expected: all pass.
- [ ] **Step 6: Snapshot** the home and pause screens (`tools/unity/snapshot.sh` gets `--screen home|pause` in this task) and check the layout at 1080×1920.
- [ ] **Step 7: Commit** `feat: app flow — splash, home, pause and settings, saved progress`.

### Task 5: Win and fail panels — reveal bar, rewarded continue, interstitial

**Files:**
- Create: `Runtime/Levels/UnlockReveal.cs`
- Modify:
  - `Runtime/UI/ResultPanel.cs`: reveal bar, buttons lock after the first tap.
  - `Runtime/AppController.cs`: handles `RoundFinished`.
  - `Editor/UiSceneParts.cs`: the bar.
- Test: `Assets/Tests/EditMode/UnlockRevealTests.cs`, `Assets/Tests/PlayMode/ResultFlowPlayTests.cs`

**Interfaces:**
- Consumes: `GameBootstrap.RoundFinished` (Task 4), `AdPacing` (Task 3), `Profile.RecordWin` (Task 1), `UiStrings.PropName` (Task 2).
- Produces:
  - `public readonly struct Reveal { public readonly bool HasNext; public readonly PropKind Next; public readonly float Progress01; }`
  - `public static class UnlockReveal { public static Reveal After(LevelList list, int wonIndex); }`
    - Works on `w = wonIndex mod Count`.
    - A level *introduces* a kind if none of the levels before it in the list has that kind.
    - `n` = the first level after `w` that introduces a kind (`HasNext = false` if there is none).
    - `p` = the last level ≤ `w` that introduces a kind.
    - `Progress01 = (w − p + 1) / (n − p)`.
    - `Next` = the first new kind at `n`, in `PropKind` order.
  - `ResultPanel.ShowWin(Reveal reveal, string nextName, Action onNext)`, `ShowLose(bool offerContinue, Action onContinue, Action onRetry)`.
  - AppController behaviour:
    - **Win:** `RecordWin(index)` and `AdPacing.LevelWon()` at once, then show the panel.
    - **Next:** if `ShouldShowInterstitial(index+1, now, removeAds)` and `InterstitialReady`, then `ShowInterstitial` → `InterstitialShown` → load next. Otherwise load next at once.
    - **Lose:** `offerContinue = Budget.CanContinue && RewardedReady`. "+3 shots" → `ShowRewarded` → `Continue()` only if `true`.

- [ ] **Step 1: Write the failing EditMode tests** (lists built in code, intros at index 0, 2, 4, 11):
  - `After_LevelOne_PointsToFirstNewKind`: w = 0 → Next is the kind introduced at 2, progress 1/2.
  - `After_IntroLevel_StartsNewSpan`: w = 4 → progress 1/7.
  - `After_LastLevelBeforeIntro_IsFull`: w = 10 → 1.0.
  - `After_NoLaterIntro_HasNoNext`: w = 11 → false.
  - `After_WrappedIndex_UsesLayoutPosition`: w = Count + 4 equals w = 4.
- [ ] **Step 2: Write the failing PlayMode tests:**
  - `Win_SavesProgressBeforeNext`
  - `Next_OnLevel6After90s_ShowsInterstitialThenLoadsNext`: drive time with a pacing clock injected through `AppController.SetClockForTest(Func<float>)`.
  - `Next_WithInterstitialNotReady_LoadsNextLevelImmediately`: Offline.
  - `Next_WithRemoveAds_NeverShowsInterstitial`
  - `Lose_Offline_HidesContinue`
  - `Lose_RewardedClosedEarly_StaysLost`
  - `ResultPanel_SecondTap_IsIgnored`: two Next clicks load only one level.
  - `Retry_NeverShowsInterstitial`
- [ ] **Step 3: Run both suites.** Expected: FAIL.
- [ ] **Step 4: Implement.**
- [ ] **Step 5: Run** `batch-tests.sh editmode` and `playmode`. Expected: all pass.
- [ ] **Step 6: Commit** `feat: win reveal bar, rewarded +3 shots and paced interstitials`.

### Task 6: Two desks

**Files:**
- Modify:
  - `Runtime/Levels/LevelDef.cs`: `public bool hasBackDesk`, `public Vector2 backDeskSize = new Vector2(1.2f, 0.5f)`, `public const int MaxProps = 40`; `Validate` checks each prop against its own desk and the prop cap.
  - `Runtime/Levels/PropPlacement.cs`: `public int desk;` (0 = main, 1 = back). Append only.
  - `Runtime/World/LevelSpawner.cs`: `public static readonly Vector3 BackDeskTop = new Vector3(0f, 1.3f, 1.05f)` (starting values); `DeskTop(int desk)`; spawns relative to it; `[SerializeField] GameObject backDesk` is shown only when `level.hasBackDesk`.
  - `Editor/PrototypeSceneBuilder.cs`: `WriteBackDeskPrefab()` (collider top + a KayKit `table_small` visual scaled so its top sits at `BackDeskTop.y`; no `FloorSurface`).
  - `Editor/LevelEditing.cs`: assigns `desk = 1` to props whose base rests on the back desk's footprint and height.
  - `tools/assets/pack-manifest.txt`, `docs/asset-licenses.md`: add `table_small`.
- Test: `LevelTests` (EditMode), `Assets/Tests/PlayMode/TwoDeskPlayTests.cs`

**Interfaces:**
- Produces: `LevelSpawner.DeskTop(int desk) -> Vector3`, `LevelDef.hasBackDesk`, `PropPlacement.desk`.

- [ ] **Step 1: Write the failing tests:**
  - `Validate_BackDeskProp_WithoutBackDesk_Fails`
  - `Validate_BackDeskProp_UsesBackDeskBounds`: x = 0.7 on a 1.2-wide back desk fails.
  - `Validate_MoreThanMaxProps_Fails`
  - PlayMode `BackDeskProps_SpawnOnTheBackDesk_AndStayPut`: after 2 s, none moved more than 1 cm.
  - PlayMode `BackDesk_HiddenWhenLevelHasNone`
  - EditMode `BackDesk_IsInsideCameraViewAtPhoneAspects`: the back desk's top corners project inside the Game camera viewport at 1080×1920 and 1080×2340.
  - PlayMode `PropFallingBetweenDesks_IsCleared`
- [ ] **Step 2: Run.** Expected: FAIL.
- [ ] **Step 3: Implement.** Adjust `BackDeskTop` only if the camera test or the snapshot says so; record the final values in the commit message.
- [ ] **Step 4: Run** both suites. Expected: all pass. Take a snapshot of a scratch two-desk level.
- [ ] **Step 5: Commit** `feat: levels can use a second, raised back desk`.

### Task 7: New object types — paper cup and desk lamp

**Files:**
- Modify:
  - `Runtime/Levels/PropKind.cs`: append `PaperCup`, `DeskLamp`.
  - `Runtime/Levels/PrototypeLevels.cs` (only if `Size`/`Mass` must know every kind).
  - `Editor/PropPrefabBuilder.cs`: two specs.
  - `Editor/SoundSetBuilder.cs`, `Editor/VfxBuilder.cs`: a feel entry and VFX per kind.
  - `Editor/MetaAssetBuilder.cs`: names "Paper Cup" and "Desk Lamp".
  - `tools/assets/pack-manifest.txt`: Tiny Treats Bakery `coffee_cup_takeaway.fbx` → `TinyTreats/`, KayKit `lamp_table.fbx` → `KayKitFurniture/`.
  - `tools/assets/audio-manifest.txt`: the chosen sounds.
  - `docs/asset-licenses.md`
- Test: `CatalogTests`, `PropFeelLibraryTests`, `ArtAssetsTests`, `BreakPlayTests`

**Interfaces:**
- Produces: catalog specs for both kinds. Each has its own `PropFeel` (sound set + VFX).

Starting values (Claude's call, tune on device):

| Kind | Mesh | Size | Mass | Physics (dyn/stat/bounce) | Collider | Break |
|---|---|---|---|---|---|---|
| `PaperCup` | `coffee_cup_takeaway` | about 1.2× a mug's height | 0.03 | 0.4 / 0.4 / 0.3 | ConvexMesh | Pop: a light paper "pok" and a small puff |
| `DeskLamp` | `lamp_table` | about 0.45 m tall | 0.6 | 0.5 / 0.6 / 0.05 | ConvexMesh | Pop: a metal clank and a spark puff |

- **Sounds:** pick from the downloaded CC0 packs already in `download-assets/audio/`. Cup: the owlishmedia clicks or wubitog pops; lamp: the rubberduck breaking pack's metal or rock hits.
- **Rule:** each kind's hit and break sounds must differ from every other kind's (spec §9).

- [ ] **Step 1: Write the failing tests:**
  - `Catalog_HasSpecForEveryKind` (existing test, now covers 9 kinds)
  - `PaperCup_IsLighterThanEveryKeycap`
  - PlayMode `DeskLamp_HitHigh_Topples`: a level-height shot at the lampshade tips the lamp past 45° within 2 s.
  - `EveryKind_HasItsOwnSounds`
  - `PackProp_ImportsUpright` for both
  - `LicenseLogTests` (existing)
- [ ] **Step 2: Run.** Expected: FAIL.
- [ ] **Step 3: Implement:**
  - Run `tools/assets/import-packs.sh`.
  - Run `tools/unity/run-method.sh Game.Editor.PrototypeSceneBuilder.Rebuild`.
- [ ] **Step 4: Run** both suites. Expected: all pass. Snapshot both props on the desk beside a mug.
- [ ] **Step 5: Commit** `feat: paper cup and desk lamp props`.

### Task 8: Playtest bot

**Files:**
- Create:
  - `Runtime/Levels/ClearRateBand.cs`, `Runtime/Levels/LevelFingerprint.cs`
  - `Assets/Tests/PlayMode/PlaytestBot.cs`
  - `Assets/Tests/EditMode/PlaytestReportTests.cs`
  - `tools/unity/playtest-bot.sh`
  - `docs/playtest/room1-report.json` and `docs/playtest/room1-report.md` (both written by the bot)
- Modify:
  - `Runtime/Core/SlingshotMath.cs`: `PullFor`.
  - `Room1LevelsPlayTests.cs`: use `PullFor`; loop over `list.Count`.
- Test: `SlingshotMathTests`, `ClearRateBandTests`, `LevelFingerprintTests` (EditMode)

**Interfaces:**
- Produces:
  - `public static Vector2 SlingshotMath.PullFor(float yawRadians, float power01, ShotTuning t)`: the inverse of `Compute`'s aim. Direction `(-sin yaw, -cos yaw)`, length `lerp(minPull, maxPull, power01)`, yaw clamped to `±maxYawDeg`.
  - `public readonly struct ClearRateBand { public readonly float Min, Max; public bool Accepts(float rate); public static ClearRateBand For(Difficulty d, int levelNumber); }`
    - Teaching (level ≤ 10 and Normal): [0.60, 1.00]
    - Normal: [0.35, 0.95]
    - Hard: [0.15, 0.65]
    - These are Claude's call, recalibrated with soft-launch data (spec §6.3).
  - `public static string LevelFingerprint.Of(LevelDef level)`:
    - A 64-bit FNV-1a over invariant-culture text.
    - Covers shots, difficulty, `hasBackDesk`, `backDeskSize`, and each prop's kind, desk, target flag, yaw (0.1°) and position (mm). `variant` is excluded, since it is only the look.
  - Bot policy (inside `PlaytestBot`, seeded by the level index):
    - Before each shot, wait until `bootstrap.IsDeskQuiet` and the reload time has passed (3 s cap).
    - Pick a random remaining target, aim at it (yaw = atan2(dx, dz) from the pouch, plus N(0, 3°) noise), and draw power from U(0.35, 1).
    - 30 attempts per level, `timeScale` 20.
    - An attempt is a win if the round is Won within the level's shots (no continue).
  - Report rows: `{ id, fingerprint, attempts, wins, clearRate, medianShotsOnWin, band }`.
  - `playtest-bot.sh` runs the batch PlayMode runner with `-testCategory Bot`. `PlaytestBot` is `[Explicit, Category("Bot")]`, so the normal suite skips it.

- [ ] **Step 1: Write the failing EditMode tests:**
  - `PullFor_RoundTripsThroughCompute`: yaw 10°, power 0.5 → `Compute` gives a velocity whose yaw is 10° ± 0.1.
  - `PullFor_ClampsYaw`
  - `Band_TeachingNormalHard_Values`
  - `Band_Accepts_IsInclusive`
  - `Fingerprint_SameLayout_SameHash`
  - `Fingerprint_PropMovedOneMillimetre_Changes`
  - `Fingerprint_VariantOnly_Unchanged`
  - `PlaytestReport_CoversEveryRoom1LevelWithCurrentLayout`: every level in `Room1LevelList` has a report row whose fingerprint matches and whose rate is in its band; the failure message lists the offenders.
- [ ] **Step 2: Run.** Expected: FAIL (the report doesn't exist yet).
- [ ] **Step 3: Implement**, then run `tools/unity/playtest-bot.sh` on the current 5 levels.
- [ ] **Step 4: Run** `batch-tests.sh editmode`. Expected: all pass, *or* the report test names levels outside their band. Those are fixed in Task 9; record the L1–L5 results in the ledger.
- [ ] **Step 5: Commit** `feat: playtest bot with clear-rate bands and level fingerprints`.

### Task 9: Room 1 levels 6–25

**Files:**
- Modify:
  - `Editor/Room1LevelBuilder.cs`: keeps only the list, `Write` and asset helpers.
  - `Assets/Tests/EditMode/LevelTests.cs`: 25 levels, Hard on 5/10/15/20/25, shots 5–10.
  - `SceneBudgetTests`: every Room 1 level, not just level 1.
- Create (partial `static class Room1Layouts`):
  - `Editor/Room1Layouts.cs`: L1–L10, with L1–L5 moved verbatim.
  - `Editor/Room1LayoutsLater.cs`: L11–L25.
  - `Editor/LayoutKit.cs`: the `Cap`/`Prop` helpers, plus `Row`, `Tower`, `Bridge`.
- Regenerate: `Assets/_Game/Levels/Room1/R1_L06..R1_L25.asset`, `docs/playtest/room1-report.*`.

The level table (ideas, kinds and shots are decided; exact positions are the builder's, proven by validation and the bot):

| # | Idea | Kinds | Shots | Diff | Back desk |
|---|---|---|---|---|---|
| 1–5 | unchanged from M2 | | 5/5/6/6/7 | L5 Hard | |
| 6 | Two desks: keycaps on the back desk, a few on the main desk | keycaps | 6 | N | yes |
| 7 | Dominoes: a spaced row of standing keycaps that falls in a chain | keycaps | 5 | N | |
| 8 | Lob: a low front book wall shields mugs behind it | books, mugs | 6 | N | |
| 9 | Wide spread: spacebar and Enter keys across the full width | keycaps | 7 | N | |
| 10 | Mixed, two desks | mugs, books, keycaps | 8 | Hard | yes |
| 11 | Pyramid of 2u keycaps | keycaps | 6 | N | |
| 12 | **New: paper cup**, a stacked cup tower | paper cups | 6 | N | |
| 13 | Spacebar bridges between keycap towers, cups on top | keycaps, cups | 7 | N | |
| 14 | Book dominoes on the back desk | books | 7 | N | yes |
| 15 | A tall mixed tower on a cactus base | cactus, books, keycaps, cups | 8 | Hard | |
| 16 | Café row: mugs and cups in a line | mugs, cups | 7 | N | |
| 17 | Spacebar bridge across two book stacks, cups on it | books, keycaps, cups | 7 | N | |
| 18 | Back-desk keycap wall, cups on the main desk | keycaps, cups | 8 | N | yes |
| 19 | Fortress: keycap walls around a cactus | keycaps, cactus | 8 | N | |
| 20 | Everything so far, two desks | all of the above | 9 | Hard | yes |
| 21 | **New: desk lamp**, a lamp with keycaps around it | lamp, keycaps | 6 | N | |
| 22 | A lamp standing on books (top-heavy) | lamp, books | 7 | N | |
| 23 | Lamps in front shield mugs behind | lamps, mugs | 8 | N | |
| 24 | A lamp and cups across two desks | lamp, cups, keycaps | 8 | N | yes |
| 25 | The finale: a big mixed pile, two desks | everything | 10 | Hard | yes |

- [ ] **Step 1: Update the failing tests:**
  - `Room1_Has25Levels_WithHardEveryFifth`
  - `Room1_ShotsWithinFiveToTen`
  - `Room1_NewKindsArriveAtLevels12And21`: uses `UnlockReveal`'s intro rule. Intros at 1 (keycaps), 3 (mug), 4 (book), 5 (cactus), 12 (paper cup), 21 (lamp).
  - `SceneBudget_EveryRoom1Level` (≤ 100k tris, ≤ 40 props)
  - The existing `Room1Levels_EveryPropIsATarget` and `PlaytestReport_CoversEveryRoom1LevelWithCurrentLayout`
- [ ] **Step 2: Run.** Expected: FAIL (5 levels).
- [ ] **Step 3: Build the layouts** and run `Room1LevelBuilder.BuildAll` via `run-method.sh`.
- [ ] **Step 4: Run** `tools/unity/playtest-bot.sh`. Re-shape any level outside its band, and re-run until all 25 pass. Note each re-shape in the ledger.
- [ ] **Step 5: Run** `batch-tests.sh editmode --coverage` and `playmode`. Expected: all pass. Coverage of Core, Levels and Meta ≥ 80%.
- [ ] **Step 6: Snapshot** levels 6, 12, 20, 21 and 25.
- [ ] **Step 7: Commit** `feat: Room 1 levels 6–25, bot-verified`.

### Task 10: Device build and checkpoint 3 hand-off

**Files:** none new (`CLAUDE.md`'s "Goal right now" is updated to M3/checkpoint 3).

- [ ] **Step 1:** Run `tools/unity/batch-tests.sh editmode --coverage`, `playmode` and `tools/blender/build.sh --tests`. Expected: all green.
- [ ] **Step 2:** Run `tools/unity/build.sh ios`. Expected: `iOS Succeeded … 0 errors`.
- [ ] **Step 3:** Run the final whole-branch review (executing-plans' Final Review), then the fix pass.
- [ ] **Step 4:** Write the owner's checkpoint 3 checklist in the final message:
  - Play L1–L25.
  - Home, pause and settings toggles, which survive a relaunch.
  - Backgrounding pauses play.
  - From level 6, a test ad after Next at most once per 2 levels and 90 s.
  - "+3 shots" with an early close versus a full watch.
  - Simulated Remove Ads, then debug Reset save, then Restore.
  - Debug "Ads offline".
  - Also a reminder from spec §3: register the store accounts. The Google Play closed test (12 testers, 14 days) should start by checkpoint 3, but Android is deferred by the owner, so they decide when.
- [ ] **Step 5:** Commit `docs: M3 status`. Then use superpowers:finishing-a-development-branch.

## Out of scope (M4 and later)

- AppLovin MAX, Unity IAP and the App Store Connect product.
- The UMP consent and ATT flows (plugged into `AppFlow`'s Consent step).
- Firebase Analytics and Crashlytics, and the privacy policy URL and page.
- Android builds, store art and the final name.
- The final cat (checkpoint 4), and Rooms 2–4.
