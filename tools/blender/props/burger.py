"""The Kitchen burger, stacked from KayKit Restaurant Bits' own parts (the pack's whole burger is 1,622 triangles, over
the 1,500 prop budget; the parts come to about 1,080).

Burger.fbx: bun bottom, patty, cheese, tomato, lettuce and bun top, joined into one mesh.
Burger_broken.fbx: the same six parts as separate pieces in the same places, so a set spawned at the burger's pose lines
up with it and food splashes apart into its layers (Kitchen spec §6).
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import export, fracture, palette, shapes  # noqa: E402

PACK = os.path.join(os.path.dirname(palette.ATLAS_PATH), "..", "KayKitRestaurant")
MATERIAL = "Atlas_KayKitRestaurant"
LAYERS = ("bun_bottom", "burger_cooked", "cheese_slice", "tomato_slice", "lettuce_slice", "bun_top")
OVERLAP = 0.03  # each layer sinks this far into the one below, so the stack reads as one burger


def stack():
    """Imports the layers bottom to top, each resting on the last; returns them in order."""
    parts, top = [], 0.0
    for layer in LAYERS:
        part = fracture.import_pack(os.path.join(PACK, f"food_ingredient_{layer}.fbx"))
        lowest = min(v.co.z for v in part.data.vertices)
        rise = max(0.0, top - OVERLAP) - lowest if parts else -lowest
        for v in part.data.vertices:
            v.co.z += rise
        top = max(v.co.z for v in part.data.vertices)
        part.name = layer
        parts.append(part)
    return parts


def build(out):
    export.reset()
    export.fbx([shapes.join("Burger", stack())], os.path.join(out, "Burger.fbx"), material=MATERIAL, authored=False)
    export.reset()
    root = bpy.data.objects.new("Burger_broken", None)
    bpy.context.scene.collection.objects.link(root)
    for i, part in enumerate(stack()):
        part.name = f"Piece_{i:02d}"
        part.parent = root
    export.fbx([root], os.path.join(out, "Burger_broken.fbx"), material=MATERIAL, authored=False)


if __name__ == "__main__":
    build(export.args().out)
