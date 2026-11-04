"""Headphones for the Gaming Den backdrop (decor only): a padded band arching over two ear cups, standing upright."""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

SPAN = 0.17         # half the distance between the ear cups
BAND_SEGMENTS = 9
CUP_RADIUS = 0.07
CUP_DEPTH = 0.05


def build(out):
    export.reset()
    parts = []
    for i in range(BAND_SEGMENTS):
        angle = math.pi * i / (BAND_SEGMENTS - 1)
        piece = shapes.rounded_box("Band", (0.05, 0.045, 0.03), bevel=0.008, segments=1)
        piece.location = (math.cos(angle) * SPAN, 0.0, CUP_RADIUS + math.sin(angle) * SPAN)
        piece.rotation_euler = (0.0, -angle + math.pi / 2, 0.0)
        atlas.paint(piece, CELLS["charcoal"])
        parts.append(piece)
    for sx in (-1, 1):
        cup = shapes.rounded_cylinder("Cup", CUP_RADIUS, CUP_DEPTH, bevel=0.015, segments=2, verts=14, base_at_origin=False)
        cup.rotation_euler = (0.0, math.pi / 2, 0.0)
        cup.location = (sx * SPAN, 0.0, CUP_RADIUS)
        atlas.paint(cup, CELLS["periwinkle"])
        parts.append(cup)
    export.fbx([shapes.join("Headphones", parts)], os.path.join(out, "Headphones.fbx"))


if __name__ == "__main__":
    build(export.args().out)
