# Generating the AI cat (owner, with Meshy)

The game ships the blockout cat until this is done. Nothing here is required for checkpoint 2.

## Before you start
- Use a Meshy plan whose terms allow **commercial use** of generated models, and note the plan name and date.
  Claude records it in `docs/asset-licenses.md` (spec §12.3).

## In Meshy: Image to 3D
1. Upload `refs/cat_front.png` (and `refs/cat_three_quarter.png` if multi-view is offered). They show the blockout
   cat beside a pack mug and keycaps, so the style matches the game's other props.
2. If a text prompt is accepted, use:

   > cute chunky low-poly cartoon cat, sitting upright, facing forward, big round head, small rounded ears,
   > big dark oval eyes, small pink nose, cream muzzle and chest, orange tabby body, cream paws, short thick tail
   > curling up behind, soft smooth shapes, flat simple colours, toy-like, no fur detail, no clothing

3. Settings: target polycount around 10,000 (cleanup reduces to under 8,000), texture **on**, PBR/normal maps **off**
   (only the base colour is used), A-pose/T-pose **off** (the cat must stay sitting).
4. Generate a few, pick the cutest from the front and three-quarter views.
5. Download as **GLB** and save it as `download-assets/ai/cat/<yyyy-mm-dd>/cat.glb`.

## Then tell Claude
Claude runs `tools/blender/build.sh cat-ai` (decimate, recolour onto the atlas, rig, five animations), places the
joints in `landmarks_ai.json`, and rebuilds the scene. If the cat isn't cute enough, the spec's fallback is
commissioning just the cat from an artist.
