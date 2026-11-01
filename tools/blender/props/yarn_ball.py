"""Yarn ball visual: diameter 0.18 (2 x ShotTuning.ballRadius), wrapped strands. Origin at the centre."""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

RADIUS = 0.09
STRAND = 0.007


def build(out):
    export.reset()
    ball = shapes.uv_sphere("Core", RADIUS - STRAND, segments=16, rings=10)
    atlas.paint(ball, CELLS["pink"], height_range=(-RADIUS, RADIUS))
    parts = [ball]
    for i, (rx, ry) in enumerate([(0, 0), (math.pi / 2, 0.3), (0.9, 1.2), (-0.7, 2.1)]):
        bpy.ops.mesh.primitive_torus_add(major_radius=RADIUS - STRAND, minor_radius=STRAND,
                                         major_segments=20, minor_segments=4, rotation=(rx, ry, 0))
        strand = bpy.context.view_layer.objects.active
        strand.name = f"Strand{i}"
        bpy.ops.object.transform_apply(rotation=True)
        shapes.smooth(strand)
        atlas.paint(strand, CELLS["rose"], height_range=(-RADIUS, RADIUS))
        parts.append(strand)
    yarn = shapes.join("YarnBall", parts)
    export.fbx([yarn], os.path.join(out, "YarnBall.fbx"))


if __name__ == "__main__":
    build(export.args().out)
