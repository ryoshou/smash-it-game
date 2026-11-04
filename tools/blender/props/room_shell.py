"""Room shells: a cosy 3.5 m wide room around the play desk (owner, checkpoint 2: "feel like we're in a room").

The portrait camera sees only about +/-2 m at the back wall, so the side walls stand at x = +/-1.75 and show as
corners, and the ceiling (3.1 m) shows along the top edge. Blender +Y is away from the player (Unity +Z). Parts stay
separate child meshes: Floor, BackWall, LeftWall, RightWall, Ceiling, mouldings, baseboards, window, Pendant.
Every room uses the same geometry (RoomKit's wall constants hold everywhere); STYLES only recolours it.
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

HALF_WIDTH = 1.75
FRONT_Y = -4.5
BACK_Y = 3.5
HEIGHT = 3.1
WALL_T = 0.1
TRIM = 0.12
PENDANT = (0.0, 1.2)  # x, y under the ceiling

# name: (floor, back wall, side walls, trim, pendant shade)
STYLES = {
    "RoomShell": ("tan", "peach", "cream", "white", "teal"),
    "RoomShell_Cafe": ("apricot", "butter", "cream", "white", "rose"),
    "RoomShell_Library": ("brown", "sage", "sage", "cream", "pine"),
    "RoomShell_Den": ("slate", "ocean", "blue", "gray_dark", "periwinkle"),
}


def part(name, size, location, cell, height_range=(0.0, HEIGHT)):
    obj = shapes.rounded_box(name, size, bevel=0.01, segments=1)
    obj.location = location
    atlas.paint(obj, CELLS[cell], height_range=height_range)
    return obj


def pendant(shade_cell):
    x, y = PENDANT
    cord = shapes.rounded_cylinder("Cord", 0.012, 0.45, bevel=0, verts=6)
    cord.location = (x, y, HEIGHT - 0.45)
    shade = shapes.rounded_cylinder("Shade", 0.26, 0.2, bevel=0.03, segments=2, verts=20)
    shade.location = (x, y, HEIGHT - 0.65)
    bulb = shapes.uv_sphere("Bulb", 0.08, segments=10, rings=6)
    bulb.location = (x, y, HEIGHT - 0.68)
    for obj, cell in ((cord, "charcoal"), (shade, shade_cell), (bulb, "butter")):
        atlas.paint(obj, CELLS[cell], height_range=(HEIGHT - 0.8, HEIGHT))
    return shapes.join("Pendant", [shade, cord, bulb])


def shell(name, floor, back, side, trim, shade):
    root = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(root)
    depth = BACK_Y - FRONT_Y
    mid_y = (BACK_Y + FRONT_Y) / 2
    width = 2 * HALF_WIDTH
    parts = [
        part("Floor", (width, depth, 0.05), (0, mid_y, -0.05), floor, (-0.05, 0.0)),
        part("BackWall", (width, WALL_T, HEIGHT), (0, BACK_Y - WALL_T / 2, 0), back),
        part("LeftWall", (WALL_T, depth - WALL_T, HEIGHT), (-HALF_WIDTH + WALL_T / 2, mid_y - WALL_T / 2, 0), side),
        part("RightWall", (WALL_T, depth - WALL_T, HEIGHT), (HALF_WIDTH - WALL_T / 2, mid_y - WALL_T / 2, 0), side),
        part("Ceiling", (width, depth, 0.05), (0, mid_y, HEIGHT), "white", (HEIGHT, HEIGHT + 0.05)),
        part("MouldingBack", (width - 2 * WALL_T, 0.06, TRIM), (0, BACK_Y - WALL_T - 0.03, HEIGHT - TRIM), trim, (HEIGHT - TRIM, HEIGHT)),
        part("MouldingLeft", (0.06, depth - WALL_T, TRIM), (-HALF_WIDTH + WALL_T + 0.03, mid_y - WALL_T / 2, HEIGHT - TRIM), trim, (HEIGHT - TRIM, HEIGHT)),
        part("MouldingRight", (0.06, depth - WALL_T, TRIM), (HALF_WIDTH - WALL_T - 0.03, mid_y - WALL_T / 2, HEIGHT - TRIM), trim, (HEIGHT - TRIM, HEIGHT)),
        part("BaseboardBack", (width - 2 * WALL_T, 0.04, TRIM), (0, BACK_Y - WALL_T - 0.02, 0), trim, (0.0, TRIM)),
        part("BaseboardLeft", (0.04, depth - WALL_T, TRIM), (-HALF_WIDTH + WALL_T + 0.02, mid_y - WALL_T / 2, 0), trim, (0.0, TRIM)),
        part("BaseboardRight", (0.04, depth - WALL_T, TRIM), (HALF_WIDTH - WALL_T - 0.02, mid_y - WALL_T / 2, 0), trim, (0.0, TRIM)),
        part("WindowFrame", (1.2, 0.06, 1.1), (-0.85, BACK_Y - WALL_T - 0.03, 1.45), trim, (1.45, 2.55)),
        part("WindowGlass", (1.04, 0.02, 0.94), (-0.85, BACK_Y - WALL_T - 0.07, 1.53), "glass", (1.53, 2.47)),
        part("WindowSill", (1.36, 0.16, 0.05), (-0.85, BACK_Y - WALL_T - 0.08, 1.4), trim, (1.4, 1.45)),
        pendant(shade),
    ]
    for p in parts:
        p.parent = root
    return root


def build(out):
    for name, cells in STYLES.items():
        export.reset()
        export.fbx([shell(name, *cells)], os.path.join(out, f"{name}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
