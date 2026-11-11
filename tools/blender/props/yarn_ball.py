"""Yarn ball visual: diameter 0.18 (2 x ShotTuning.ballRadius), wrapped strands. Origin at the centre.

Two tones of wraps at many angles, so it reads as yarn from every side, and a short loose end curling off it.
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

RADIUS = 0.09
STRAND = 0.0065
# (tilt about X, turn about Z, cell): wraps spread over the sphere, alternating tones.
WRAPS = [
    (0.0, 0.0, "rose"),
    (math.pi / 2, 0.3, "pink"),
    (0.9, 1.2, "rose"),
    (-0.7, 2.1, "pink"),
    (1.3, 2.7, "rose"),
    (-1.2, 0.8, "pink"),
]
TAIL_SEGMENTS = 4


def torus(name, rotation):
    bpy.ops.mesh.primitive_torus_add(major_radius=RADIUS - STRAND, minor_radius=STRAND,
                                     major_segments=20, minor_segments=4, rotation=rotation)
    obj = bpy.context.view_layer.objects.active
    obj.name = name
    bpy.ops.object.transform_apply(rotation=True)
    shapes.smooth(obj)
    return obj


def tail():
    """The loose end: a few short strand pieces curling away from the surface."""
    pieces = []
    angle, lift = 0.35, 0.0
    start = (RADIUS * math.cos(angle), 0.0, RADIUS * math.sin(angle))
    for i in range(TAIL_SEGMENTS):
        angle -= 0.22
        lift += 0.012
        r = RADIUS + lift
        end = (r * math.cos(angle), 0.0, r * math.sin(angle) - 0.004 * i)
        dx, dz = end[0] - start[0], end[2] - start[2]
        piece = shapes.rounded_cylinder(f"Tail{i}", STRAND, math.hypot(dx, dz), bevel=0.0, verts=6)
        piece.rotation_euler = (0, math.atan2(dx, dz), 0)
        piece.location = start
        pieces.append(piece)
        start = end
    return pieces


def build(out):
    export.reset()
    core = shapes.uv_sphere("Core", RADIUS - STRAND, segments=16, rings=10)
    atlas.paint(core, CELLS["pink"], height_range=(-RADIUS, RADIUS))
    parts = [core]
    for i, (rx, rz, cell) in enumerate(WRAPS):
        strand = torus(f"Strand{i}", (rx, 0, rz))
        atlas.paint(strand, CELLS[cell], height_range=(-RADIUS, RADIUS))
        parts.append(strand)
    loose = tail()
    bpy.context.view_layer.update()
    for piece in loose:
        bpy.context.view_layer.objects.active = piece
        piece.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    for piece in loose:
        atlas.paint(piece, CELLS["rose"], height_range=(-RADIUS, RADIUS))
    yarn = shapes.join("YarnBall", parts + loose)
    export.fbx([yarn], os.path.join(out, "YarnBall.fbx"))


if __name__ == "__main__":
    build(export.args().out)
