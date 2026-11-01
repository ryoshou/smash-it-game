"""UV painting onto the shared atlas: every face lands inside one cell, v follows height (top light, bottom dark)."""
import bmesh

from lib.palette import GRID

U_SPREAD = 0.2  # fraction of the cell width a face's island may use, centred


def cell_rect(col, row, grid=GRID):
    cols, rows = grid
    return (col / cols, row / rows, (col + 1) / cols, (row + 1) / rows)


def paint(obj, cell, inset=0.15, face_filter=None, height_range=None):
    """Puts UVs of obj's faces (all, or those face_filter(polygon) accepts) inside `cell`.

    v runs from the cell's dark bottom to its light top by each vertex's height within `height_range`
    (default: the object's own z range), so tops come out light and bottoms dark.
    """
    mesh = obj.data
    if not mesh.uv_layers:
        mesh.uv_layers.new(name="UVMap")
    u0, v0, u1, v1 = cell_rect(*cell)
    zmin, zmax = height_range or _z_range(mesh)
    span = max(zmax - zmin, 1e-6)
    cu = (u0 + u1) / 2
    half_u = (u1 - u0) * U_SPREAD / 2
    lo = v0 + (v1 - v0) * inset
    hi = v1 - (v1 - v0) * inset
    uv_data = mesh.uv_layers.active.data
    for poly in mesh.polygons:
        if face_filter and not face_filter(poly):
            continue
        for k, li in enumerate(poly.loop_indices):
            z = mesh.vertices[mesh.loops[li].vertex_index].co.z
            t = min(max((z - zmin) / span, 0.0), 1.0)
            u = cu + half_u * (((k % 3) - 1) * 0.5)
            uv_data[li].uv = (u, lo + (hi - lo) * t)


def _z_range(mesh):
    zs = [v.co.z for v in mesh.vertices]
    return (min(zs), max(zs)) if zs else (0.0, 1.0)
