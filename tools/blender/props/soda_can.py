"""Soda cans for the Gaming Den (Room 4), and the fizzy soda special (spec §6.2).

A can is 0.3 tall and 0.18 wide (a Tiny Treats mug is 0.35 tall), with a rounded rim and a ring-pull bump; flat top
and bottom so cans stack. Writes SodaCan_<colour>.fbx for each colour, and FizzySoda.fbx: the same can in lime with a
band of bubble dots and a bulged top, so it reads as "about to burst".
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

RADIUS = 0.09
HEIGHT = 0.3
RIM = 0.012
COLOURS = {"red": "red", "blue": "blue", "grass": "grass"}  # name: body cell
BUBBLES = 8
BUBBLE_RADIUS = 0.018


def can(name, cell, fizzy=False):
    body = shapes.rounded_cylinder("Body", RADIUS, HEIGHT - RIM, bevel=0.018, segments=2, verts=16)
    atlas.paint(body, CELLS[cell])
    rim = shapes.rounded_cylinder("Rim", RADIUS * 0.88, RIM, bevel=0.004, segments=1, verts=16)
    rim.location.z = HEIGHT - RIM
    atlas.paint(rim, CELLS["silver"])
    tab = shapes.rounded_box("Tab", (0.05, 0.03, 0.006), bevel=0.002, segments=1)
    tab.location = (0.0, -0.02, HEIGHT - 0.005)  # 1 mm proud of the rim, so stacked cans sit flat
    atlas.paint(tab, CELLS["gray"])
    parts = [body, rim, tab]
    if fizzy:
        dome = shapes.uv_sphere("Dome", RADIUS * 0.8, segments=12, rings=6)
        dome.scale = (1.0, 1.0, 0.35)
        dome.location.z = HEIGHT - RIM
        atlas.paint(dome, CELLS["silver"])
        parts.append(dome)
        for i in range(BUBBLES):
            angle = 2 * math.pi * i / BUBBLES
            dot = shapes.uv_sphere("Bubble", BUBBLE_RADIUS, segments=6, rings=4)
            dot.location = (math.cos(angle) * RADIUS, math.sin(angle) * RADIUS, HEIGHT * (0.45 + 0.12 * (i % 2)))
            atlas.paint(dot, CELLS["white"])
            parts.append(dot)
    return shapes.join(name, parts)


def build(out):
    for colour, cell in COLOURS.items():
        export.reset()
        name = f"SodaCan_{colour}"
        export.fbx([can(name, cell)], os.path.join(out, f"{name}.fbx"))
    export.reset()
    export.fbx([can("FizzySoda", "yellow", fizzy=True)], os.path.join(out, "FizzySoda.fbx"))


if __name__ == "__main__":
    build(export.args().out)
