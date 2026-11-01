"""Turns an AI-generated cat (glb from the owner's tool run, untrusted data) into Generated/Cat.fbx (spec §12.3).

1. import, bake transforms, join; assumes the model faces glTF +Z (Blender -Y), like the blockout
2. decimate to the cat budget
3. sample the AI texture at each face's UV centroid → nearest atlas cell; re-UV each face into its cell by height
4. drop the AI material (the atlas material is assigned in Unity), smooth shading, scale to the blockout's height
5. rig from a landmarks JSON with automatic weights, add the five takes, export like build_cat

Usage: build.sh cat-ai  (newest download-assets/ai/cat/*/cat.glb, landmarks_ai.json)
"""
import argparse
import glob
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))

import bpy  # noqa: E402

from cat import actions, rig  # noqa: E402
from lib import atlas, colors, export, palette, shapes  # noqa: E402

ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
TRIANGLE_BUDGET = 7500  # under the 8,000 cat budget, leaving room for rounding in the decimator
TARGET_HEIGHT = 0.8     # the blockout's height (ear tips)
GLASS_CELLS = ((6, 0), (7, 0))


def newest_glb():
    found = sorted(glob.glob(os.path.join(ROOT, "download-assets", "ai", "cat", "*", "cat.glb")))
    if not found:
        raise FileNotFoundError("no download-assets/ai/cat/<date>/cat.glb yet (the owner's Meshy export)")
    return found[-1]


def prepare(glb, landmarks):
    """Imports and cleans the mesh, rigs it; returns (mesh, armature). Leaves the scene ready to export."""
    export.reset()
    bpy.context.scene.render.fps = actions.FPS
    mesh = _import(glb)
    _decimate(mesh)
    cells = _sample_cells(mesh)
    _repaint(mesh, cells)
    mesh.data.materials.clear()
    shapes.smooth(mesh)
    _fit(mesh)
    armature = rig.armature(landmarks)
    bpy.ops.object.select_all(action='DESELECT')
    mesh.select_set(True)
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')
    actions.build(armature)
    return mesh, armature


def _import(glb):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=glb)
    meshes = [o for o in bpy.data.objects if o not in before and o.type == 'MESH']
    for o in [o for o in bpy.data.objects if o not in before and o.type != 'MESH']:
        bpy.data.objects.remove(o)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes:
        o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return shapes.join("Cat", meshes) if len(meshes) > 1 else meshes[0]


def _decimate(mesh):
    tris = shapes.triangles(mesh)
    if tris <= TRIANGLE_BUDGET:
        return
    modifier = mesh.modifiers.new("Decimate", 'DECIMATE')
    modifier.ratio = TRIANGLE_BUDGET / tris
    bpy.context.view_layer.objects.active = mesh
    bpy.ops.object.modifier_apply(modifier=modifier.name)


def _texture(mesh):
    for material in mesh.data.materials:
        if material and material.node_tree:
            for node in material.node_tree.nodes:
                if node.type == 'TEX_IMAGE' and node.image:
                    return node.image
    raise ValueError("the AI mesh has no base-colour texture to sample")


def _sample_cells(mesh):
    image = _texture(mesh)
    width, height = image.size
    pixels = image.pixels[:]
    atlas_image = bpy.data.images.load(palette.ATLAS_PATH)
    cells_palette = colors.atlas_palette(atlas_image, exclude=GLASS_CELLS)
    uv = mesh.data.uv_layers.active.data
    cells = {}
    for poly in mesh.data.polygons:
        u = sum(uv[li].uv.x for li in poly.loop_indices) / poly.loop_total
        v = sum(uv[li].uv.y for li in poly.loop_indices) / poly.loop_total
        x = min(max(int((u % 1.0) * width), 0), width - 1)
        y = min(max(int((v % 1.0) * height), 0), height - 1)
        i = (y * width + x) * 4
        cells[poly.index] = colors.nearest_cell((pixels[i], pixels[i + 1], pixels[i + 2]), cells_palette)
    return cells


def _repaint(mesh, cells):
    zs = [v.co.z for v in mesh.data.vertices]
    height_range = (min(zs), max(zs))
    for cell in set(cells.values()):
        atlas.paint(mesh, cell, height_range=height_range, face_filter=lambda p, c=cell: cells[p.index] == c)


def _fit(mesh):
    zs = [v.co.z for v in mesh.data.vertices]
    xs = [v.co.x for v in mesh.data.vertices]
    ys = [v.co.y for v in mesh.data.vertices]
    scale = TARGET_HEIGHT / (max(zs) - min(zs))
    cx, cy, base = (min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2, min(zs)
    for v in mesh.data.vertices:
        v.co.x = (v.co.x - cx) * scale
        v.co.y = (v.co.y - cy) * scale
        v.co.z = (v.co.z - base) * scale


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--glb")
    parser.add_argument("--landmarks", default=os.path.join(HERE, "landmarks_ai.json"))
    args = parser.parse_args(argv)
    _, armature = prepare(args.glb or newest_glb(), args.landmarks)
    export.fbx([armature], os.path.join(args.out, "Cat.fbx"), with_animation=True, authored=False)


if __name__ == "__main__":
    main()
