"""Y-frame slingshot. Origin = the M1 slingshot root; empties BandAnchorL/R sit at the M1 fork tips (x = +/-0.13, up 0.192).

A chunky wooden fork: a tapered handle with a wrapped grip, arms that curve out and then up, round tips with the rubber
bands' red wraps just below them. Only the look changed from M1: the tips (band anchors) are where they always were.
"""
import math
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import bmesh  # noqa: E402
import bpy  # noqa: E402

from lib import atlas, export, shapes  # noqa: E402
from lib.palette import CELLS  # noqa: E402

FORK_HALF_WIDTH = 0.13
TIP_Z = 0.192
HANDLE_LENGTH = 0.35
ELBOW = (0.085, 0.075)  # where each arm bends from outwards to upwards (x, z)
HANDLE_RADIUS = 0.024
ARM_RADIUS = 0.02
TIP_RADIUS = 0.016
GRIP_ROWS = tuple(-0.31 + 0.027 * i for i in range(6))  # touching turns of cord
WRAP_SHARE = 0.8  # the band wrap sits this far up the upper arm
VERTS = 10


def limb(name, start, end, radius_start, radius_end):
    """A tapered round post from start to end, in the XZ plane."""
    dx, dz = end[0] - start[0], end[2] - start[2]
    length = math.hypot(dx, dz)
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=VERTS,
                          radius1=radius_start, radius2=radius_end, depth=length)
    bmesh.ops.translate(bm, vec=(0, 0, length / 2), verts=bm.verts)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    shapes.smooth(obj)
    obj.rotation_euler = (0, math.atan2(dx, dz), 0)
    obj.location = start
    return obj


def ball(name, radius, at):
    obj = shapes.uv_sphere(name, radius, segments=VERTS, rings=6)
    obj.location = at
    return obj


def ring(name, major, minor, at, tilt=0.0):
    """A torus around a post whose axis leans `tilt` radians from vertical in the XZ plane."""
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=10, minor_segments=4,
                                     location=at, rotation=(0, tilt, 0))
    obj = bpy.context.view_layer.objects.active
    obj.name = name
    shapes.smooth(obj)
    return obj


def build(out):
    export.reset()
    wood, grip, band = [], [], []
    wood.append(limb("Handle", (0, 0, -HANDLE_LENGTH), (0, 0, 0), HANDLE_RADIUS * 1.08, HANDLE_RADIUS * 0.92))
    wood.append(ball("Butt", HANDLE_RADIUS * 1.1, (0, 0, -HANDLE_LENGTH)))
    wood.append(ball("Crotch", HANDLE_RADIUS, (0, 0, 0)))
    for side, sign in (("L", -1), ("R", 1)):
        elbow = (sign * ELBOW[0], 0, ELBOW[1])
        tip = (sign * FORK_HALF_WIDTH, 0, TIP_Z)
        wood.append(limb("Lower" + side, (0, 0, 0), elbow, ARM_RADIUS, ARM_RADIUS * 0.95))
        wood.append(ball("Elbow" + side, ARM_RADIUS * 0.95, elbow))
        wood.append(limb("Upper" + side, elbow, tip, ARM_RADIUS * 0.95, TIP_RADIUS))
        wood.append(ball("Tip" + side, TIP_RADIUS * 1.35, tip))
        wrap_at = tuple(e + (t - e) * WRAP_SHARE for e, t in zip(elbow, tip))
        tilt = math.atan2(tip[0] - elbow[0], tip[2] - elbow[2])
        band.append(ring("Wrap" + side, TIP_RADIUS * 1.15, 0.0055, wrap_at, tilt))
    for i, z in enumerate(GRIP_ROWS):
        grip.append(ring(f"Grip{i}", HANDLE_RADIUS * 1.0, 0.0085, (0, 0, z)))

    parts = wood + grip + band
    bpy.context.view_layer.update()
    for p in parts:
        bpy.context.view_layer.objects.active = p
        p.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    height = (-HANDLE_LENGTH - HANDLE_RADIUS, TIP_Z + TIP_RADIUS)
    for p in wood:
        atlas.paint(p, CELLS["tan"], height_range=height)
    for p in grip:
        atlas.paint(p, CELLS["brown"], height_range=height)
    for p in band:
        atlas.paint(p, CELLS["red"], height_range=height)
    frame = shapes.join("Slingshot", parts)
    for side, x in (("L", -FORK_HALF_WIDTH), ("R", FORK_HALF_WIDTH)):
        anchor = bpy.data.objects.new("BandAnchor" + side, None)
        bpy.context.scene.collection.objects.link(anchor)
        anchor.location = (x, 0, TIP_Z)
        anchor.parent = frame
    export.fbx([frame], os.path.join(out, "Slingshot.fbx"))


if __name__ == "__main__":
    build(export.args().out)
