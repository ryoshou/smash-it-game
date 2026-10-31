# Smash It (working title): Cat Desk Smash Design

**Date:** 2026-10-26
**Status:** Approved by owner (2026-10-30)

Throughout this doc, **[Owner]** means the owner decided it in conversation. **[Claude's call]** means the owner handed the decision over or never discussed it, so the builder may revisit it if there's a good reason.

---

## 1. Intent

A relaxing, fun, easy-to-play mobile physics game. A cat with a slingshot knocks everything off a desk.

It's inspired by *Royal Smash! – Physics Puzzle* (Cypher Games, Unity, 12M+ installs). That game is a cannon you fire by tapping, and you knock royal-themed structures off a pedestal table. We keep its core rule, "clear everything off the table," and its satisfying destruction. We change two things:

- **How you shoot [Owner]:** a drag-and-release slingshot with a natural gravity arc. Each shot is aimed and deliberate, not rapid-fire.
- **The world [Owner]:** a cat knocking desk clutter and keyboard keycaps off desks. Cats knocking things off tables is a well-known behavior, and it maps exactly onto the win condition.

**Why the owner wants it:** they found the original relaxing, fun and easy to play [Owner].

**Success [Owner]:** a game live on the App Store and Google Play that earns money.

## 2. Goals, non-goals, anti-goals

**Goals**
- A brand-new player understands what to do within about 2 seconds, and can play one-handed in portrait.
- Every hit feels great. Destruction is the reward.
- It feels relaxing: no timers and no energy or lives gate. Failing is cheap and friendly.
- Launch with **100 levels** [Claude's call, owner agreed to ask for a recommendation], then add about 20–30 levels per update.
- It earns money through ads plus a remove-ads purchase, without feeling pushy.

**Non-goals for launch** (candidates for after launch, if soft-launch numbers justify them) [Owner agreed]
- Coins and a currency economy, boosters, a shop beyond remove-ads.
- The Journey/map meta, limited-time events, leaderboards, daily rewards.
- Lives or energy.
- Cloud save, accounts, multiplayer.

**Anti-goals** (it fails if these are true, even when everything technically works)
- It feels pushy, the way the original's reviewers complain about: pop-up offers after every level, frustrating "super hard" walls, or pressure to pay to continue.
- Aiming feels random. Shots must be readable and repeatable: same pull, same flight.
- Destruction feels floaty or weak.
- It is recognisably a copy of the original's art, audio, levels or code (see §3).

## 3. Constraints

- **Clean-room rule (hard).** Nothing gets copied from the original: no art, models, textures, audio, level layouts, text, or code. We studied its *design* from public store pages, two gameplay videos, and a list of type names in its IL2CPP metadata (findings in Appendix A). We did not extract any of its assets. The original APK in this folder came from a mirror site (apkcube) and is untrusted: never install it, never run it, never commit it.
- **Engine [Claude's call]:** Unity 6 LTS (the original uses 6000.0.x) with URP and 3D physics (PhysX). The owner's previous project (`~/games/ant-game`) used Unity with EditMode tests and iOS builds, so reuse familiar tooling and conventions where they fit.
- **Platforms [Owner]:** iOS and Android phones, portrait only. Tablets should work, but they're letterboxed and not specially laid out.
- **Performance bar [Claude's call]:** 60 fps target on a mid-range Android phone from around 2021, never under 30 fps, even in the busiest destruction moment. A physics quality setting may scale down debris on weak devices, as the original does.
- **Offline [Claude's call]:** fully playable offline. Ads just don't show when there's no network.
- **Art and audio [Owner]:** art comes from the owner's downloaded CC0 packs, plus pieces we build to match: scripted in Blender, or AI-generated and cleaned up (§12). The owner has budget to buy or commission assets where that falls short. **So every visual and audio element must be swappable without code changes**, through prefabs, materials and ScriptableObject data, never hard-wired.
- **Store compliance [Claude's call]:** the game targets a general audience and is **not** enrolled in Google Play "Designed for Families" or the iOS Kids category. Those categories heavily restrict ads. It still needs: Apple's ATT prompt, a GDPR/UMP consent flow (through the ad mediation SDK's built-in consent flow), a privacy policy URL, and Data Safety and App Privacy labels that match the SDKs actually shipped.
- **No target date [Owner].** It launches when it meets the launch bar in §9.
- **Store accounts [Owner]:** none registered yet. The owner will register *personal* Apple Developer and Google Play Console accounts. Plan for this lead time: register early, because a new personal Play account must run a **closed test with ≥12 testers for 14 consecutive days** before it can apply for production access. Schedule that closed test to run in parallel with content production, starting no later than checkpoint 3 (§10a).
- **Art style [Owner]:** cute low-poly 3D (chunky, soft, bright, similar in spirit to the original but our own). The base is the owner's downloaded CC0 packs, and missing pieces are built to match. See §12.

## 4. The core loop (decided)

### 4.1 Screen and camera
- The camera is fixed, angled slightly down, looking at one desk (some levels have two desks). It never moves during a level.
- The cat and slingshot sit at the bottom center. The desk with its pile of objects sits in the upper-middle of the screen.
- **HUD:** shots remaining (top-left), a settings/pause button (top-right), and the level number. Nothing else during play.

### 4.2 Shooting [Owner]
- You touch near the slingshot pouch and drag down/back. Moving left or right aims, and pull distance sets power: further means faster and higher. Release fires.
- **Flight:** the ball follows a pure gravity arc with no sideways curve. During brainstorming the owner tried a throwaway browser feel-test (not Unity code; nothing to reuse) and chose "gravity arc" over "arc + hook." No Unity code exists yet, so the build starts from an empty project.
- **Aim preview [Claude's call]:** a short dotted line shows the first part of the path while you pull. It doesn't show the landing point, so aiming stays a skill. The builder tunes the preview length so early levels feel easy.
- **Cancel:** releasing with almost no pull, or dragging back into the pouch, cancels the shot without spending it.
- **Ammo [Claude's call]:** a ball of yarn. Visual only.
- **Consistency is required:** the same pull gives the same launch velocity every time. No random spread.

### 4.3 Win, fail, settle
- **Win:** every *target object* has left the desk surface. An object counts as cleared once it falls below the desk top and touches the floor, or leaves the play volume. Objects that hit the floor break or burst and fade away, as in the original. Some levels may have fixed or decorative objects (like a monitor bolted down) that don't count.
- **Settle-then-judge:** once the last shot is fired, the game waits for physics to settle before declaring a loss. Settled means objects below speed thresholds for a hold time, with a maximum wait. A slow topple that clears the desk is a win.
- **Out of shots:** a friendly fail panel offers (a) watch an ad for +3 shots, once per attempt, or (b) retry. No lives are lost and nothing is gated.

### 4.4 Shots per level [Claude's call]
- About 5–10 per level, set per level during tuning (see §6). This is far fewer than the original's 20–30, because each shot is a deliberate pull.

### 4.5 Win screen [Claude's call]
- The cat celebrates (smug), with confetti, "Desk cleared!", and Next.
- There's a progress bar toward the next new object type ("New thing to knock over: Coffee Mug!"), mirroring the original's "new item unlocked" beat. It's purely a reveal; no currency.

## 5. Feel ("the juice")

This is the most important quality area. Patterns observed in the original, through gameplay plus type names like `BallImpactSpeedMultiplier`, `ApplyFirstContactExplosion`, `ObstacleImpactMaxLinearSpeed`, `SettleLinearThreshold` and `BrokenPieceFader`:

- **Impact boost:** on contact the ball transfers *more* force than real physics would, so hits feel strong.
- **First-contact burst:** a small radial impulse where the ball first touches the pile, so a single good shot starts a chain reaction.
- **Speed caps:** cap how fast objects can fly, so nothing launches to the moon or jitters.
- **Material personalities:** keycaps scatter and bounce, mugs shatter into pieces, books thud and slide, plant pots are heavy and spill. Each type has its own sound, break effect and mass.
- **Feedback:** hit sounds that change with impact speed, a light screen shake on big impacts, phone vibration (toggleable), a pull-tension sound and slingshot "twang," and cat reactions (annoyed on a miss, proud on a clear).
- **Debris:** broken pieces fade out, and debris is pooled. Nothing builds up into frame drops.

The builder chooses all the specific numbers, but they must live in tunable data assets, not constants buried in code.

## 6. Levels

### 6.1 Launch content [Claude's call]
- **100 levels in 4 rooms of 25:** Home Office → Café → Library → Gaming Den. Each room has its own backdrop, desk style and object mix.
- **Teaching (levels 1–10):** one idea per level. Pull to shoot, power, aiming, toppling tall stacks, heavy objects, breaking objects, two desks, and so on.
- **New object type about every 10 levels** (drives the win-screen reveal bar).
- **About every 5th level is "Hard"** and labelled as such. There's no "super hard" tier at launch; that tier drove frustration in the original's reviews.

### 6.2 Object roster (starting point; builder may adjust) [Claude's call]
Keycaps in several sizes (the signature stacking block), keyboard chunks, books, sticky-note pads, pencil cups, coffee mugs (shatter), soda cans, rubber duck, houseplant pot (heavy), desk lamp, and later at least one "special": a fizzy soda that bursts when knocked over, standing in for the original's TNT chain reactions.

### 6.3 Authoring and automated testing [Claude's call]
- **An in-editor level tool** to place, snap and stack objects, set shots and difficulty, and preview.
- Levels are stored as data (ScriptableObject or JSON), separate from scenes, so updates can add levels easily.
- **An automated playtest bot** fires many sampled shots at each level and reports how often it gets cleared within the shot limit. Every level must pass a clear-rate band for its difficulty before shipping. This catches impossible and trivial levels before a human plays them. The builder sets the bands, then recalibrates them with soft-launch data.

## 7. Screens and flow [Claude's call]
- **Launch:** splash → consent/ATT (first run only) → home.
- **Home:** the cat on a cozy backdrop, a big Play button showing the current level, a settings button, and remove-ads (until purchased).
- **Gameplay:** see §4.
- **Pause/settings:** sound, music, haptics, restart level, restore purchases, privacy policy.
- **Win and fail panels:** see §4.3 and §4.5.
- No level-select map at launch. Players always continue from their highest level, with restart available.

## 8. Money [Owner agreed to this shape; specifics are Claude's call]
- **Ad mediation:** AppLovin MAX (the original uses it too; it's the industry default for this genre).
- **Interstitial ads:** only after a *win*, never after a fail or retry. None during the first ~5 levels. Frequency-capped by both level count and time, roughly no more than one per 2 levels and per ~90 seconds. Exact caps get tuned in soft launch.
- **Rewarded ad:** "+3 shots" on the fail panel, once per attempt. Always optional.
- **Remove Ads purchase:** one non-consumable that removes interstitials and keeps the optional rewarded ads. Price around $3.99–$4.99; the original charges $7.99, but ours has no other purchases yet. Restore Purchases is required on iOS.
- **No other offers, pop-ups or bundles at launch.**

## 9. Launch bar and soft launch [Claude's call, from the owner's "build it perfectly"]
"Perfect" means meeting this bar:
- No known crashes; crash-free sessions ≥ 99.5% in testing.
- Meets the §3 performance bar on a mid-range Android device and a recent iPhone.
- All 100 levels pass the bot's clear-rate bands, plus a human playthrough.
- Every object type has its own sound, break behavior and effect, with no placeholder art or audio left.
- Ads, consent, the purchase and restore all work on both stores' test environments.
- Analytics events fire correctly: level start, win, fail, shots used, continue used, ad shown, purchase, and retention.

Then **soft launch** in 1–2 smaller markets for a few weeks. Watch day-1 and day-7 retention, where each level makes players quit, and ad revenue per user. Tune the levels and ad frequency, then launch worldwide. The owner hasn't set target numbers; the builder proposes benchmarks for the casual-physics genre.

## 10. Other decisions [Claude's call]
- **Analytics:** Firebase Analytics + Crashlytics (free, standard).
- **Save:** local only: highest level, settings, remove-ads flag (with store restore as backup).
- **Localization:** English at launch, with every string externalized so translations can be added later.
- **Name and icon:** "Smash It" is a working title. The final name, icon and store art are decided before soft launch.

## 10a. Owner checkpoints [Claude's call]
The owner judges feel and art. The builder decides everything else. The owner plays a device build at these points, and work past each one waits for their OK:
1. **Shooting prototype:** slingshot, arc, one desk of keycaps, placeholder art. This confirms the shot feel.
2. **Vertical slice:** Room 1's look, the cat, 3–4 object types with full juice, and 5 levels. This confirms the art direction and audio, including the refined scripted props. It's also where the owner decides whether to keep the generated cat or commission one (§12.3).
3. **Room 1 complete:** 25 levels, all screens, ads and the purchase in test mode.
4. **Content complete:** 100 levels, ready for soft launch.

The builder (with the bot) does the full 100-level playthrough. The owner may spot-check.

## 11. Left to the builder
- Code architecture, package choices (beyond Unity/URP/MAX/Firebase above), and project structure.
- All tuning numbers: physics, shot counts, preview length, settle thresholds, ad caps.
- The exact object roster and intro order within the cadence in §6.
- Which AI 3D and audio generators to use, within §12 and the swappable-asset constraint.
- How the playtest bot samples shots and what clear-rate bands to use.
- Test strategy. The owner's global rules ask for TDD on logic and 80% coverage of testable code; physics feel is validated by playtesting.

## 12. Art and audio pipeline

### 12.1 Base library [Owner]
- The owner rejected the Asset Store candidates and downloaded free packs instead, into `download-assets/`.
  - KayKit by Kay Lousberg: Furniture Bits, Restaurant Bits, Board Game Bits, Halloween Bits.
  - Tiny Treats by Isa Lousberg: Baked Goods, Bakery Interior, Charming Kitchen, Homely House, Pleasant Picnic, Pretty Park.
  - About 1,100 models in obj / fbx / gltf.
- **License:** all are CC0. Commercial use and modification are allowed, and no credit is required.
  - [Claude's call] Credit the authors in the settings screen anyway, as a courtesy.
- **[Claude's call] Import only the models actually used** into the Unity project. Don't commit the whole download folder.

### 12.2 The visual language (every new asset must follow it)
Measured from the pack files:
- **Shapes:** chunky primitives with soft beveled edges. Smooth shading, no visible facets on rounded parts.
- **Color:** one shared 1024×1024 atlas per pack, made of vertical light-to-dark gradient cells. The Tiny Treats atlas is an 8×4 grid.
  - Every face's UVs sit in a tiny island inside one cell, spread across that cell's gradient by height. Tops come out lighter and bottoms darker.
  - No painted textures, no detail maps.
- **Material:** glTF metallic 0, roughness 0.5, one material per prop.
- **Size and budget:** a pack mug is about 340 triangles and 0.35 units tall on a 2.0 × 1.5 × 1.0 table.
  - [Claude's call] Budgets: a prop ≤ 1,500 tris, a keyboard ≤ 2,500, the cat ≤ 8,000, the whole visible scene ≤ 100k.
- **New pieces use the Tiny Treats atlas** (pastel, the closest to "cute"). KayKit props can sit beside them; the spike showed the two mix well.

### 12.3 Missing pieces and how each gets made [Owner agreed to this split]
- **Hard-surface props: built by script in Blender** (Blender 5.2 LTS, installed by the owner), using the method above.
  - Keycaps in several sizes (1u, 2u, spacebar, enter) and the keyboard.
  - Monitor, laptop, mouse, pencils, pencil cup, sticky-note pad, stapler.
  - Headphones, game controller, office desk, the slingshot, the yarn ball, soda-can variants, the fizzy-soda special.
  - Each prop is a small script, so sizes, colors and variants are cheap to change.
- **Spike result [Owner]:** a throwaway test built a monitor, a keyboard and a keycap stack, rendered beside pack props. Verdict: "not perfect but acceptable; we will refine them later."
  - Known refinements: legends on the keys, something on the monitor screen, and a proportion pass.
  - The spike's scripts were throwaway. The real generators get rewritten inside the project.
- **Organic shapes: AI image-to-3D, then cleanup.** This covers the cat, the rubber duck, and figurines or plushies for the Gaming Den.
  - The builder chooses the tool (for example Meshy, Tripo or Rodin) and feeds it pack renders as style references.
  - Cleanup is required: reduce to the triangle budget, re-map UVs onto the atlas cells, then check it side by side with pack props.
  - Check the chosen tool's commercial-use terms for the plan used, and record them.
- **The cat (highest risk).** It needs idle, pull/aim (holding the slingshot), release, celebrate (smug) and annoyed animations.
  - [Claude's call] Design it as a sitting, chunky cat whose acting is mostly upper body, head and tail, so rigging stays simple.
  - The builder chooses the rigging route: an AI auto-rig, a hand rig in Blender, or simple squash-and-stretch.
  - **Fallback:** if the generated cat isn't cute enough at checkpoint 2, the owner commissions just the cat from an artist (the owner has said there's budget for this).
- **Breakable versions:** mugs, pots, cans and the like are pre-fractured in Blender (Cell Fracture or scripted cuts).
  - [Claude's call] 6–12 pieces per object, sharing the atlas. Broken faces use a lighter cell of the object's own color.
- **Room backdrops:** built from pack walls, floors and furniture.
  - Kitchen and Bakery pieces for the Café, KayKit shelves and books for the Library.
  - Plain walls plus scripted props for the Home Office and Gaming Den.

### 12.4 Pipeline rules [Claude's call]
- All Blender generator and fracture scripts live in the repo (for example `tools/blender/`), and one command rebuilds every generated asset.
- Export to Unity with a fixed import preset: scale, and a material remap onto one shared atlas material per atlas.
- **An asset validator** runs on import and in CI. It checks the triangle budget, that there's one material, that the UVs fall inside atlas cells, and that the scale is sane.
- **An asset license log** (one file) lists every model, sound and font with its source and license or AI-tool terms. It's a launch-bar item.

### 12.5 Audio
- Nothing downloaded yet.
- [Claude's call] Sound effects come from CC0 libraries (for example Kenney's audio packs) and AI sound generators, recorded in the license log. One cozy royalty-free music loop per room.
- The owner judges audio along with art at checkpoint 2.

---

## Appendix A: Research summary of the original (design only)

**Sources:** App Store and Google Play listings, AppGoblin SDK data, BlueStacks listing, two gameplay videos supplied by the owner (sampled frame by frame), and the type and method names in the APK's IL2CPP `global-metadata.dat` (read-only static inspection; the APK was never installed or run).

- **Engine:** Unity 6000.0.62f1, IL2CPP, Burst, Addressables. Built on an in-house "Foundation" framework (DI, ECS, FSM).
- **Loop:** fixed portrait camera, a pedestal table (sometimes two), a cannon at the bottom center. You tap and the cannon aims and fires (`AimAtScreenPoint`, `CannonAimSmoothing`). There are 20–30 balls per level, fired about once per 0.5–1 s. You win by clearing the table. Ground hits shatter pieces. The loss check waits for settle (`WaitForSettleThenFail`), then offers a coin/ad continue.
- **Objects:** stone, wooden box, jam jar, barrels, cones, cylinders, bricks, TNT (chain detonation), red and blue magnets.
- **Boosters:** Big Ball, Piercing Ball, Barrage, Fairy Dust (lift), Lightning, Infinite Moves. They unlock at set levels.
- **Difficulty:** Normal / Hard / Super Hard, with multipliers per tier and multi-stage levels.
- **Meta:** coins (15 per normal win), lives, Journey map, Power Play (leaderboard race), Trouble Trap, Push the Tempo, daily rewards, rewarded continues, and many offers.
- **Money:** ads via AppLovin MAX, Unity and AdMob mediation; IAP for Remove Ads at $7.99 and various bundles at $1.99–$7.99.
- **Reviews:**
  - Praise: satisfying destruction, varied levels, fair early curve.
  - Complaints: frustrating hard and super hard levels, small rewards, pushy purchases, players wanting a casual mode with unlimited lives.
- **Themes seen:** countryside, sea, beach. The theme changes every few levels.
