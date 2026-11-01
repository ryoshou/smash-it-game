import unittest

import bmesh
import bpy

from lib import atlas, palette, shapes


def fresh_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def loop_uvs(obj, face_filter=None):
    mesh = obj.data
    uv_layer = mesh.uv_layers.active.data
    for poly in mesh.polygons:
        if face_filter and not face_filter(poly):
            continue
        for li in poly.loop_indices:
            yield poly, uv_layer[li].uv


class AtlasTests(unittest.TestCase):
    def setUp(self):
        fresh_scene()

    def test_cell_rect_bounds(self):
        self.assertEqual(atlas.cell_rect(0, 0), (0.0, 0.0, 0.125, 0.25))
        self.assertEqual(atlas.cell_rect(7, 3), (0.875, 0.75, 1.0, 1.0))

    def test_paint_keeps_every_uv_in_cell(self):
        obj = shapes.rounded_box("Box", (0.2, 0.2, 0.2), bevel=0.02, segments=2)
        atlas.paint(obj, (3, 1))
        u0, v0, u1, v1 = atlas.cell_rect(3, 1)
        for _, uv in loop_uvs(obj):
            self.assertTrue(u0 < uv.x < u1 and v0 < uv.y < v1, f"uv {tuple(uv)} outside cell")

    def test_paint_top_is_lighter(self):
        obj = shapes.rounded_box("Box", (0.2, 0.2, 0.2), bevel=0.02, segments=2)
        atlas.paint(obj, (3, 1))
        top = [uv.y for p, uv in loop_uvs(obj, lambda p: p.normal.z > 0.9)]
        bottom = [uv.y for p, uv in loop_uvs(obj, lambda p: p.normal.z < -0.9)]
        # The atlas png is light at the top of each cell; image top is UV v = 1.
        self.assertGreater(sum(top) / len(top), sum(bottom) / len(bottom))

    def test_atlas_png_cells_are_light_at_top(self):
        image = bpy.data.images.load(palette.ATLAS_PATH)
        width, height = image.size
        pixels = image.pixels[:]
        for col, row in [(0, 3), (3, 1), (6, 0)]:
            u0, v0, u1, v1 = atlas.cell_rect(col, row)
            x = int((u0 + u1) / 2 * width)
            top = luminance(pixels, width, x, int((v1 - 0.02) * height))
            bottom = luminance(pixels, width, x, int((v0 + 0.02) * height))
            self.assertGreater(top, bottom, f"cell {(col, row)}")

    def test_palette_cells_in_grid(self):
        for name, (col, row) in palette.CELLS.items():
            self.assertTrue(0 <= col < 8 and 0 <= row < 4, name)

    def test_paint_faces_overrides_only_selected(self):
        obj = shapes.rounded_box("Box", (0.2, 0.2, 0.2), bevel=0.02, segments=2)
        atlas.paint(obj, (3, 1))
        atlas.paint(obj, (0, 3), face_filter=lambda p: p.normal.z > 0.9)
        u0, _, u1, _ = atlas.cell_rect(0, 3)
        for p, uv in loop_uvs(obj):
            inside = u0 < uv.x < u1
            self.assertEqual(inside, p.normal.z > 0.9)


def luminance(pixels, width, x, y):
    i = (y * width + x) * 4
    return 0.2126 * pixels[i] + 0.7152 * pixels[i + 1] + 0.0722 * pixels[i + 2]


if __name__ == "__main__":
    unittest.main()
