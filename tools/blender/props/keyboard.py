"""Decorative keyboard for the side table: a rounded case with a grid of key bumps (<= 2,500 tris)."""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

CASE = (0.9, 0.3, 0.035)
KEY = 0.052
GAP = 0.01
COLS, ROWS = 14, 4


def build(out):
    export.reset()
    case = shapes.rounded_box("Keyboard", CASE, bevel=0.012, segments=2)
    atlas.paint(case, CELLS["slate"])
    keys = [case]
    span_x = COLS * KEY + (COLS - 1) * GAP
    span_y = ROWS * KEY + (ROWS - 1) * GAP
    for r in range(ROWS):
        for c in range(COLS):
            key = shapes.rounded_box(f"Key{r}_{c}", (KEY, KEY, 0.02), bevel=0)
            key.location = (-span_x / 2 + KEY / 2 + c * (KEY + GAP), -span_y / 2 + KEY / 2 + r * (KEY + GAP), CASE[2])
            keys.append(key)
    for key in keys[1:]:
        atlas.paint(key, CELLS["white"], height_range=(-0.02, 0.02))
    keyboard = shapes.join("Keyboard", keys)
    export.fbx([keyboard], os.path.join(out, "Keyboard.fbx"))


if __name__ == "__main__":
    build(export.args().out)
