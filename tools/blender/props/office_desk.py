"""The play desk: a 2.0 x 1.5 top whose surface is at 1.0 m (matches the M1 desk collider), four chunky legs."""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

TOP = (2.0, 1.5, 0.08)
SURFACE_Z = 1.0
LEG = 0.07
LEG_INSET = 0.08


def build(out):
    export.reset()
    top = shapes.rounded_box("Top", TOP, bevel=0.02, segments=2)
    top.location.z = SURFACE_Z - TOP[2]
    atlas.paint(top, CELLS["tan"])
    parts = [top]
    leg_h = SURFACE_Z - TOP[2]
    for sx in (-1, 1):
        for sy in (-1, 1):
            leg = shapes.rounded_box("Leg", (LEG, LEG, leg_h), bevel=0.012, segments=1)
            leg.location = (sx * (TOP[0] / 2 - LEG_INSET), sy * (TOP[1] / 2 - LEG_INSET), 0)
            atlas.paint(leg, CELLS["terracotta"])
            parts.append(leg)
    desk = shapes.join("OfficeDesk", parts)
    export.fbx([desk], os.path.join(out, "OfficeDesk.fbx"))


if __name__ == "__main__":
    build(export.args().out)
