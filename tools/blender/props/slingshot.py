"""Y-frame slingshot. Origin = the M1 slingshot root; empties BandAnchorL/R sit at the M1 fork tips (x = +/-0.13, up 0.192)."""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

FORK_HALF_WIDTH = 0.13
TIP_Z = 0.192
HANDLE_LENGTH = 0.35
RADIUS = 0.018


def limb(name, start, end, radius):
    dx, dz = end[0] - start[0], end[2] - start[2]
    length = math.hypot(dx, dz)
    obj = shapes.rounded_cylinder(name, radius, length, bevel=radius * 0.4, segments=1, verts=10)
    obj.rotation_euler = (0, math.atan2(dx, dz), 0)
    obj.location = start
    return obj


def build(out):
    export.reset()
    parts = [
        limb("Handle", (0, 0, -HANDLE_LENGTH), (0, 0, 0), RADIUS * 1.3),
        limb("ArmL", (0, 0, 0), (-FORK_HALF_WIDTH, 0, TIP_Z), RADIUS),
        limb("ArmR", (0, 0, 0), (FORK_HALF_WIDTH, 0, TIP_Z), RADIUS),
    ]
    for side, x in (("L", -FORK_HALF_WIDTH), ("R", FORK_HALF_WIDTH)):
        knob = shapes.uv_sphere("Knob" + side, RADIUS * 1.4, segments=10, rings=6)
        knob.location = (x, 0, TIP_Z)
        parts.append(knob)
    bpy.context.view_layer.update()
    for p in parts:
        bpy.context.view_layer.objects.active = p
        p.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for p in parts:
        atlas.paint(p, CELLS["terracotta"], height_range=(-HANDLE_LENGTH, TIP_Z))
    frame = shapes.join("Slingshot", parts)
    for side, x in (("L", -FORK_HALF_WIDTH), ("R", FORK_HALF_WIDTH)):
        anchor = bpy.data.objects.new("BandAnchor" + side, None)
        bpy.context.scene.collection.objects.link(anchor)
        anchor.location = (x, 0, TIP_Z)
        anchor.parent = frame
    export.fbx([frame], os.path.join(out, "Slingshot.fbx"))


if __name__ == "__main__":
    build(export.args().out)
