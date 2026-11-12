"""Y-frame slingshot. Origin = the M1 slingshot root; empties BandAnchorL/R sit at the M1 fork tips (x = +/-0.13, up 0.192).

One carved piece of wood (the owner, 2026-11-12: "monolithic, not assembled pieces"): a skin over a skeleton from the
butt up the handle, through the crotch and out along each arm, subdivided so the crotch and elbows blend with no
joints. The cord grip is gentle ridges in the same skin and the band wraps are a slightly sunk band near each tip,
both painted onto that one mesh. Only the look changed from M1: the tips (band anchors) are where they always were.
"""
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
ELBOW = (0.085, 0.075)  # where each arm bends from outwards to upwards (x, z)
BUTT_RADIUS = 0.027
HANDLE_RADIUS = 0.024
ARM_RADIUS = 0.02
TIP_RADIUS = 0.018
GRIP = (-0.31, -0.16)  # the cord-wrapped part of the handle (z)
GRIP_TURNS = 6
GRIP_RIDGE = 0.003  # how far each turn of cord stands proud of the grooves between
WRAP = (0.72, 0.88)  # the band wrap's share of the way from elbow to tip
WRAP_SINK = 0.0015
TRIANGLE_BUDGET = 1400
SUBDIVISIONS = 1  # ~1,100 triangles; level 2 needs decimating, which leaves ragged paint edges


def skeleton():
    """(points, radii, edges): the handle bottom-up, then each arm from the crotch to its tip."""
    points, radii, edges = [(0.0, 0.0, -HANDLE_LENGTH)], [BUTT_RADIUS], []

    def add(point, radius, after):
        points.append(point)
        radii.append(radius)
        edges.append((after, len(points) - 1))
        return len(points) - 1

    last = 0
    bottom, top = GRIP
    steps = GRIP_TURNS * 2
    for i in range(steps + 1):
        z = bottom + (top - bottom) * i / steps
        ridge = GRIP_RIDGE if i % 2 == 1 else 0.0
        last = add((0.0, 0.0, z), HANDLE_RADIUS + ridge, last)
    crotch = add((0.0, 0.0, 0.0), HANDLE_RADIUS * 0.95, last)
    for sign in (-1, 1):
        elbow = (sign * ELBOW[0], 0.0, ELBOW[1])
        tip = (sign * FORK_HALF_WIDTH, 0.0, TIP_Z)
        at = add(elbow, ARM_RADIUS, crotch)
        for share, sink in ((WRAP[0], 0.0), ((WRAP[0] + WRAP[1]) / 2, WRAP_SINK), (WRAP[1], 0.0)):
            point = tuple(e + (t - e) * share for e, t in zip(elbow, tip))
            radius = ARM_RADIUS + (TIP_RADIUS - ARM_RADIUS) * share - sink
            at = add(point, radius, at)
        add(tip, TIP_RADIUS, at)
    return points, radii, edges


def carve(points, radii, edges):
    mesh = bpy.data.meshes.new("Slingshot")
    mesh.from_pydata(points, edges, [])
    obj = bpy.data.objects.new("Slingshot", mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.modifiers.new("Skin", "SKIN")
    for i, radius in enumerate(radii):
        mesh.skin_vertices[0].data[i].radius = (radius, radius)
    mesh.skin_vertices[0].data[0].use_root = True
    obj.modifiers.new("Smooth", "SUBSURF").levels = SUBDIVISIONS
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    for name in ("Skin", "Smooth"):
        bpy.ops.object.modifier_apply(modifier=name)
    tris = shapes.triangles(obj)
    if tris > TRIANGLE_BUDGET:
        decimate = obj.modifiers.new("Budget", "DECIMATE")
        decimate.ratio = TRIANGLE_BUDGET / tris
        bpy.ops.object.modifier_apply(modifier="Budget")
    shapes.smooth(obj)
    return obj


def in_grip(poly):
    x, _, z = poly.center
    return abs(x) < HANDLE_RADIUS * 2 and GRIP[0] - 0.004 <= z <= GRIP[1] + 0.004


def in_wrap(poly):
    """Faces whose centre lies in the wrap's span of either upper arm."""
    x, _, z = poly.center
    for sign in (-1, 1):
        ex, ez = sign * ELBOW[0], ELBOW[1]
        dx, dz = sign * FORK_HALF_WIDTH - ex, TIP_Z - ez
        length2 = dx * dx + dz * dz
        share = ((x - ex) * dx + (z - ez) * dz) / length2
        across = abs((x - ex) * dz - (z - ez) * dx) / math.sqrt(length2)
        if WRAP[0] <= share <= WRAP[1] and across < ARM_RADIUS * 2:
            return True
    return False


def build(out):
    export.reset()
    frame = carve(*skeleton())
    height = (-HANDLE_LENGTH - BUTT_RADIUS, TIP_Z + TIP_RADIUS)
    atlas.paint(frame, CELLS["tan"], height_range=height)
    atlas.paint(frame, CELLS["brown"], height_range=height, face_filter=in_grip)
    atlas.paint(frame, CELLS["red"], height_range=height, face_filter=in_wrap)
    for side, x in (("L", -FORK_HALF_WIDTH), ("R", FORK_HALF_WIDTH)):
        anchor = bpy.data.objects.new("BandAnchor" + side, None)
        bpy.context.scene.collection.objects.link(anchor)
        anchor.location = (x, 0, TIP_Z)
        anchor.parent = frame
    export.fbx([frame], os.path.join(out, "Slingshot.fbx"))


if __name__ == "__main__":
    build(export.args().out)
