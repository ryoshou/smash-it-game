"""Chunky primitives with soft bevels and smooth shading (spec §12.2). Sizes in metres, Z up, origin at the base centre."""
import bmesh
import bpy


def _object(name, bm):
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    smooth(obj)
    return obj


def smooth(obj):
    for poly in obj.data.polygons:
        poly.use_smooth = True


def rounded_box(name, size, bevel=0.01, segments=2, base_at_origin=True):
    sx, sy, sz = size
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    bmesh.ops.scale(bm, vec=(sx, sy, sz), verts=bm.verts)
    if base_at_origin:
        bmesh.ops.translate(bm, vec=(0, 0, sz / 2), verts=bm.verts)
    _bevel(bm, bevel, segments)
    return _object(name, bm)


def rounded_cylinder(name, radius, depth, bevel=0.01, segments=2, verts=16, base_at_origin=True):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=verts,
                          radius1=radius, radius2=radius, depth=depth)
    if base_at_origin:
        bmesh.ops.translate(bm, vec=(0, 0, depth / 2), verts=bm.verts)
    _bevel(bm, bevel, segments, only_sharp=True)
    return _object(name, bm)


def uv_sphere(name, radius, segments=16, rings=10):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=segments, v_segments=rings, radius=radius)
    return _object(name, bm)


def _bevel(bm, offset, segments, only_sharp=False):
    if offset <= 0:
        return
    edges = bm.edges[:]
    if only_sharp:
        edges = [e for e in edges if e.calc_face_angle(0) > 0.6]
    bmesh.ops.bevel(bm, geom=edges, offset=offset, segments=segments, profile=0.5, affect='EDGES', clamp_overlap=True)


def join(name, objects):
    """Joins objects into the first, renamed `name`."""
    bpy.ops.object.select_all(action='DESELECT')
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    joined = bpy.context.view_layer.objects.active
    joined.name = name
    joined.data.name = name
    return joined


def triangles(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)
