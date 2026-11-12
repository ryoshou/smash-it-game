"""The ball: diameter 0.18 (2 x ShotTuning.ballRadius), origin at the centre.

A simple smooth pink ball for now (owner, 2026-11-12): no licensed yarn model was found; it is replaced when one is.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

RADIUS = 0.09


def build(out):
    export.reset()
    ball = shapes.uv_sphere("YarnBall", RADIUS, segments=20, rings=12)
    atlas.paint(ball, CELLS["pink"], height_range=(-RADIUS, RADIUS))
    export.fbx([ball], os.path.join(out, "YarnBall.fbx"))


if __name__ == "__main__":
    build(export.args().out)
