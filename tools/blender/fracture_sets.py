"""Pre-fractured sets for the pack breakables (spec §12.3: 6-12 pieces sharing the atlas).

Mug_<colour>_broken: the mug body cut into 7 shards plus the handle whole (8 pieces).
CactusPot_broken: the pot cut into 7 shards plus the cactus whole, which spills out (8 pieces).
Plate_broken / MilkBottle_broken / Hourglass_broken: the plate / bottle / glass cut into 7 shards plus its other parts
whole (8 pieces).
Kitchen kinds (Kitchen spec §5-6): dishes and glass cut into 8 shards (the teapot 7 plus its lid, handle and spout joined whole:
8), jars and bottles into 6 chunks. Food has no pieces: it squashes whole.
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


def build_set(name, pack_file, material, out, shards=SHARDS):
    export.reset()
    source = fracture.import_pack(os.path.join(PACKS, pack_file))
    largest, *rest = loose_parts(source)
    whole = shapes.join("Whole", rest) if len(rest) > 1 else rest[0]
    cut = fracture.shatter(largest, pieces=shards, seed=SEED)
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    for i, piece in enumerate([*cut, whole]):
        piece.name = f"Piece_{i:02d}"
        piece.parent = root
    export.fbx([root], os.path.join(out, f"{name}.fbx"), material=material, authored=False)


def build_chunks(name, pack_file, material, out, pieces):
    """The whole model, every part together, cut into a few chunks."""
    export.reset()
    source = fracture.import_pack(os.path.join(PACKS, pack_file))
    chunks = fracture.shatter(source, pieces=pieces, seed=SEED)
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    for i, piece in enumerate(chunks):
        piece.name = f"Piece_{i:02d}"
        piece.parent = root
    export.fbx([root], os.path.join(out, f"{name}.fbx"), material=material, authored=False)


def build(out):
    for colour in ("pink", "blue", "yellow"):
        build_set(f"Mug_{colour}_broken", os.path.join("TinyTreats", f"mug_A_{colour}.fbx"), "Atlas_TinyTreats", out)
    build_set("CactusPot_broken", os.path.join("KayKitFurniture", "cactus_small_A.fbx"), "Atlas_KayKitFurniture", out)
    build_set("Plate_broken", os.path.join("TinyTreats", "plate.fbx"), "Atlas_TinyTreats", out)
    build_set("MilkBottle_broken", os.path.join("TinyTreats", "milk.fbx"), "Atlas_TinyTreats", out)
    build_set("Hourglass_broken", os.path.join("KayKitBoardGame", "hourglass.fbx"), "Atlas_KayKitBoardGame", out)
    build_kitchen(out)


# name: (pack file, atlas, pieces); every part of the model is cut together.
KITCHEN_CHUNKS = {
    "DinnerPlate_broken": ("TinyTreats/plate_A.fbx", "Atlas_TinyTreats", 8),
    "Bowl_broken": ("TinyTreats/bowl.fbx", "Atlas_TinyTreats", 8),
    "WineGlass_broken": ("TinyTreats/wine_glass.fbx", "Atlas_TinyTreats", 8),
    "JamJar_broken": ("TinyTreats/jam.fbx", "Atlas_TinyTreats", 6),
    "KetchupBottle_broken": ("KayKitRestaurant/ketchup.fbx", "Atlas_KayKitRestaurant", 6),
}


def build_kitchen(out):
    build_set("Teapot_broken", os.path.join("TinyTreats", "teapot.fbx"), "Atlas_TinyTreats", out)
    for name, (pack_file, material, pieces) in KITCHEN_CHUNKS.items():
        build_chunks(name, pack_file, material, out, pieces=pieces)


if __name__ == "__main__":
    build(export.args().out)
