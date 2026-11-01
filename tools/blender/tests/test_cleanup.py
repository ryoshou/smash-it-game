import os
import tempfile
import unittest

import bpy

from cat import blockout, cleanup
from lib import export, palette, shapes

HERE = os.path.dirname(os.path.abspath(__file__))
LANDMARKS = os.path.join(os.path.dirname(HERE), "cat", "landmarks_blockout.json")


def fake_ai_glb(path):
    """The blockout, textured with the atlas, exported as a glb: stands in for an AI tool's output."""
    export.reset()
    parts = [p for group in blockout.build().values() for p in group]
    mesh = shapes.join("AICat", parts)
    # AI textures don't follow our cell rule: grow each face's UV island around its centre so it straddles cells
    # (the colour at the centre, which cleanup samples, is unchanged).
    uv = mesh.data.uv_layers.active.data
    for poly in mesh.data.polygons:
        cu = sum(uv[li].uv.x for li in poly.loop_indices) / poly.loop_total
        cv = sum(uv[li].uv.y for li in poly.loop_indices) / poly.loop_total
        for li in poly.loop_indices:
            uv[li].uv = (cu + (uv[li].uv.x - cu) * 8 + 0.07 * (li % 2), cv + (uv[li].uv.y - cv) * 8 + 0.15 * (li % 2))
    material = bpy.data.materials.new("AITexture")
    tex = material.node_tree.nodes.new("ShaderNodeTexImage")
    tex.image = bpy.data.images.load(palette.ATLAS_PATH)
    material.node_tree.links.new(tex.outputs["Color"], material.node_tree.nodes["Principled BSDF"].inputs["Base Color"])
    mesh.data.materials.append(material)
    bpy.ops.object.select_all(action='DESELECT')
    mesh.select_set(True)
    bpy.ops.export_scene.gltf(filepath=path, export_format='GLB', use_selection=True)


class CleanupTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.tmp = tempfile.mkdtemp()
        cls.glb = os.path.join(cls.tmp, "cat.glb")
        fake_ai_glb(cls.glb)
        cls.mesh, cls.armature = cleanup.prepare(cls.glb, LANDMARKS)

    def test_under_cat_budget(self):
        self.assertLessEqual(shapes.triangles(self.mesh), 8000)

    def test_every_face_inside_one_atlas_cell(self):
        uv = self.mesh.data.uv_layers.active.data
        for poly in self.mesh.data.polygons:
            cells = {(int(uv[li].uv.x * 8), int(uv[li].uv.y * 4)) for li in poly.loop_indices}
            self.assertEqual(len(cells), 1, f"face {poly.index} spans {cells}")

    def test_scaled_to_blockout_height_on_the_floor(self):
        zs = [v.co.z for v in self.mesh.data.vertices]
        self.assertAlmostEqual(min(zs), 0.0, places=3)
        self.assertAlmostEqual(max(zs) - min(zs), cleanup.TARGET_HEIGHT, places=3)

    def test_rigged_with_the_shared_bones_and_five_takes(self):
        self.assertEqual(self.mesh.parent, self.armature)
        self.assertEqual(len(self.armature.data.bones), 17)
        for take in ("Idle", "Aim", "Release", "Celebrate", "Annoyed"):
            self.assertIn(take, bpy.data.actions)

    def test_orange_body_maps_to_orange_cell(self):
        uv = self.mesh.data.uv_layers.active.data
        cells = [(int(uv[li].uv.x * 8), int(uv[li].uv.y * 4)) for p in self.mesh.data.polygons for li in p.loop_indices]
        self.assertEqual(max(set(cells), key=cells.count), palette.CELLS["orange"])


if __name__ == "__main__":
    unittest.main()
