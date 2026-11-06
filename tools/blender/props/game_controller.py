"""Game controllers for the Gaming Den (Room 5), lying flat: 0.4 wide, 0.1 tall, 0.26 deep (Blender +Y away).

Two rounded grips towards the player, a d-pad on the left and four buttons on the right as bumps. The underside is
flat so a controller lies still and stacks on keycaps. Writes GameController_<colour>.fbx for each body colour.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

BODY = (0.4, 0.2, 0.08)
GRIP_RADIUS = 0.065
GRIP_X = 0.13
GRIP_Y = -0.07
COLOURS = {"charcoal": ("charcoal", "gray_mid"), "red": ("red", "white"), "sky": ("sky", "white")}  # body, bumps
BUTTON_CELLS = ("yellow", "grass", "blue", "red")


def controller(name, body_cell, bump_cell):
    body = shapes.rounded_box("Body", BODY, bevel=0.03, segments=2)
    body.location.y = 0.02
    atlas.paint(body, CELLS[body_cell])
    parts = [body]
    for sx in (-1, 1):
        grip = shapes.rounded_cylinder("Grip", GRIP_RADIUS, BODY[2], bevel=0.025, segments=2, verts=12)
        grip.location = (sx * GRIP_X, GRIP_Y, 0.0)
        atlas.paint(grip, CELLS[body_cell])
        parts.append(grip)
    for size in ((0.075, 0.024, 0.012), (0.024, 0.075, 0.012)):
        arm = shapes.rounded_box("Dpad", size, bevel=0.004, segments=1)
        arm.location = (-0.1, 0.04, BODY[2])
        atlas.paint(arm, CELLS[bump_cell])
        parts.append(arm)
    for i, (dx, dy) in enumerate(((0.0, 0.03), (0.03, 0.0), (0.0, -0.03), (-0.03, 0.0))):
        button = shapes.rounded_cylinder("Button", 0.013, 0.012, bevel=0.003, segments=1, verts=8)
        button.location = (0.1 + dx, 0.04 + dy, BODY[2])
        atlas.paint(button, CELLS[BUTTON_CELLS[i]])
        parts.append(button)
    return shapes.join(name, parts)


def build(out):
    for colour, (body_cell, bump_cell) in COLOURS.items():
        export.reset()
        name = f"GameController_{colour}"
        export.fbx([controller(name, body_cell, bump_cell)], os.path.join(out, f"{name}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
