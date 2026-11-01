import os
import unittest

import bmesh
import bpy

from lib import atlas, fracture, palette, shapes

PACKS = os.path.join(os.path.dirname(palette.ATLAS_PATH), "..")


def fresh_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def volume(obj):
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    v = abs(bm.calc_volume())
    bm.free()
    return v


def cylinder():
    obj = shapes.rounded_cylinder("Cyl", 0.15, 0.3, bevel=0.02, segments=2, verts=16)
    atlas.paint(obj, palette.CELLS["pink"])
    return obj


class FractureTests(unittest.TestCase):
    def setUp(self):
        fresh_scene()

    def test_piece_count(self):
        pieces = fracture.shatter(cylinder(), pieces=8, seed=3)
        self.assertEqual(len(pieces), 8)

    def test_deterministic(self):
        first = [len(p.data.vertices) for p in fracture.shatter(cylinder(), pieces=8, seed=3)]
        fresh_scene()
        second = [len(p.data.vertices) for p in fracture.shatter(cylinder(), pieces=8, seed=3)]
        self.assertEqual(first, second)

    def test_volume_conserved(self):
        source = cylinder()
        original = volume(source)
        pieces = fracture.shatter(source, pieces=8, seed=3)
        self.assertAlmostEqual(sum(volume(p) for p in pieces) / original, 1.0, delta=0.05)

    def test_pieces_keep_world_position(self):
        source = cylinder()
        pieces = fracture.shatter(source, pieces=6, seed=1)
        zs = [(p.matrix_world @ v.co).z for p in pieces for v in p.data.vertices]
        self.assertAlmostEqual(min(zs), 0.0, delta=0.005)
        self.assertAlmostEqual(max(zs), 0.3, delta=0.005)

    def test_fill_faces_in_light_end_of_dominant_cell(self):
        pieces = fracture.shatter(cylinder(), pieces=6, seed=2)
        u0, v0, u1, v1 = atlas.cell_rect(*palette.CELLS["pink"])
        top_band = v1 - (v1 - v0) * fracture.FILL_BAND
        fills = 0
        for p in pieces:
            fill_layer = p.data.attributes.get(fracture.FILL_ATTRIBUTE)
            uv = p.data.uv_layers.active.data
            for poly in p.data.polygons:
                if not fill_layer.data[poly.index].value:
                    continue
                fills += 1
                for li in poly.loop_indices:
                    self.assertTrue(u0 < uv[li].uv.x < u1 and top_band <= uv[li].uv.y < v1, tuple(uv[li].uv))
        self.assertGreater(fills, 0)

    def test_mug_set_under_budget(self):
        mug = fracture.import_pack(os.path.join(PACKS, "TinyTreats", "mug_A_pink.fbx"))
        pieces = fracture.shatter(mug, pieces=8, seed=7)
        self.assertEqual(len(pieces), 8)
        self.assertLessEqual(sum(shapes.triangles(p) for p in pieces), 1500)


if __name__ == "__main__":
    unittest.main()
