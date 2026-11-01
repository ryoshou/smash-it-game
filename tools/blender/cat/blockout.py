"""Sitting chunky cat from primitives (spec §12.3 blockout): orange tabby, cream muzzle/chest/paws, pink ears/nose.

Blender Z up, origin at the base centre, face towards -Y. Each part carries the name of the bone that moves it
(rigid skinning, see rig.py), so the parts can stay separate islands.
"""
import math

import bmesh
import bpy
from mathutils import Matrix, Vector

from lib import atlas, shapes
from lib.palette import CELLS

HEIGHT_RANGE = (0.0, 0.8)


def _ellipsoid(name, radii, center, cell, segments=16, rings=10):
    obj = shapes.uv_sphere(name, 1.0, segments=segments, rings=rings)
    obj.scale = radii
    obj.location = center
    _apply(obj)
    atlas.paint(obj, CELLS[cell], height_range=HEIGHT_RANGE)
    return obj


def _cone(name, radius, depth, center, tilt, cell):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=8, radius1=radius, radius2=radius * 0.15, depth=depth)
    obj = shapes._object(name, bm)
    obj.rotation_euler = tilt
    obj.location = center
    _apply(obj)
    atlas.paint(obj, CELLS[cell], height_range=HEIGHT_RANGE)
    return obj


def _limb(name, start, end, radius, cell):
    start, end = Vector(start), Vector(end)
    axis = end - start
    obj = shapes.rounded_cylinder(name, radius, axis.length, bevel=radius * 0.4, segments=1, verts=10, base_at_origin=True)
    obj.matrix_world = Matrix.Translation(start) @ axis.to_track_quat('Z', 'Y').to_matrix().to_4x4()
    _apply(obj)
    atlas.paint(obj, CELLS[cell], height_range=HEIGHT_RANGE)
    return obj


def _apply(obj):
    bpy.context.view_layer.update()
    obj.data.transform(obj.matrix_world)
    obj.matrix_world = Matrix.Identity(4)


def build():
    """Returns {bone name: [part objects]}."""
    parts = {
        "spine": [_ellipsoid("Body", (0.2, 0.19, 0.22), (0, 0.03, 0.22), "orange"),
                  _ellipsoid("HaunchL", (0.1, 0.13, 0.1), (0.13, 0.06, 0.09), "orange"),
                  _ellipsoid("HaunchR", (0.1, 0.13, 0.1), (-0.13, 0.06, 0.09), "orange")],
        "chest": [_ellipsoid("Chest", (0.13, 0.08, 0.15), (0, -0.11, 0.3), "butter")],
        "head": [_ellipsoid("Head", (0.17, 0.15, 0.14), (0, -0.05, 0.58), "orange", 18, 12),
                 _ellipsoid("Muzzle", (0.08, 0.05, 0.05), (0, -0.18, 0.54), "butter"),
                 _ellipsoid("EyeL", (0.025, 0.012, 0.032), (0.065, -0.185, 0.6), "charcoal", 8, 6),
                 _ellipsoid("EyeR", (0.025, 0.012, 0.032), (-0.065, -0.185, 0.6), "charcoal", 8, 6),
                 _ellipsoid("Nose", (0.018, 0.012, 0.012), (0, -0.23, 0.56), "pink", 8, 6)],
        "ear.L": [_cone("EarL", 0.055, 0.12, (0.1, -0.03, 0.72), (0, math.radians(20), 0), "orange"),
                  _cone("EarInL", 0.03, 0.08, (0.098, -0.06, 0.71), (0, math.radians(20), 0), "pink")],
        "ear.R": [_cone("EarR", 0.055, 0.12, (-0.1, -0.03, 0.72), (0, math.radians(-20), 0), "orange"),
                  _cone("EarInR", 0.03, 0.08, (-0.098, -0.06, 0.71), (0, math.radians(-20), 0), "pink")],
        "arm.L": [_limb("ArmL", (0.09, -0.1, 0.34), (0.09, -0.12, 0.2), 0.045, "orange")],
        "forearm.L": [_limb("ForearmL", (0.09, -0.12, 0.2), (0.09, -0.13, 0.06), 0.04, "orange")],
        "paw.L": [_ellipsoid("PawL", (0.05, 0.065, 0.035), (0.09, -0.16, 0.035), "butter")],
        "arm.R": [_limb("ArmR", (-0.09, -0.1, 0.34), (-0.09, -0.12, 0.2), 0.045, "orange")],
        "forearm.R": [_limb("ForearmR", (-0.09, -0.12, 0.2), (-0.09, -0.13, 0.06), 0.04, "orange")],
        "paw.R": [_ellipsoid("PawR", (0.05, 0.065, 0.035), (-0.09, -0.16, 0.035), "butter")],
    }
    tail = [(0, 0.17, 0.06), (0, 0.27, 0.08), (0, 0.33, 0.16), (0, 0.35, 0.26), (0, 0.33, 0.36)]
    for i in range(4):
        parts[f"tail.{i + 1}"] = [_limb(f"Tail{i + 1}", tail[i], tail[i + 1], 0.035 - i * 0.004, "orange" if i < 3 else "butter")]
    return parts
