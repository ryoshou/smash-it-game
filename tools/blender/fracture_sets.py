"""Pre-fractured sets for the pack breakables (spec §12.3: 6-12 pieces sharing the atlas).

Mug_<colour>_broken: the mug body cut into 7 shards plus the handle whole (8 pieces).
CactusPot_broken: the pot cut into 7 shards plus the cactus whole, which spills out (8 pieces).
Plate_broken / MilkBottle_broken: the plate / bottle cut into 7 shards plus its small parts whole (8 pieces).
Pieces keep the intact model's coordinates, so a set spawned at the prop's pose lines up with it.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import bpy  # noqa: E402

from lib import export, fracture, palette, shapes  # noqa: E402

PACKS = os.path.join(os.path.dirname(palette.ATLAS_PATH), "..")
SEED = 11
SHARDS = 7


def loose_parts(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.separate(type='LOOSE')
    bpy.ops.object.mode_set(mode='OBJECT')
    parts = list(bpy.context.selected_objects)
    return sorted(parts, key=lambda o: len(o.data.vertices), reverse=True)


def build_set(name, pack_file, material, out):
    export.reset()
    source = fracture.import_pack(os.path.join(PACKS, pack_file))
    largest, *rest = loose_parts(source)
    whole = shapes.join("Whole", rest) if len(rest) > 1 else rest[0]
    shards = fracture.shatter(largest, pieces=SHARDS, seed=SEED)
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    for i, piece in enumerate([*shards, whole]):
        piece.name = f"Piece_{i:02d}"
        piece.parent = root
    export.fbx([root], os.path.join(out, f"{name}.fbx"), material=material, authored=False)


def build(out):
    for colour in ("pink", "blue", "yellow"):
        build_set(f"Mug_{colour}_broken", os.path.join("TinyTreats", f"mug_A_{colour}.fbx"), "Atlas_TinyTreats", out)
    build_set("CactusPot_broken", os.path.join("KayKitFurniture", "cactus_small_A.fbx"), "Atlas_KayKitFurniture", out)
    build_set("Plate_broken", os.path.join("TinyTreats", "plate.fbx"), "Atlas_TinyTreats", out)
    build_set("MilkBottle_broken", os.path.join("TinyTreats", "milk.fbx"), "Atlas_TinyTreats", out)


if __name__ == "__main__":
    build(export.args().out)
