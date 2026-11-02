"""Keycaps standing up with the letter towards the camera (owner, checkpoint 2).

Blender Z up, +Y away from the camera. A key of width W is W wide, 0.15 tall (its keyboard depth) and 0.11 deep (its
keyboard height). Top and bottom are flat so keys stack into walls; the left and right sides taper towards the
letter face, which is dished and carries a raised legend that stands just proud of the rim.
Writes Keycap<Size>_<colour>.fbx for every size and colour variant.
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bmesh  # noqa: E402
import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

TALL = 0.15          # world height (the key's depth on a keyboard)
DEEP = 0.11          # world depth towards the camera (the key's height on a keyboard)
FACE_SCALE = 0.82    # the letter face is this much narrower than the back
DISH_DEPTH = 0.004
LEGEND_DEPTH = 0.012
LEGEND_PROUD = 0.002  # how far the legend stands in front of the rim
BEVEL = 0.012

SIZES = {  # name: (width x, legend)
    "Keycap1u": (0.15, "A"),
    "Keycap2u": (0.31, "Shift"),
    "KeycapSpace": (0.63, ""),
    "KeycapEnter": (0.225, "Enter"),
}
ONE_U_LETTERS = {"cream": "A", "pink": "S", "sky": "D"}  # 1u keys vary by colour, like a keyboard row
VARIANTS = {  # colour: (body cell, legend cell)
    "cream": ("cream", "charcoal"),
    "pink": ("pink", "rose"),
    "sky": ("sky", "blue"),
}


def body(name, width):
    obj = shapes.rounded_box(name, (width, DEEP, TALL), bevel=BEVEL, segments=2)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    for v in bm.verts:  # taper only the left/right sides, towards the letter face (-Y); top and bottom stay flat
        if v.co.y < 0:
            v.co.x *= FACE_SCALE + (1 - FACE_SCALE) * (1 - 0.15 / max(width, 0.15)) * 0.5
    face = max((f for f in bm.faces if f.normal.y < -0.99), key=lambda f: f.calc_area())
    inner = bmesh.ops.inset_region(bm, faces=[face], thickness=min(width, TALL) * 0.12, depth=0)["faces"]
    bmesh.ops.translate(bm, vec=(0, DISH_DEPTH, 0), verts=list({v for f in inner for v in f.verts}))
    bm.to_mesh(obj.data)
    bm.free()
    shapes.smooth(obj)
    return obj


def legend(text):
    if not text:
        return None
    curve = bpy.data.curves.new("Legend", type='FONT')
    curve.body = text
    curve.align_x = 'CENTER'
    curve.align_y = 'CENTER'
    curve.size = TALL * (0.42 if len(text) == 1 else 0.2)
    curve.extrude = LEGEND_DEPTH / 2
    curve.resolution_u = 2
    obj = bpy.data.objects.new("Legend", curve)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target='MESH')
    obj = bpy.context.view_layer.objects.active
    # Text lies in XY facing +Z: stand it up facing -Y (towards the camera), reading left to right.
    obj.rotation_euler = (math.radians(90), 0, 0)
    obj.location = (0, -DEEP / 2 - LEGEND_PROUD + LEGEND_DEPTH / 2, TALL / 2)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return obj


def build(out):
    for name, (width, text) in SIZES.items():
        for colour, (body_cell, legend_cell) in VARIANTS.items():
            export.reset()
            cap = body(f"{name}_{colour}", width)
            atlas.paint(cap, CELLS[body_cell])
            mark = legend(ONE_U_LETTERS[colour] if name == "Keycap1u" else text)
            if mark is not None:
                atlas.paint(mark, CELLS[legend_cell], height_range=(0.0, TALL))
                cap = shapes.join(f"{name}_{colour}", [cap, mark])
            export.fbx([cap], os.path.join(out, f"{name}_{colour}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
