# Kitchen Room Design

**Date:** 2026-11-06 · **Status:** approved by the owner in conversation; awaiting their review of this document
**Builds on:** `docs/superpowers/specs/2026-10-26-cat-desk-smash-design.md` (the game spec). Everything there still holds
unless this document says otherwise. Comes before M4b (SDKs, release build).

Tags: **[Owner]** the owner decided it · **[Claude's call]** proposed by Claude, not yet confirmed by the owner ·
everything else is left to the builder (§8).

## 1. Intent

The owner wants more levels where one good shot sends a heap of small things over the edge and they all burst on the
floor. That moment is the most satisfying thing in the game, and it should come early and often so new players stay.
**[Owner]** It's for fun and retention.

So the game gets a new room, a **Kitchen**, with tables crowded with food and dishes. The payoff is the mess: lots of
things shattering and splashing. It is not a harder puzzle.

## 2. Goals, non-goals, anti-goals

**Goals**
- A Kitchen room whose levels hold many more, smaller objects than the other rooms. **[Owner]**
- Food that splashes when it breaks and dishes that shatter, so a feast level clears in a burst of colour. **[Owner]**
- Players reach the Kitchen early: it is Room 2. **[Owner]**

**Non-goals**
- No change to the break rule. Things still break only when they hit the floor (game spec §4.3). The ball does not
  smash things on the table. **[Owner]**
- No change to the Home Office, Café, Library or Gaming Den levels apart from their numbers (§3). The one exception is
  the donut's new splash (§6).
- No change to existing objects' sizes. The owner will settle object sizes and the back-desk wedge in a later pass
  (see memory: back-desk wedge kept until then).
- No SDKs, ads or analytics. Those are M4b.

**Anti-goals** (failure even if it technically works)
- **Oversized objects.** Kitchen objects must look real next to each other: no giant apple, no novelty-sized teapot.
  **[Owner]** See §5.
- Feast levels that feel like a chore or a hard puzzle. They are a reward.
- A frame-rate drop when a whole feast lands on the floor at once (game spec §3 performance bar: 60 fps target, never
  under 30 fps on a mid-range 2021 phone).
- Splash marks piling up on the floor. **[Owner]** They fade (§6).

## 3. Where it sits in the game

- **The game grows from 100 to 125 levels in 5 rooms of 25.** **[Owner]**
  1. Home Office: levels 1–25, unchanged.
  2. **Kitchen: levels 26–50, new.**
  3. Café: 51–75 (was 26–50).
  4. Library: 76–100 (was 51–75).
  5. Gaming Den: 101–125 (was 76–100).
- Rooms 3–5 keep their layouts, shots and difficulty exactly; they are only renumbered. **[Owner]** Their bot clear
  rates must still land in band after the move.
- **[Claude's call]** Bot results move about ±0.1 from run to run. A renumbered level that drifts out of band is
  re-run first. Only if it still misses does it get the smallest nudge (a prop moved, or a shot added or removed)
  that re-centres it. Every nudge is listed for the owner.
- The "new object to knock over" reveal and its introduction order will change, because the Kitchen now comes before
  the Café. Kinds the Café introduced (plate, donut, milk bottle) may now appear first in the Kitchen. The builder
  re-orders the introduction schedule so each kind still appears first in a level that shows it off.
- After level 125, play wraps to level 1, as it does after level 100 today.
- Saves: the game hasn't shipped, so a test save already past level 25 simply resumes in the Kitchen. No migration.

## 4. Kitchen levels

- **Two kinds of level.** **[Owner]**
  - **Normal levels:** 10–18 objects, about as many as the other rooms. They're smaller and packed closer, so one
    shot usually knocks over several.
  - **Feast levels:** about 8 of the 25, heaped with 25–40 objects. 40 stays the per-level limit, for phone
    performance (`LevelDef.MaxProps`). They get extra shots and are tuned to clear easily.
- **Level 50, the room finale, is the biggest feast.** **[Owner]**
- **Feasts are a reward, never hard.** **[Owner]** They are checked against the easy (teaching) band, 0.60–1.00
  clear rate.
- **[Claude's call]** Apart from the feasts, the room follows the other rooms' pattern: about every 5th level is Hard,
  and the rest are Normal, in the usual bands (normal 0.35–0.95, hard 0.15–0.65). Feasts don't land on Hard slots.
  Level 50 is the one exception: it takes the room-finale slot but uses the feast band.
- **[Claude's call]** Feasts are spread through the room rather than bunched, roughly one every three levels, so a
  player is never long without one. The first comes early, within the first three Kitchen levels.
- **[Claude's call]** New kitchen objects are introduced a few at a time across the room, not all in level 26.
- The two-desk rule from M4a still holds: on a level with the back desk, keep tall heavy objects outside its span
  (x ±0.6), and put only light objects on the back desk.

## 5. Objects and their sizes

**Roster [Owner listed the examples; the final list is the builder's within these lines]:** food, dishes, a teapot,
cans, ketchup and the like. The CC0 packs already in `download-assets/` cover this:
- Tiny Treats Charming Kitchen: kettle, pot, pan, plate, mug, containers, toaster, utensil cup.
- Tiny Treats Pleasant Picnic: teapot, apple, cheese, grapes, sandwich, jam jar, drink can, wine bottle, wine glass,
  thermos, bowl.
- KayKit Restaurant Bits: ketchup, jars, bowls, burger, tomato, onion, carrot, cheese and other ingredients.
- Tiny Treats Baked Goods: cakes, pies, bread, croissant, cupcake, muffin, cookie.

**[Claude's call]** About 12–16 new kinds, a mix of food and dishes.

**Reusing existing kinds [Owner]:** reuse an existing kind only where it is already in true proportion to the mug (the
mug itself, and the donut if it measures right). Where an existing kind is undersized (the plate at 0.45 wide, the
soda can at 0.30 tall, the milk bottle at 0.42 tall), the Kitchen gets its own correctly sized kind instead, such as
a dinner plate or a drink can. The old kinds stay as they are in the other rooms. Every new kind needs a row in
`docs/asset-licenses.md` and passes the existing art budget checks (≤ 1,500 triangles per prop, scene ≤ 100k).

**Sizes [Owner]: true proportion to each other, at the game's current scale.**
- The game is not at real-world scale. Everything is enlarged to read on a phone (the mug is about 3.5× a real mug,
  the ball is 18 cm across). The owner agreed that bringing everything down to real size is not wanted.
- **The mug is the reference.** The mug is 0.35 units tall, standing for a real ~10 cm mug, so **1 real cm ≈ 0.035
  units**. Each new kind is sized so its real-world size relative to a real mug is kept. Rough targets the builder
  checks against real objects:

  | Object | Real size | In game (units) |
  |---|---|---|
  | Apple, tomato | ~7–8 cm across | ~0.25–0.28 |
  | Teapot | ~15 cm tall | ~0.5 |
  | Ketchup bottle | ~20 cm tall | ~0.7 |
  | Drink can | ~12 cm tall | ~0.42 |
  | Bowl | ~15 cm across, ~6 cm tall | ~0.5 × 0.2 |
  | Wine glass | ~18 cm tall | ~0.63 |

- Existing kinds keep their current sizes (non-goal above), even where they are off true proportion.
- Each kind's mass and feel follow its material: dishes and glass are light and shatter, food is light and splashes,
  a full pot or kettle is heavier. Numbers live in the prop catalog and tuning assets, never in code (game spec §5).

## 6. Breaking: shatter and splash

The trigger is unchanged: an object breaks when it hits the floor.
- **Dishes, glass, ceramic: shatter** into pieces, like the mug and plate today (pre-fractured, game spec §12.3).
- **Food: splash.** **[Owner]** It bursts into chunks of itself plus a juicy burst in its own colour (red for
  ketchup and tomato, yellow for cheese, and so on). A coloured splat appears on the floor where it landed.
- **Splats fade away** after a moment. They don't build up through the level. **[Owner]**
- **[Claude's call]** Food gets its own wet "splat" sound family, distinct from the ceramic shatter.
- **[Claude's call]** The existing donut is food, so it splashes too, in every room. This is the one look-and-sound
  change to the Café; its layouts are untouched.
- The packs' ready-cut pieces (apple pieces, tomato slices, chopped cheese, cake slices) may serve as food chunks.
- Debris stays pooled and fades, as today. A full feast landing at once must hold the performance bar (§2), so the
  builder may use fewer pieces per food object than the 6–12 used for dishes, and may cap live debris.

## 7. The room itself

- Backdrop built from the Tiny Treats Charming Kitchen pieces: kitchen walls, tiled floor, countertops, stove,
  fridge, wall cabinets, and so on. It follows the other rooms' build path (a `Rooms/` builder, `RoomKit`, its own
  RoomTheme).
- The desk look is a kitchen table that matches the play desk exactly (top 2.0 × 1.5 at y 1.0, centred at the origin).
- **Music [Owner]: a new track.** All four mintodog loops are already used, one per room. The Kitchen gets a fifth
  cozy royalty-free loop in the same spirit (CC0 preferred), so no room reuses another's track. It needs a row in the
  licence log.
- Nothing in the backdrop may block the shot or the camera's view of the table.

## 8. Left to the builder

- The final object roster within §5, each kind's mass, and its break pieces.
- How the splash and the splat are made (particles, a flat fading quad, a decal, or similar), within the rules: no
  new packages without asking, and no ProjectSettings changes unless the plan says so.
- How Rooms 3–5 are renumbered in code and assets (folder names, level ids, builders), as long as their layouts don't
  change.
- Level layouts, shots per level, and which slots are feasts, within §4.
- The new introduction schedule (§3).
- Whether feast levels show a label on screen (like "Hard" does), and its wording. All player-visible text goes in
  `UiStrings`.

## 9. Done when

- 125 levels play in order, wrapping from 125 to 1, with the Kitchen at 26–50.
- Every level, including the renumbered ones, passes its bot band. Feasts pass the easy band.
- Every Kitchen level is stable at spawn and stays within the per-level and scene budgets.
- The owner's on-device check: feasts feel satisfying, sizes look real, splats fade, no stutter when a feast lands, and
  the Kitchen's look and music.
