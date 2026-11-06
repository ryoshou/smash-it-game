"""The play desk looks: a 2.0 x 1.5 top whose surface is at 1.0 m (matches the M1 desk collider), four chunky legs.

One look per room (spec §6.1: each room has its own desk style); the shape and the top height never change, so play
is the same on every desk. Library adds an inlay panel flush on the top; the Den adds an edge band under its lip.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

TOP = (2.0, 1.5, 0.08)
SURFACE_Z = 1.0
LEG = 0.07
LEG_INSET = 0.08
INLAY_INSET = 0.18
INLAY_T = 0.004
BAND_H = 0.035
BAND_OUT = 0.004

# name: (top cell, leg cell, inlay cell or None, edge band cell or None)
STYLES = {
    "OfficeDesk": ("tan", "terracotta", None, None),
    "CafeDesk": ("butter", "cream", None, None),
    "LibraryDesk": ("brown", "brown", "pine", None),
    "DenDesk": ("charcoal", "gray_dark", None, "periwinkle"),
    "KitchenDesk": ("tan", "white", None, None),  # farmhouse table: oak top, painted legs
}


def desk(name, top_cell, leg_cell, inlay_cell, band_cell):
    top = shapes.rounded_box("Top", TOP, bevel=0.02, segments=2)
    top.location.z = SURFACE_Z - TOP[2]
    atlas.paint(top, CELLS[top_cell])
    parts = [top]
    leg_h = SURFACE_Z - TOP[2]
    for sx in (-1, 1):
        for sy in (-1, 1):
            leg = shapes.rounded_box("Leg", (LEG, LEG, leg_h), bevel=0.012, segments=1)
            leg.location = (sx * (TOP[0] / 2 - LEG_INSET), sy * (TOP[1] / 2 - LEG_INSET), 0)
            atlas.paint(leg, CELLS[leg_cell])
            parts.append(leg)
    if inlay_cell:
        size = (TOP[0] - 2 * INLAY_INSET, TOP[1] - 2 * INLAY_INSET, INLAY_T)
        inlay = shapes.rounded_box("Inlay", size, bevel=0.0015, segments=1)
        inlay.location.z = SURFACE_Z - INLAY_T / 2
        atlas.paint(inlay, CELLS[inlay_cell])
        parts.append(inlay)
    if band_cell:
        size = (TOP[0] + 2 * BAND_OUT, TOP[1] + 2 * BAND_OUT, BAND_H)
        band = shapes.rounded_box("Band", size, bevel=0.008, segments=1)
        band.location.z = SURFACE_Z - TOP[2] - 0.005
        atlas.paint(band, CELLS[band_cell])
        parts.append(band)
    return shapes.join(name, parts)


def build(out):
    for name, cells in STYLES.items():
        export.reset()
        export.fbx([desk(name, *cells)], os.path.join(out, f"{name}.fbx"))


if __name__ == "__main__":
    build(export.args().out)
