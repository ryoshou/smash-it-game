"""Decorative monitor: base, neck, bezel and a screen showing a cell-coloured desktop (a spike refinement)."""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

SCREEN_W, SCREEN_H, BEZEL_D = 0.9, 0.55, 0.05
NECK_H = 0.18
SCREEN_Z = 0.03 + NECK_H


def box(name, size, location, cell, height_range=None):
    obj = shapes.rounded_box(name, size, bevel=min(size) * 0.25 if min(size) > 0.01 else 0, segments=1)
    obj.location = location
    atlas.paint(obj, CELLS[cell], height_range=height_range)
    return obj


def build(out):
    export.reset()
    parts = [
        box("Base", (0.36, 0.22, 0.03), (0, 0, 0), "slate"),
        box("Neck", (0.06, 0.04, NECK_H), (0, 0.04, 0.03), "slate"),
        box("Bezel", (SCREEN_W, BEZEL_D, SCREEN_H), (0, 0, SCREEN_Z), "charcoal"),
    ]
    # Screen and desktop sit just in front of the bezel (the side facing the player, -Y).
    front = -BEZEL_D / 2 - 0.004
    screen_range = (SCREEN_Z, SCREEN_Z + SCREEN_H)
    parts.append(box("Screen", (SCREEN_W - 0.06, 0.006, SCREEN_H - 0.06), (0, front, SCREEN_Z + 0.03), "sky", screen_range))
    parts.append(box("Window", (0.36, 0.006, 0.24), (-0.14, front - 0.004, SCREEN_Z + 0.2), "cream", screen_range))
    parts.append(box("Window2", (0.22, 0.006, 0.16), (0.22, front - 0.004, SCREEN_Z + 0.28), "pink", screen_range))
    parts.append(box("Taskbar", (SCREEN_W - 0.06, 0.006, 0.04), (0, front - 0.004, SCREEN_Z + 0.03), "slate", screen_range))
    monitor = shapes.join("Monitor", parts)
    export.fbx([monitor], os.path.join(out, "Monitor.fbx"))


if __name__ == "__main__":
    build(export.args().out)
