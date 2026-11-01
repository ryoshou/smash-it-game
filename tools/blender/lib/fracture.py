"""Pre-fracture by scripted cuts (no Cell Fracture add-on): recursive plane bisects with filled cut faces.

Deterministic for a seed. Cut faces are tagged with the FILL_ATTRIBUTE face attribute and painted in the light end
(top band) of the object's dominant atlas cell, so broken faces read as a lighter shade of the object's own colour.
"""
import random
from collections import Counter

import bmesh
import bpy
from mathutils import Vector

from lib import atlas, shapes
from lib.palette import GRID

FILL_ATTRIBUTE = "fracture_fill"
FILL_BAND = 0.25          # top fraction of the cell used by cut faces
MIN_PART_SHARE = 0.06     # a cut is rejected when either half keeps less than this share of the parent's bbox volume
MAX_ATTEMPTS_PER_PIECE = 40
MERGE_DISTANCE = 1e-5


def import_pack(path):
    """Imports a pack fbx, bakes its axis rotation, merges duplicate verts and returns one mesh object (Z up)."""
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path)
    meshes = [o for o in bpy.data.objects if o not in before and o.type == 'MESH']
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj = shapes.join(meshes[0].name, meshes) if len(meshes) > 1 else meshes[0]
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=MERGE_DISTANCE)
    bm.to_mesh(obj.data)
    bm.free()
    return obj


def dominant_cell(obj, grid=GRID):
    uv = obj.data.uv_layers.active.data
    cols, rows = grid
    cells = Counter((min(int(l.uv.x * cols), cols - 1), min(int(l.uv.y * rows), rows - 1)) for l in uv)
    return cells.most_common(1)[0][0]


def shatter(obj, pieces, seed, name_prefix="Piece"):
    """Cuts obj into `pieces` objects (each origin at its own vertex centroid, placed where it was). Removes obj."""
    rng = random.Random(seed)
    cell = dominant_cell(obj)
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bm.transform(obj.matrix_world)
    bm.faces.layers.int.new(FILL_ATTRIBUTE)
    parts = [bm]
    attempts = 0
    while len(parts) < pieces and attempts < pieces * MAX_ATTEMPTS_PER_PIECE:
        attempts += 1
        parts.sort(key=_bbox_volume, reverse=True)
        halves = _try_cut(parts[0], rng)
        if halves:
            parts[0].free()
            parts[0:1] = list(halves)
    if len(parts) < pieces:
        raise RuntimeError(f"could only cut {obj.name} into {len(parts)} of {pieces} pieces")
    bpy.data.objects.remove(obj)
    result = [_to_object(f"{name_prefix}_{i:02d}", part, cell) for i, part in enumerate(parts)]
    bpy.context.view_layer.update()
    return result


def _try_cut(bm, rng):
    center = sum((v.co for v in bm.verts), Vector()) / len(bm.verts)
    normal = Vector((rng.gauss(0, 1), rng.gauss(0, 1), rng.gauss(0, 1)))
    if normal.length < 1e-6:
        return None
    normal.normalize()
    keep_below = _half(bm, center, normal, clear_outer=True)
    keep_above = _half(bm, center, normal, clear_outer=False)
    parent = _bbox_volume(bm)
    for half in (keep_below, keep_above):
        if len(half.verts) < 4 or _bbox_volume(half) < parent * MIN_PART_SHARE:
            keep_below.free()
            keep_above.free()
            return None
    return keep_below, keep_above


def _half(bm, co, no, clear_outer):
    half = bm.copy()
    fill_layer = half.faces.layers.int[FILL_ATTRIBUTE]
    cut = bmesh.ops.bisect_plane(half, geom=half.verts[:] + half.edges[:] + half.faces[:], dist=1e-6,
                                 plane_co=co, plane_no=no, clear_outer=clear_outer, clear_inner=not clear_outer)
    edges = [e for e in cut["geom_cut"] if isinstance(e, bmesh.types.BMEdge)]
    if edges:
        filled = bmesh.ops.triangle_fill(half, use_beauty=True, use_dissolve=False, edges=edges,
                                         normal=no if clear_outer else -no)
        for face in filled["geom"]:
            if isinstance(face, bmesh.types.BMFace):
                face[fill_layer] = 1
    bmesh.ops.recalc_face_normals(half, faces=half.faces[:])
    return half


def _bbox_volume(bm):
    if not bm.verts:
        return 0.0
    lo = Vector((min(v.co.x for v in bm.verts), min(v.co.y for v in bm.verts), min(v.co.z for v in bm.verts)))
    hi = Vector((max(v.co.x for v in bm.verts), max(v.co.y for v in bm.verts), max(v.co.z for v in bm.verts)))
    size = hi - lo
    return size.x * size.y * size.z


def _to_object(name, bm, cell):
    centroid = sum((v.co for v in bm.verts), Vector()) / len(bm.verts)
    bmesh.ops.translate(bm, vec=-centroid, verts=bm.verts)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.location = centroid
    shapes.smooth(obj)
    _paint_fill(obj, cell)
    return obj


def _paint_fill(obj, cell):
    fill = obj.data.attributes[FILL_ATTRIBUTE].data
    u0, v0, u1, v1 = atlas.cell_rect(*cell)
    band_lo = v1 - (v1 - v0) * FILL_BAND
    pad = (v1 - v0) * 0.02
    cu = (u0 + u1) / 2
    uv = obj.data.uv_layers.active.data
    for poly in obj.data.polygons:
        if fill[poly.index].value:
            poly.use_smooth = False
            for k, li in enumerate(poly.loop_indices):
                uv[li].uv = (cu + (u1 - u0) * 0.05 * ((k % 3) - 1), band_lo + pad + (v1 - pad - band_lo - pad) * 0.5)
