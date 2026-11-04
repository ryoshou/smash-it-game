"""Pencil cups for the Library (Room 3): a rounded cup with three pencils rising out of it, one joined mesh.

Sized to the props around it (a Tiny Treats mug is 0.35 tall): the cup is 0.26 tall and 0.2 wide, the pencil tips
reach about 0.42. Writes PencilCup_<colour>.fbx for each cup colour.
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

CUP_RADIUS = 0.1
CUP_HEIGHT = 0.26
PENCIL_RADIUS = 0.017
PENCIL_LENGTH = 0.2
TIP_LENGTH = 0.035
COLOURS = {"teal": "teal", "rose": "rose", "butter": "butter"}  # name: cup cell
# (x, y offset in the cup, lean about x and y in degrees, pencil cell)
PENCILS = ((-0.035, 0.02, -8.0, -6.0, "yellow"), (0.035, 0.015, 6.0, -9.0, "red"), (0.0, -0.035, 0.0, 10.0, "blue"))


def pencil(x, y, lean_x, lean_y, cell):
    body = shapes.rounded_cylinder("Pencil", PENCIL_RADIUS, PENCIL_LENGTH, bevel=0, verts=6)
    atlas.paint(body, CELLS[cell])
    tip = shapes.rounded_cylinder("Tip", PENCIL_RADIUS * 0.45, TIP_LENGTH, bevel=0, verts=6)
    tip.location.z = PENCIL_LENGTH
    tip.scale = (1.0, 1.0, 1.0)
    atlas.paint(tip, CELLS["tan"])
    stick = shapes.join("Pencil", [body, tip])
    stick.location = (x, y, CUP_HEIGHT - PENCIL_LENGTH * 0.45)
    stick.rotation_euler = (math.radians(lean_x), math.radians(lean_y), 0.0)
    return stick


def cup(name, cell):
    body = shapes.rounded_cylinder("Cup", CUP_RADIUS, CUP_HEIGHT, bevel=0.015, segments=2, verts=14)
    atlas.paint(body, CELLS[cell])
    parts = [body] + [pencil(*spec) for spec in PENCILS]
    return shapes.join(name, parts)


def build(out):
    for colour, cell in COLOURS.items():
        export.reset()
        name = f"PencilCup_{colour}"
        export.fbx([cup(name, cell)], os.path.join(out, f"{name}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
