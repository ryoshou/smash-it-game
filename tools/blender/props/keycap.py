"""Keycaps: footprints match the M1 colliders (1u = 0.15 x 0.15, height 0.11), tapered body, dished top, raised legend.

Writes Keycap<Size>_<colour>.fbx for every size and colour variant.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bmesh  # noqa: E402
import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

HEIGHT = 0.11
TOP_SCALE = 0.82
DISH_DEPTH = 0.008
LEGEND_DEPTH = 0.003
BEVEL = 0.012

SIZES = {  # name: (width x, depth y, legend)
    "Keycap1u": (0.15, 0.15, "A"),
    "Keycap2u": (0.31, 0.15, "Shift"),
    "KeycapSpace": (0.63, 0.15, ""),
    "KeycapEnter": (0.225, 0.15, "Enter"),
}
VARIANTS = {  # colour: (body cell, legend cell)
    "cream": ("cream", "charcoal"),
    "pink": ("pink", "rose"),
    "sky": ("sky", "blue"),
}


def body(name, width, depth):
    obj = shapes.rounded_box(name, (width, depth, HEIGHT), bevel=BEVEL, segments=2)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    top = [v for v in bm.verts if v.co.z > HEIGHT * 0.5]
    for v in top:
        v.co.x *= TOP_SCALE + (1 - TOP_SCALE) * (1 - 0.15 / max(width, 0.15)) * 0.5
        v.co.y *= TOP_SCALE
    flat = max((f for f in bm.faces if f.normal.z > 0.99), key=lambda f: f.calc_area())
    inner = bmesh.ops.inset_region(bm, faces=[flat], thickness=min(width, depth) * 0.12, depth=0)["faces"]
    bmesh.ops.translate(bm, vec=(0, 0, -DISH_DEPTH), verts=list({v for f in inner for v in f.verts}))
    bm.to_mesh(obj.data)
    bm.free()
    shapes.smooth(obj)
    return obj


def legend(text, depth):
    if not text:
        return None
    curve = bpy.data.curves.new("Legend", type='FONT')
    curve.body = text
    curve.align_x = 'CENTER'
    curve.align_y = 'CENTER'
    curve.size = depth * (0.42 if len(text) == 1 else 0.2)
    curve.extrude = LEGEND_DEPTH / 2
    curve.resolution_u = 2
    obj = bpy.data.objects.new("Legend", curve)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target='MESH')
    obj = bpy.context.view_layer.objects.active
    obj.location.z = HEIGHT - DISH_DEPTH + LEGEND_DEPTH / 2
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def build(out):
    for name, (width, depth, text) in SIZES.items():
        for colour, (body_cell, legend_cell) in VARIANTS.items():
            export.reset()
            cap = body(f"{name}_{colour}", width, depth)
            atlas.paint(cap, CELLS[body_cell])
            mark = legend(text, depth)
            if mark is not None:
                atlas.paint(mark, CELLS[legend_cell], height_range=(0.0, HEIGHT))
                cap = shapes.join(f"{name}_{colour}", [cap, mark])
            export.fbx([cap], os.path.join(out, f"{name}_{colour}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
