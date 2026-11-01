"""Home Office shell: 8 x 8 floor at z = 0, back wall at y = +3.5, left wall at x = -4, window, baseboards.

Blender +Y is away from the player (Unity +Z). Each part stays a separate child mesh.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

FLOOR = 8.0
FLOOR_CENTER_Y = -0.5
WALL_H = 4.0
WALL_T = 0.1
BACK_Y = 3.5
LEFT_X = -4.0


def part(name, size, location, cell, height_range=(0.0, WALL_H)):
    obj = shapes.rounded_box(name, size, bevel=0.01, segments=1)
    obj.location = location
    atlas.paint(obj, CELLS[cell], height_range=height_range)
    return obj


def build(out):
    export.reset()
    root = bpy.data.objects.new("RoomShell", None)
    bpy.context.scene.collection.objects.link(root)
    wall_len_x = FLOOR
    wall_len_y = BACK_Y - (FLOOR_CENTER_Y - FLOOR / 2)
    left_center_y = (FLOOR_CENTER_Y - FLOOR / 2 + BACK_Y - WALL_T) / 2
    parts = [
        part("Floor", (FLOOR, FLOOR, 0.05), (0, FLOOR_CENTER_Y, -0.05), "tan", (-0.05, 0.0)),
        part("BackWall", (wall_len_x - 0.01, WALL_T, WALL_H), (0, BACK_Y - WALL_T / 2, 0), "cream"),
        part("LeftWall", (WALL_T, wall_len_y - WALL_T, WALL_H), (LEFT_X + WALL_T / 2, left_center_y, 0), "peach"),
        part("BaseboardBack", (wall_len_x - 0.01, 0.03, 0.12), (0, BACK_Y - WALL_T - 0.015, 0), "white", (0.0, 0.12)),
        part("BaseboardLeft", (0.03, wall_len_y - WALL_T, 0.12), (LEFT_X + WALL_T + 0.015, left_center_y, 0), "white", (0.0, 0.12)),
        part("WindowFrame", (1.6, 0.06, 1.2), (-1.4, BACK_Y - WALL_T - 0.03, 1.6), "white", (1.6, 2.8)),
        part("WindowGlass", (1.44, 0.02, 1.04), (-1.4, BACK_Y - WALL_T - 0.07, 1.68), "glass", (1.68, 2.72)),
    ]
    for p in parts:
        p.parent = root
    export.fbx([root], os.path.join(out, "RoomShell.fbx"))


if __name__ == "__main__":
    build(export.args().out)
